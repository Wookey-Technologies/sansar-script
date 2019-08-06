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
    [Tooltip("Sends simple script events when an avatar sits down or stands up from the owning object.")]
    [DisplayName("Sit/Stand")]
    public class SitStand : LibraryBase
    {
        #region EditorProperties

        [Tooltip("The events to send when an avatar sits on this object")]
        [DefaultValue("sitdown")]
        [DisplayName("Sit Down ->")]
        public readonly string SitDownMessage;

        [Tooltip("The events to send when an avatar stands up from this object")]
        [DefaultValue("standup")]
        [DisplayName("Stand Up ->")]
        public readonly string StandUpMessage;

        [Tooltip("Enable responding to events for this script")]
        [DefaultValue("sit_stand_enable")]
        [DisplayName("-> Enable")]
        public readonly string EnableEvent;

        [Tooltip("Disable responding to events for this script")]
        [DefaultValue("sit_stand_disable")]
        [DisplayName("-> Disable")]
        public readonly string DisableEvent;

        [Tooltip(@"If StartEnabled is true then the script will respond to interactions when the scene is loaded
If StartEnabled is false then the script will not respond to interactions until an (-> Enable) event is received.")]
        [DefaultValue(true)]
        [DisplayName("Start Enabled")]
        public readonly bool StartEnabled = true;
        #endregion

        IEventSubscription sitSubscribe = null;
        IEventSubscription standSubscribe = null;
        RigidBodyComponent rigidBody;

        protected override void SimpleInit()
        {
            if (!ObjectPrivate.TryGetFirstComponent<RigidBodyComponent>(out rigidBody))
            {
                Log.Write(LogLevel.Error, __SimpleTag, "SimpleSitStand must be attached to an object with a RigidBodyComponent");
                return;
            }

            SitObjectInfo[] sitPoints = rigidBody.GetSitObjectInfo();
            if (sitPoints.Length == 0)
            {
                Log.Write(LogLevel.Warning, __SimpleTag, "SimpleSitStand must be attached to an object with at least one sit point to be useful!");
                return;
            }

            if (StartEnabled) Subscribe(null);

            SubscribeToAll(EnableEvent, Subscribe);
            SubscribeToAll(DisableEvent, Unsubscribe);
        }

        private void Subscribe(ScriptEventData data)
        {
            if (!string.IsNullOrWhiteSpace(SitDownMessage))
            {
                sitSubscribe = rigidBody.SubscribeToSitObject(SitEventType.Start, (SitObjectData sitData) =>
                {
                    SendToAll(SitDownMessage, SitStandData(sitData));
                });
            }

            if (!string.IsNullOrWhiteSpace(StandUpMessage))
            {
                standSubscribe = rigidBody.SubscribeToSitObject(SitEventType.End, (SitObjectData sitData) =>
                {
                    SendToAll(StandUpMessage, SitStandData(sitData));
                });
            }
        }

        private void Unsubscribe(ScriptEventData data)
        {
            if (sitSubscribe != null)
            {
                sitSubscribe.Unsubscribe();
                sitSubscribe = null;
            }

            if (standSubscribe != null)
            {
                standSubscribe.Unsubscribe();
                standSubscribe = null;
            }
        }

        private SimpleData SitStandData(SitObjectData sitData)
        {
            SimpleData sd = new SimpleData(this);

            sd.ObjectId = sitData.ObjectId;
            sd.SourceObjectId = ObjectPrivate.ObjectId; // expected to equal sd.ObjectId
            sd.AgentInfo = ScenePrivate.FindAgent(sitData.SitObjectInfo.SessionId)?.AgentInfo;

            return sd;
        }
    }
}