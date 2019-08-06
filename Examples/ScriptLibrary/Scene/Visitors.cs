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
using System.Collections.Generic;

namespace ScriptLibrary
{
    [Tooltip("Sends simple script events when players enter or leave the scene.")]
    [DisplayName("Visitors")]
    public class Visitors : LibraryBase
    {
        #region EditorProperties
        [Tooltip("The events to send when a user visits the scene. Can be a comma separated list of event names.")]
        [DefaultValue("on")]
        [DisplayName("On Visitor Join ->")]
        public readonly string OnAgentEnter;

        [Tooltip("The events to send when a user leaves the scene. Can be a comma separated list of event names.")]
        [DefaultValue("off")]
        [DisplayName("On Visitor Exit ->")]
        public readonly string OnAgentExit;

        [Tooltip("Enable responding to events for this script")]
        [DefaultValue("visitors_enable")]
        [DisplayName("-> Enable")]
        public readonly string EnableEvent;

        [Tooltip("Disable responding to events for this script")]
        [DefaultValue("visitors_disable")]
        [DisplayName("-> Disable")]
        public readonly string DisableEvent;

        [Tooltip(@"If StartEnabled is true then the script will respond to events when the scene is loaded
If StartEnabled is false then the script will not respond to events until an (-> Enable) event is received.")]
        [DefaultValue(true)]
        [DisplayName("Start Enabled")]
        public readonly bool StartEnabled = true;
        #endregion

        Action subscriptions = null;

        Dictionary<SessionId, AgentInfo> visitors = new Dictionary<SessionId, AgentInfo>();
        protected override void SimpleInit()
        {
            SubscribeToAll(DisableEvent, (data) =>
            {
                if (subscriptions != null)
                {
                    subscriptions();
                    subscriptions = null;
                    visitors.Clear();
                }
            });

            SubscribeToAll(EnableEvent, Enable);

            if (StartEnabled) Enable(null);
        }

        private void Enable(ScriptEventData data)
        {
            if (subscriptions == null)
            {
                IEventSubscription sub = ScenePrivate.User.Subscribe(User.AddUser, (UserData ud) =>
            {
                SimpleData sd = new SimpleData(this);
                sd.SourceObjectId = ObjectPrivate.ObjectId;
                sd.AgentInfo = ScenePrivate.FindAgent(ud.User)?.AgentInfo;
                if (sd.AgentInfo != null)
                {
                    sd.ObjectId = sd.AgentInfo.ObjectId;
                }
                visitors[ud.User] = sd.AgentInfo;
                SendToAll(OnAgentEnter, sd);
            });
                subscriptions = sub.Unsubscribe;
                sub = ScenePrivate.User.Subscribe(User.RemoveUser, (UserData ud) =>
                {
                    SimpleData sd = new SimpleData(this);
                    sd.SourceObjectId = ObjectPrivate.ObjectId;
                    AgentInfo agentInfo = null;
                    if (visitors.TryGetValue(ud.User, out agentInfo))
                    {
                        sd.AgentInfo = agentInfo;
                        visitors.Remove(ud.User);
                    }
                    SendToAll(OnAgentExit, sd);
                });
                subscriptions += sub.Unsubscribe;
            }
        }

    }
}