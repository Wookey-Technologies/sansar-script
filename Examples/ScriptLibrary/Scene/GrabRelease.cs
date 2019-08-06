/* This content is licensed under the terms of the Creative Commons Attribution 4.0 International License.
 * When using this content, you must:
 * •    Acknowledge that the content is from the Sansar Knowledge Base.
 * •    Include our copyright notice: "© 2017 Linden Research, Inc."
 * •    Indicate that the content is licensed under the Creative Commons Attribution-Share Alike 4.0 International License.
 * •    Include the URL for, or link to, the license summary at https://creativecommons.org/licenses/by-sa/4.0/deed.hi (and, if possible, to the complete license terms at https://creativecommons.org/licenses/by-sa/4.0/legalcode.
 * For example:
 * "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."
 */

using Sansar.Script;
using Sansar.Simulation;
using System;
using System.Linq;

namespace ScriptLibrary
{
    [Tooltip("Sends simple script events when the owning object is grabbed/released.")]
    [DisplayName("Grab/Release")]
    public class GrabRelease : LibraryBase
    {
        #region EditorProperties

        [Tooltip("The events to send when this object is grabbed by an avatar")]
        [DefaultValue("grabbed")]
        [DisplayName("Grabbed ->")]
        public readonly string GrabbedMessage;

        [Tooltip("The events to send when this object is released by an avatar")]
        [DefaultValue("released")]
        [DisplayName("Released ->")]
        public readonly string ReleasedMessage;

        [Tooltip("Whether to send messages when grabbed/released using the right wand or mouse button")]
        [DefaultValue(true)]
        [DisplayName("Send Right Hand")]
        public readonly bool SendRightHand;

        [Tooltip("Whether to send messages when grabbed/released using the left wand")]
        [DefaultValue(true)]
        [DisplayName("Send Left Hand")]
        public readonly bool SendLeftHand;

        [Tooltip("Set the object to be grabbable on these events. Can be a comma separated list of event names.")]
        [DefaultValue("grabbable_enable")]
        [DisplayName("-> Enable Grab")]
        public readonly string EnableGrabEvent;

        [Tooltip("Set the object to be NOT grabbable on these events. Can be a comma separated list of event names.")]
        [DefaultValue("grabbable_disable")]
        [DisplayName("-> Disable Grab")]
        public readonly string DisableGrabEvent;

        [Tooltip("Enable responding to events for this script")]
        [DefaultValue("grab_release_enable")]
        [DisplayName("-> Enable")]
        public readonly string EnableEvent;

        [Tooltip("Disable responding to events for this script")]
        [DefaultValue("grab_release_disable")]
        [DisplayName("-> Disable")]
        public readonly string DisableEvent;

        [Tooltip(@"If StartEnabled is true then the script will respond to interactions when the scene is loaded
If StartEnabled is false then the script will not respond to interactions until an (-> Enable) event is received.")]
        [DefaultValue(true)]
        [DisplayName("Start Enabled")]
        public readonly bool StartEnabled = true;
        #endregion

        Action unsubscribes = null;
        IEventSubscription grabSubscribe = null;
        IEventSubscription releaseSubscribe = null;
        RigidBodyComponent rigidBody;

        protected override void SimpleInit()
        {
            if (!ObjectPrivate.TryGetFirstComponent<RigidBodyComponent>(out rigidBody))
            {
                Log.Write(LogLevel.Error, __SimpleTag, "SimpleGrabRelease must be attached to an object with a RigidBodyComponent");
                return;
            }

            if (StartEnabled) Subscribe(null);

            SubscribeToAll(EnableEvent, Subscribe);
            SubscribeToAll(DisableEvent, Unsubscribe);
        }

        private void Subscribe(ScriptEventData data)
        {
            unsubscribes = SubscribeToAll(EnableGrabEvent, (ScriptEventData subdata) =>
            {
                rigidBody.SetCanGrab(true);
            });

            unsubscribes += SubscribeToAll(DisableGrabEvent, (ScriptEventData subData) =>
            {
                rigidBody.SetCanGrab(false);
            });

            if (!string.IsNullOrWhiteSpace(GrabbedMessage))
            {
                grabSubscribe = rigidBody.SubscribeToHeldObject(HeldObjectEventType.Grab, (HeldObjectData holdData) =>
                {
                    if (ShouldSend(holdData))
                    {
                        SendToAll(GrabbedMessage, GrabReleaseData(holdData));
                    }
                });
            }

            if (!string.IsNullOrWhiteSpace(ReleasedMessage))
            {
                releaseSubscribe = rigidBody.SubscribeToHeldObject(HeldObjectEventType.Release, (HeldObjectData holdData) =>
                {
                    if (ShouldSend(holdData))
                    {
                        SendToAll(ReleasedMessage, GrabReleaseData(holdData));
                    }
                });
            }
        }

        private void Unsubscribe(ScriptEventData data)
        {
            if (unsubscribes != null)
            {
                unsubscribes();
                unsubscribes = null;
            }
            if (grabSubscribe != null)
            {
                grabSubscribe.Unsubscribe();
            }
            if (releaseSubscribe != null)
            {
                releaseSubscribe.Unsubscribe();
            }
        }

        private bool ShouldSend(HeldObjectData holdData)
        {
            bool send = SendRightHand && holdData.HeldObjectInfo.ControlPoint == ControlPointType.RightTool || holdData.HeldObjectInfo.ControlPoint == ControlPointType.DesktopGrab;
            send |= SendLeftHand && holdData.HeldObjectInfo.ControlPoint == ControlPointType.LeftTool;

            return send;
        }

        private SimpleData GrabReleaseData(HeldObjectData holdData)
        {
            SimpleData sd = new SimpleData(this);

            sd.ObjectId = holdData.ObjectId;
            sd.SourceObjectId = ObjectPrivate.ObjectId; // expected to equal sd.ObjectId
            sd.AgentInfo = ScenePrivate.FindAgent(holdData.HeldObjectInfo.SessionId)?.AgentInfo;

            return sd;
        }
    }
}