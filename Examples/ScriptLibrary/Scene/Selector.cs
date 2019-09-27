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
using System.Linq;
using System;

namespace ScriptLibrary
{
    [Tooltip("Put this script on an object to create a selector that will send events after being picked up when the 'Select Command' is used to select users, objects, or targets.")]
    [DisplayName("Selector")]
    public class Selector : LibraryBase // SubscribeToCopmmand requries Client requires AgentPrivate requires ScenePrivate
    {
        [DefaultValue("Trigger")]
        [Tooltip("The input method to select something. Default: 'Trigger' corresponds to left mouse button and VR triggers.")]
        [DisplayName("Select Command")]
        public string SelectCommand;

        [Tooltip("Event sent when another avatar is selected. These events are sent as if the avatar selected performed the action! A Teleport script listening for this event would teleport the person selected.")]
        [DisplayName("Player Selected ->")]
        [DefaultValue("on")]
        public string PlayerSelectedEvent;

        [Tooltip("Event sent when an object with the Simple Script Library script 'Game.Target' is selected. These events are sent as if the object selected performed the action. A Teleport script listening for this event will do nothing.")]
        [DisplayName("Target Selected ->")]
        [DefaultValue("on")]
        public string TargetSelectedEvent;

        [Tooltip("If true then the 'Target Selected ->' event and the Target's 'Target Hit ->' event will only be sent if the target is in the same group as the selector. If the Target is in a different group then 'Object Selected ->' events are sent.")]
        [DisplayName("Group Targets Only")]
        [DefaultValue(true)]
        public bool SameGroupRequired = true;

        [Tooltip("Event sent when an object without the Target script is selected. These events are sent as if the object selected performed the action! A Teleport script listening for this event would do nothing.")]
        [DisplayName("Object Selected ->")]
        [DefaultValue("off")]
        public string ObjectSelectedEvent;

        [Tooltip("Event sent when a shot hits nothing - no players or objects of any kind.")]
        [DisplayName("Nothing Selected ->")]
        [DefaultValue("off")]
        public string NothingSelectedEvent;

        [Tooltip("This event happens every time the 'Select Command' is used, whether anything is hit or not. These events are sent as the user of the selector object performing the action. A Teleport script listening for this event would teleport the user holding the selector object.")]
        [DisplayName("Every Select Command ->")]
        public string EveryCommandEvent;

        [DisplayName("VR Enabled")]
        [Tooltip("If true the selector object will work in VR.")]
        [DefaultValue(true)]
        public bool VREnabled;

        [DisplayName("Mouse Look Enabled")]
        [Tooltip("If true the selector will only work in desktop mouse look mode. Press Escape to enter or exit mouse look mode.")]
        [DefaultValue(true)]
        public bool MouseLookEnabled;

        [DisplayName("Free Click Enabled")]
        [Tooltip("If true the selector will work with normal mouse clicks, outside of mouse look mode.\nNote that this mode can make some aiming games trivial.")]
        [DefaultValue(true)]
        public bool FreeClickEnabled;

        [DisplayName("Free Cam Enabled")]
        [Tooltip("If true the selector will work while the user is in 'Free Cam' mode.")]
        [DefaultValue(true)]
        public bool FreeCamEnabled;

        private ControlPointType heldHand = ControlPointType.Invalid;
        private AgentInfo holdingAgent;
        private SimpleData simpleData;

        protected override void SimpleInit()
        {
            simpleData = new SimpleData(this);
            simpleData.SourceObjectId = ObjectPrivate.ObjectId;

            RigidBodyComponent RigidBody;
            if (ObjectPrivate.TryGetFirstComponent(out RigidBody))
            {
                if (!RigidBody.GetCanGrab())
                {
                    RigidBody.SetCanGrab(true, (data) =>
                    {
                        if (data.Success == false)
                        {
                            Log.Write("Could not set selector to grabbable - won't be able to pick up the gun.");
                        }
                    });
                }
                RigidBody.SubscribeToHeldObject(HeldObjectEventType.Grab, OnPickup);
                RigidBody.SubscribeToHeldObject(HeldObjectEventType.Release, OnDrop);
            }
        }

        System.Action unsubscribe = null;

        void OnPickup(HeldObjectData data)
        {
            try
            {
                AgentPrivate agent = ScenePrivate.FindAgent(data.HeldObjectInfo.SessionId);
                
                holdingAgent = agent.AgentInfo;
                heldHand = data.HeldObjectInfo.ControlPoint;
                simpleData.AgentInfo = holdingAgent;
                simpleData.ObjectId = holdingAgent.ObjectId;

                unsubscribe = agent.Client.SubscribeToCommand(SelectCommand, OnSelect, null).Unsubscribe;
            }
            catch (System.Exception)
            {
                holdingAgent = null;
                simpleData.AgentInfo = null;
                simpleData.ObjectId = ObjectId.Invalid;
            }
        }

        void OnDrop(HeldObjectData data)
        {
            unsubscribe();
            unsubscribe = null;
            holdingAgent = null;
            simpleData.AgentInfo = null;
            simpleData.ObjectId = ObjectId.Invalid;
        }

        void OnSelect(CommandData command)
        {
            try
            {
                if (!FreeCamEnabled && command.CameraControlMode == CameraControlMode.FlyCam)
                {
                    AgentPrivate user = ScenePrivate.FindAgent(holdingAgent.SessionId);
                    user.SendChat("This device does not work in Free Cam mode, return to 1st or 3rd person views to use this device.");
                    return;
                }

                switch (command.ControlPoint)
                {
                    case ControlPointType.DesktopGrab:
                        if (!MouseLookEnabled && command.MouseLookMode)
                        {
                            AgentPrivate user = ScenePrivate.FindAgent(holdingAgent.SessionId);
                            user.SendChat("This device does not work in desktop Mouse Look Mode: press Escape to enter or exit Mouse Look.");
                            return;
                        }
                        if (!FreeClickEnabled && !command.MouseLookMode)
                        {
                            AgentPrivate user = ScenePrivate.FindAgent(holdingAgent.SessionId);
                            user.SendChat("This device does not work in desktop Free Click Mode: press Escape to enter or exit Mouse Look.");
                            return;
                        }
                        break;
                    case ControlPointType.LeftTool:
                    case ControlPointType.RightTool:
                        if (!VREnabled)
                        {
                            AgentPrivate user = ScenePrivate.FindAgent(holdingAgent.SessionId);
                            user.SendChat("This device does not work in VR.");
                            return;
                        }

                        // Check hand after CheckReload which handles "shooting yourself to reload"
                        // If they grabbed it in desktop let them use it from whichever hand now? I guess?
                        if (heldHand != ControlPointType.DesktopGrab && command.ControlPoint != heldHand)
                        {
                            return;
                        }
                        break;
                    default:
                        break;
                }

                SendToAll(EveryCommandEvent, simpleData);

                var targetAgent = ScenePrivate.FindAgent(command.TargetingComponent.ObjectId);
                if (targetAgent != null)
                {
                    SimpleData targetSimpleData = new SimpleData(this);
                    targetSimpleData.AgentInfo = targetAgent.AgentInfo;
                    targetSimpleData.ObjectId = targetSimpleData.AgentInfo.ObjectId;
                    targetSimpleData.SourceObjectId = simpleData.SourceObjectId;

                    SendToAll(PlayerSelectedEvent, targetSimpleData);
                }
                else
                {
                    ObjectPrivate targetObject = ScenePrivate.FindObject(command.TargetingComponent.ObjectId);
                    if (targetObject != null)
                    {
                        SimpleData targetSimpleData = new SimpleData(this);
                        targetSimpleData.AgentInfo = null;
                        targetSimpleData.ObjectId = targetObject.ObjectId;
                        targetSimpleData.SourceObjectId = simpleData.SourceObjectId;

                        ITarget target = targetObject.FindScripts<ITarget>("Simple.Target").FirstOrDefault();
                        if (target == null
                            || (SameGroupRequired && target.GetGroupTag() != base.Group))
                        {
                            SendToAll(ObjectSelectedEvent, targetSimpleData);
                            return;
                        }
                        else
                        {
                            target.Hit(holdingAgent, command);
                            SendToAll(TargetSelectedEvent, targetSimpleData);
                            return;
                        }
                    }

                    SendToAll(NothingSelectedEvent, simpleData);
                }
            }
            catch (System.Exception) { } // ignore exceptions for not found agents.
        }

        public interface ITarget
        {
            int Hit(AgentInfo agent, CommandData data);
            string GetGroupTag();
        }
    }
}