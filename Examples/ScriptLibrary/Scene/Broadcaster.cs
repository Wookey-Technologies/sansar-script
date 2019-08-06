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
using System.Linq;

namespace ScriptLibrary
{
    [Tooltip("Controls voice broadcasting in response to simple script events.")]
    [DisplayName(nameof(Broadcaster))]
    public class Broadcaster : LibraryBase // Broadcaster settings require ScenePrivate
    {
        #region EditorProperties
        [Tooltip("Start broadcasting the agent. Can be a comma separated list of event names.")]
        [DefaultValue("toggle_broadcasting")]
        [DisplayName("-> Toggle Agent Broadcasting")]
        public readonly string ToggleEvent;

        [Tooltip("Start broadcasting the agent. Can be a comma separated list of event names.")]
        [DefaultValue("on")]
        [DisplayName("-> Start Broadcasting Agent")]
        public readonly string StartEvent;

        [Tooltip("Stop broadcasting the agent. Can be a comma separated list of event names.")]
        [DefaultValue("off")]
        [DisplayName("-> Stop Broadcasting Agent")]
        public readonly string StopEvent;

        [Tooltip("Enable responding to events for this script. Can be a comma separated list of event names.")]
        [DefaultValue("broadcast_enable")]
        [DisplayName("-> Enable")]
        public readonly string EnableEvent;

        [Tooltip("Disable responding to events for this script. Can be a comma separated list of event names.")]
        [DefaultValue("broadcast_disable")]
        [DisplayName("-> Disable")]
        public readonly string DisableEvent;

        [Tooltip(@"If StartEnabled is true then the script will respond to events when the scene is loaded
If StartEnabled is false then the script will not respond to events until an (-> Enable) event is received.")]
        [DefaultValue(true)]
        [DisplayName("Start Enabled")]
        public readonly bool StartEnabled = true;

        #endregion


        Action subscription = null;
        HashSet<AgentPrivate> broadcastAgents = null;

        protected override void SimpleInit()
        {
            broadcastAgents = new HashSet<AgentPrivate>();

            if (StartEnabled) Subscribe(null);

            SubscribeToAll(EnableEvent, Subscribe);
            SubscribeToAll(DisableEvent, Unsubscribe);
        }

        private void Subscribe(ScriptEventData sed)
        {
            if (subscription == null)
            {
                subscription = SubscribeToAll(StartEvent, (ScriptEventData subdata) =>
                {
                    ISimpleData simpledata = subdata.Data?.AsInterface<ISimpleData>();
                    if (simpledata != null && simpledata.AgentInfo != null)
                    {
                        AgentPrivate agent = ScenePrivate.FindAgent(simpledata.AgentInfo.SessionId);

                        if (agent != null && agent.IsValid)
                        {
                            broadcastAgents.Add(agent);
                            ScenePrivate.SetMegaphone(agent, true); // User gets notified of megaphone status
                        }
                    }
                });

                subscription += SubscribeToAll(StopEvent, (ScriptEventData subdata) =>
                {
                    ISimpleData simpledata = subdata.Data?.AsInterface<ISimpleData>();
                    if (simpledata != null && simpledata.AgentInfo != null)
                    {
                        AgentPrivate agent = ScenePrivate.FindAgent(simpledata.AgentInfo.SessionId);

                        if (agent != null && agent.IsValid)
                        {
                            broadcastAgents.Remove(agent);
                            ScenePrivate.SetMegaphone(agent, false); // User gets notified of megaphone status
                        }
                    }
                });

                subscription += SubscribeToAll(ToggleEvent, (ScriptEventData subdata) =>
                {
                    ISimpleData simpledata = subdata.Data?.AsInterface<ISimpleData>();
                    if (simpledata != null && simpledata.AgentInfo != null)
                    {
                        AgentPrivate agent = ScenePrivate.FindAgent(simpledata.AgentInfo.SessionId);

                        if (agent != null && agent.IsValid)
                        {
                            if (broadcastAgents.Contains(agent))
                            {
                                broadcastAgents.Remove(agent);
                                ScenePrivate.SetMegaphone(agent, false); // User gets notified of megaphone status
                            }
                            else
                            {
                                broadcastAgents.Add(agent);
                                ScenePrivate.SetMegaphone(agent, true); // User gets notified of megaphone status
                            }
                        }
                    }
                });
            }
        }

        private void Unsubscribe(ScriptEventData sed)
        {
            if (subscription != null)
            {
                subscription();
                subscription = null;
            }
        }
    }
}