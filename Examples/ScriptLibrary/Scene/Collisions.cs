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
    [Tooltip("Sends simple script events when collisions occur.")]
    [DisplayName(nameof(Collisions))]
    public class Collisions : LibraryBase
    {
        #region EditorProperties
        [Tooltip(@"The events to send when an agent bumps the object. Can be a comma separated list of event names.
For Trigger Volumes will send when the agent enters the volume.")]
        [DefaultValue("on")]
        [DisplayName("On Agent Collide ->")]
        public readonly string OnAgentEnter;

        [Tooltip(@"For Trigger Volumes the events to send when an agent exits the volume. Can be a comma separated list of event names.
Only sent if the script is on a Trigger Volume.")]
        [DefaultValue("off")]
        [DisplayName("On Agent Exit ->")]
        public readonly string OnAgentExit;

        [Tooltip(@"The events to send when an agent punches the object. Can be a comma separated list of event names.
For Trigger Volumes will send when the agent's punch enters the volume.")]
        [DefaultValue("punch_on")]
        [DisplayName("On Agent Punch Collide ->")]
        public readonly string OnAgentPunchEnter;

        [Tooltip(@"For Trigger Volumes the events to send when an agent's punch exits the volume. Can be a comma separated list of event names.
Only sent if the script is on a Trigger Volume.")]
        [DefaultValue("punch_off")]
        [DisplayName("On Agent Punch Exit ->")]
        public readonly string OnAgentPunchExit;

        [Tooltip(@"Will send when the first agent enters a trigger volume.
Can be a comma separated list of event names.")]
        [DefaultValue("trigger_on")]
        [DisplayName("On First Agent Trigger ->")]
        public readonly string OnFirstAgentTriggerEnter;

        [Tooltip(@"Will send when the last agent exits a trigger volume.
Can be a comma separated list of event names.")]
        [DefaultValue("trigger_off")]
        [DisplayName("On Last Agent Trigger Exit ->")]
        public readonly string OnLastAgentTriggerExit;

        [Tooltip(@"The events to send when an object bumps the object. Can be a comma separated list of event names.
For Trigger Volumes will send when the object enters the volume.")]
        [DefaultValue("on")]
        [DisplayName("On Object Collide ->")]
        public readonly string OnObjectEnter;

        [Tooltip(@"For Trigger Volumes the events to send when an object exits the volume. Can be a comma separated list of event names.
Only sent if the script is on a Trigger Volume")]
        [DefaultValue("off")]
        [DisplayName("On Object Exit ->")]
        public readonly string OnObjectExit;

        [Tooltip("Enable responding to events for this script")]
        [DefaultValue("collisions_enable")]
        [DisplayName("-> Enable")]
        public readonly string EnableEvent;

        [Tooltip("Disable responding to events for this script")]
        [DefaultValue("collisions_disable")]
        [DisplayName("-> Disable")]
        public readonly string DisableEvent;

        [Tooltip(@"If StartEnabled is true then the script will respond to events when the scene is loaded
If StartEnabled is false then the script will not respond to events until an (-> Enable) event is received.")]
        [DefaultValue(true)]
        [DisplayName("Start Enabled")]
        public readonly bool StartEnabled = true;
        #endregion

        private IEventSubscription collisionSubscription = null;
        private RigidBodyComponent RigidBody = null;

        private Dictionary<ObjectId, AgentInfo> agentsInTrigger = null;

        bool enabled = false;
        int userLoggedOut = 0;

        protected override void SimpleInit()
        {
            if (!ObjectPrivate.TryGetFirstComponent(out RigidBody))
            {
                Log.Write(LogLevel.Error, "Could not start " + GetType().Name + " because no RigidBodyComponent was found.");
                return;
            }

            SubscribeToAll(DisableEvent, (data) =>
            {
                enabled = false;
            });

            SubscribeToAll(EnableEvent, (data) => { enabled = true; });
            enabled = StartEnabled;

            // Always subscribe to collisions to manage the agentsInTrigger list.
            // enabled will determine whether events are sent.
            if (collisionSubscription == null)
            {
                collisionSubscription = RigidBody.Subscribe(CollisionEventType.AllCollisions, OnCollision);
            }

            ScenePrivate.User.Subscribe(User.RemoveUser, (UserData data) =>
            {
                foreach (var agent in agentsInTrigger.Where((kvp) => kvp.Value.SessionId == data.User))
                {
                    SimpleData sd = new SimpleData(this);
                    sd.ObjectId = agent.Value.ObjectId;
                    sd.AgentInfo = agent.Value;
                    sd.SourceObjectId = ObjectPrivate.ObjectId;

                    agentsInTrigger.Remove(agent.Key);

                    if (enabled)
                    {
                        SendToAll(OnAgentExit, sd);
                        ++userLoggedOut;
                        if (agentsInTrigger.Count == 0)
                        {
                            SendToAll(OnLastAgentTriggerExit, sd);
                        }
                    }
                    break; // should only ever be 1 or none.
                }
            });

            agentsInTrigger = new Dictionary<ObjectId, AgentInfo>();
        }

        private void OnCollision(CollisionData data)
        {
            SimpleData sd = new SimpleData(this);
            sd.ObjectId = data.HitComponentId.ObjectId;
            sd.AgentInfo = ScenePrivate.FindAgent(sd.ObjectId)?.AgentInfo;
            sd.SourceObjectId = ObjectPrivate.ObjectId;

            bool isAgentPunch = (data.HitControlPoint != ControlPointType.Invalid);

            if (!enabled)
            {
                // No events are sent while disabled, just manage agentsInTrigger
                if (data.Phase == CollisionEventPhase.TriggerExit)
                {
                    if (agentsInTrigger.ContainsKey(sd.ObjectId) && !isAgentPunch)
                    {
                        agentsInTrigger.Remove(sd.ObjectId);
                    }
                }
                else if (data.Phase == CollisionEventPhase.TriggerEnter)
                {
                    if (sd.AgentInfo != null && !isAgentPunch && !agentsInTrigger.ContainsKey(sd.ObjectId))
                    {
                        agentsInTrigger[sd.ObjectId] = sd.AgentInfo;
                    }
                }
                return;
            }

            if (data.Phase == CollisionEventPhase.TriggerExit)
            {
                if (sd.ObjectId == ObjectId.Invalid)
                {
                    if (userLoggedOut > 0)
                    {
                        // This object has left the scene and it was a user whose events were managed in the user subscription.
                        --userLoggedOut;
                    }
                    else
                    {
                        // Object that was destroyed while inside the collision volume
                        SendToAll(OnObjectExit, sd);
                    }
                    return;
                }

                // We determine agent or not on exit by whether they are in the agentsInTrigger Dictionary
                // This helps handle the case where a user logs out from within the trigger volume - after which their AgentInfo will be null.
                AgentInfo storedInfo;
                if (agentsInTrigger.TryGetValue(sd.ObjectId, out storedInfo))
                {
                    // Use the stored info, in case the user has logged out.
                    sd.AgentInfo = storedInfo;
                    if (isAgentPunch)
                    {
                        SendToAll(OnAgentPunchExit, sd);
                    }
                    else
                    {
                        SendToAll(OnAgentExit, sd);
                        agentsInTrigger.Remove(sd.ObjectId);

                        if (agentsInTrigger.Count == 0)
                        {
                            SendToAll(OnLastAgentTriggerExit, sd);
                        }
                    }
                }
                else
                {
                    SendToAll(OnObjectExit, sd);
                }
            }
            else if (data.Phase == CollisionEventPhase.TriggerEnter || data.Phase == CollisionEventPhase.Invalid)
            {
                if (sd.AgentInfo != null)
                {
                    if (isAgentPunch)
                    {
                        SendToAll(OnAgentPunchEnter, sd);
                    }
                    else
                    {
                        SendToAll(OnAgentEnter, sd);

                        // Only track agents in the object if it is an enter event.
                        if (data.Phase == CollisionEventPhase.TriggerEnter)
                        {
                            if (agentsInTrigger.Count == 0)
                            {
                                SendToAll(OnFirstAgentTriggerEnter, sd);
                            }

                            agentsInTrigger[sd.ObjectId] = sd.AgentInfo;
                        }
                    }
                }
                else
                {
                    SendToAll(OnObjectEnter, sd);
                }
            }
        }
    }
}