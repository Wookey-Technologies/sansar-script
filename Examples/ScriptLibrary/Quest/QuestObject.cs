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
    [DisplayName("Quest Objective Controller")]
    public class QuestObject : QuestObjectBase
    {
        #region EditorProperties
        [Tooltip(@"Activate the objective for a single user, i.e make it completable. Must be events that contain agent data, e.g events sent from the Interaction Library script. Can be a comma separated list of event names.")]
        [DefaultValue("activate")]
        [DisplayName("-> Activate")]
        public readonly string ActivateEvent;

        [Tooltip(@"Complete the objective for a single user. Must be events that contain agent data, e.g events sent from the Interaction Library script. Can be a comma separated list of event names.")]
        [DefaultValue("complete")]
        [DisplayName("-> Complete")]
        public readonly string CompleteEvent;

        [Tooltip(@"Lock the objective for a single user. Must be events that contain agent data, e.g events sent from the Interaction Library script. Can be a comma separated list of event names.")]
        [DefaultValue("lock")]
        [DisplayName("-> Lock")]
        public readonly string LockEvent;

        [Tooltip(@"Complete the objective immediately when a user joins the experience, as long as the objective is available to them.")]
        [DefaultValue(false)]
        [DisplayName("Complete on join")]
        public readonly bool CompleteOnSceneEnter;

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
        [DefaultValue("objective_enable")]
        [DisplayName("-> Enable")]
        public readonly string EnableEvent;

        [Tooltip("Disable responding to events for this script")]
        [DefaultValue("objective_disable")]
        [DisplayName("-> Disable")]
        public readonly string DisableEvent;

        [Tooltip(@"If StartEnabled is false then this script will not respond to events until an -> Enable event is received.")]
        [DefaultValue(true)]
        [DisplayName("Start Enabled")]
        public readonly bool StartEnabled = true;

        #endregion

        Action Unsubscribes = null;

        protected override void SimpleInit()
        {
            base.SimpleInit();

            if (StartEnabled) Subscribe(null);

            SubscribeToAll(EnableEvent, Subscribe);
            SubscribeToAll(DisableEvent, Unsubscribe);
        }

        private void Subscribe(ScriptEventData sed)
        {
            if (Unsubscribes == null)
            {
                Unsubscribes = SubscribeToAll(CompleteEvent, CompleteObjective);
                Unsubscribes += SubscribeToAll(ActivateEvent, ActivateObjective);
                Unsubscribes += SubscribeToAll(LockEvent, LockObjective);
            }
        }

        private void Unsubscribe(ScriptEventData sed)
        {
            if (Unsubscribes != null)
            {
                Unsubscribes();
                Unsubscribes = null;
            }
        }

        protected override void OnObjectiveJoinExperience(AgentInfo agentInfo, Objective objective)
        {
            base.OnObjectiveJoinExperience(agentInfo, objective);

            if (CompleteOnSceneEnter && Unsubscribes != null && objective.GetState() == ObjectiveState.Active)
            {
                objective.SetState(ObjectiveState.Completed);
            }
        }

        protected override void OnObjectiveActive(AgentInfo agentInfo, Objective objective)
        {
            base.OnObjectiveActive(agentInfo, objective);
            SendToAll(ActivatedEvent, GetEventData(agentInfo));

            if (CompleteOnSceneEnter && Unsubscribes != null)
            {
                objective.SetState(ObjectiveState.Completed);
            }
        }

        protected override void OnObjectiveLocked(AgentInfo agentInfo, Objective objective)
        {
            base.OnObjectiveLocked(agentInfo, objective);
            SendToAll(LockedEvent, GetEventData(agentInfo));
        }

        protected override void OnObjectiveCompleted(AgentInfo agentInfo, Objective objective)
        {
            base.OnObjectiveCompleted(agentInfo, objective);
            SendToAll(CompletedEvent, GetEventData(agentInfo));
        }

        private void ActivateObjective(ScriptEventData sed)
        {
            Objective objective = GetObjectiveFromScriptEventData(sed);

            if (objective.GetState() == ObjectiveState.None)
            {
                Log.Write(LogLevel.Error, "Can't activate objective if user is not on quest.");
                return;
            }
            objective.SetState(ObjectiveState.Active);
        }

        private void LockObjective(ScriptEventData sed)
        {
            Objective objective = GetObjectiveFromScriptEventData(sed);

            if (objective.GetState() == ObjectiveState.None)
            {
                Log.Write(LogLevel.Error, "Can't lock objective if user is not on quest.");
                return;
            }
            objective.SetState(ObjectiveState.Locked);
        }

        private void CompleteObjective(ScriptEventData sed)
        {
            Objective objective = GetObjectiveFromScriptEventData(sed);
           
            if (objective.GetState() == ObjectiveState.Active)
            {
                objective.SetState(ObjectiveState.Completed);
            }
        }

        Objective GetObjectiveFromScriptEventData(ScriptEventData sed)
        {
            ISimpleData idata = sed.Data.AsInterface<ISimpleData>();
            if (idata != null && idata.AgentInfo != null)
            {
                SessionId sessionId = idata.AgentInfo.SessionId;

                var objectiveData = WaitFor(ObjectiveDefinition.GetObjective, sessionId) as ObjectiveDefinition.GetObjectiveData;

                if (objectiveData.Success)
                {
                    return objectiveData.Objective;
                }
            }
            Log.Write(LogLevel.Error, "Failed to get objective from event data");
            return null;
        }
    }
}

