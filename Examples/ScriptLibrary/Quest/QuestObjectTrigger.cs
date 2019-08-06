/* This content is licensed under the terms of the Creative Commons Attribution 4.0 International License.
 * When using this content, you must:
 * •    Acknowledge that the content is from the Sansar Knowledge Base.
 * •    Include our copyright notice: "© 2017 Linden Research, Inc."
 * •    Indicate that the content is licensed under the Creative Commons Attribution-Share Alike 4.0 International License.
 * •    Include the URL for, or link to, the license summary at https://creativecommons.org/licenses/by-sa/4.0/deed.hi (and, if possible, to the complete license terms at https://creativecommons.org/licenses/by-sa/4.0/legalcode.
 * For example:
 * "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."
 */

using Sansar;
using Sansar.Script;
using Sansar.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptLibrary
{
    [Tooltip("Provides an interface to a user created quest objective.")]
    [DisplayName("Quest Objective Trigger")]
    public class QuestObjectTriggerVolume : QuestObjectBase
    {
        #region EditorProperties
        [Tooltip(@"Events to send when a user starts the objective. Can be a comma separated list of event names.")]
        [DefaultValue("isActive")]
        [DisplayName("On Activate ->")]
        public readonly string ActivatedEvent;

        [Tooltip(@"Events to send when the objective is locked. Can be a comma separated list of event names.")]
        [DefaultValue("isLocked")]
        [DisplayName("On Locked ->")]
        public readonly string LockedEvent;

        [Tooltip(@"Events to send when a user completes the objective. Can be a comma separated list of event names.")]
        [DefaultValue("isComplete")]
        [DisplayName("On Complete ->")]
        public readonly string CompletedEvent;

        [Tooltip("Enable responding to events for this script")]
        [DefaultValue("trigger_enable")]
        [DisplayName("-> Enable")]
        public readonly string EnableEvent;

        [Tooltip("Disable responding to events for this script")]
        [DefaultValue("trigger_disable")]
        [DisplayName("-> Disable")]
        public readonly string DisableEvent;

        [Tooltip(@"If StartEnabled is true then the script will respond when the scene is loaded
If StartEnabled is false then the script will not respond until an (-> Enable) event is received.")]
        [DefaultValue(true)]
        [DisplayName("Start Enabled")]
        public readonly bool StartEnabled = true;
        #endregion

        private RigidBodyComponent triggerVolume;
        bool enabled;

        protected override void SimpleInit()
        {
            if (!ObjectPrivate.TryGetFirstComponent<RigidBodyComponent>(out triggerVolume) || !triggerVolume.IsTriggerVolume())
            {
                Log.Write(LogLevel.Error, "QuestObjectTriggerVolume", "Must be attached to a trigger volume.");
                return;
            }

            base.SimpleInit();

            triggerVolume.Subscribe(CollisionEventType.Trigger, OnCollide);

            SubscribeToAll(EnableEvent, (ScriptEventData) => { enabled = true; });
            SubscribeToAll(DisableEvent, (ScriptEventData) => { enabled = false; });
            enabled = StartEnabled;
        }

        void OnCollide(CollisionData collisionData)
        {
            if (enabled && collisionData.Phase == CollisionEventPhase.TriggerEnter)
            {
                AgentPrivate agent = ScenePrivate.FindAgent(collisionData.HitObject.ObjectId);
                if (agent != null && agent.IsValid)
                {
                    SessionId sessionId;
                    try { sessionId = agent.AgentInfo.SessionId; }
                    catch { Log.Write("Agent left scene"); return; }

                    var objectiveData = WaitFor(ObjectiveDefinition.GetObjective, sessionId) as ObjectiveDefinition.GetObjectiveData;
                    if (objectiveData.Success && objectiveData.Objective.GetState() == ObjectiveState.Active)
                    {
                        objectiveData.Objective.SetState(ObjectiveState.Completed);
                    }
                }
            }
        }

        protected override void OnObjectiveActive(AgentInfo agentInfo, Objective objective)
        {
            base.OnObjectiveActive(agentInfo, objective);
            SendToAll(ActivatedEvent, GetEventData(agentInfo));
        }

        protected override void OnObjectiveCompleted(AgentInfo agentInfo, Objective objective)
        {
            base.OnObjectiveCompleted(agentInfo, objective);
            SendToAll(CompletedEvent, GetEventData(agentInfo));
        }

        protected override void OnObjectiveLocked(AgentInfo agentInfo, Objective objective)
        {
            base.OnObjectiveLocked(agentInfo, objective);
            SendToAll(LockedEvent, GetEventData(agentInfo));
        }
    }
}

