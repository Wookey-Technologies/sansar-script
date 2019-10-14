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
        [Tooltip("The objective definition.")]
        [DisplayName("Objective")]
        public readonly ObjectiveDefinition _ObjectiveDefinition;
        protected override ObjectiveDefinition ObjectiveDefinition => _ObjectiveDefinition;

        [Tooltip("Complete on join\nComplete the objective immediately when a user joins the experience, as long as the objective is available to them.")]
        [DefaultValue(false)]
        [DisplayName("Complete on join")]
        public readonly bool CompleteOnSceneEnter;

        [Tooltip("Send Events on Join\nIf enabled then Activate/Complete/Lock events will be sent for the objective when users join the scene.")]
        [DefaultValue(false)]
        [DisplayName("Send Events on Join")]
        public readonly bool SendJoinEvents;

        [Tooltip("-> Activate\nActivate the objective for a single user, i.e make it completable. Must be events that contain agent data, e.g events sent from the Interaction Library script. Can be a comma separated list of event names.")]
        [DefaultValue("activate")]
        [DisplayName("-> Activate")]
        public readonly string ActivateEvent;

        [Tooltip("-> Complete\nComplete the objective for a single user. Must be events that contain agent data, e.g events sent from the Interaction Library script. Can be a comma separated list of event names.")]
        [DefaultValue("complete")]
        [DisplayName("-> Complete")]
        public readonly string CompleteEvent;

        [Tooltip("-> Lock\nLock the objective for a single user. Must be events that contain agent data, e.g events sent from the Interaction Library script. Can be a comma separated list of event names.")]
        [DefaultValue("lock")]
        [DisplayName("-> Lock")]
        public readonly string LockEvent;

        [Tooltip("-> Lock\nLock the objective for a single user. Must be events that contain agent data, e.g events sent from the Interaction Library script. Can be a comma separated list of event names.")]
        [DefaultValue("collect")]
        [DisplayName("-> Collect")]
        public readonly string CollectEvent;

        [Tooltip("-> Lock\nLock the objective for a single user. Must be events that contain agent data, e.g events sent from the Interaction Library script. Can be a comma separated list of event names.")]
        [DefaultValue("cancel")]
        [DisplayName("-> Cancel Collect")]
        public readonly string CancelEvent;

        [Tooltip("On Activate ->\nEvents to send when a user starts the objective. Can be a comma separated list of event names.")]
        [DefaultValue("isActive")]
        [DisplayName("On Activate ->")]
        public readonly string OnActivatedEvent;

        [Tooltip("On Locked ->\nEvents to send when the objective is locked. Can be a comma separated list of event names.")]
        [DefaultValue("isLocked")]
        [DisplayName("On Locked ->")]
        public readonly string OnLockedEvent;

        [Tooltip("On Collecting ->\nEvents to send when the objective has started being collected.")]
        [DefaultValue("collecting")]
        [DisplayName("On Collecting ->")]
        public readonly string OnCollectingEvent;

        [Tooltip("On Collected->\nEvents to send when this objective as been successfully collected.")]
        [DefaultValue("collected")]
        [DisplayName("On Collected ->")]
        public readonly string OnCollectedEvent;

        [Tooltip("On Collect Canceled ->\nEvents to send when this objective was being collected but got canceled.")]
        [DefaultValue("collectCanceled")]
        [DisplayName("On Collect Canceled ->")]
        public readonly string OnCollectCanceledEvent;

        [Tooltip("On Complete ->\nEvents to send when a user completes the objective. Can be a comma separated list of event names.")]
        [DefaultValue("isComplete")]
        [DisplayName("On Complete ->")]
        public readonly string OnCompletedEvent;

        [Tooltip("-> Check Objective\nWhen this event is received new events will be sent based on the state of the objective for the user.")]
        [DefaultValue("checkObjective")]
        [DisplayName("-> Check Objective")]
        public readonly string StateCheck;

        [Tooltip("If Active ->\nWhen '-> Check Objective' events are received, if this objective is Active for the user these events will be sent.")]
        [DefaultValue("activeObjective")]
        [DisplayName("If Active ->")]
        public readonly string IfActive;

        [Tooltip("If Locked ->\nWhen '-> Check Objective' events are received, if this objective is Locked for the user these events will be sent.")]
        [DefaultValue("lockedObjective")]
        [DisplayName("If Locked ->")]
        public readonly string IfLocked;

        [Tooltip("If Completed ->\nWhen '-> Check Objective' events are received, if this objective is Completed for the user these events will be sent.")]
        [DefaultValue("completedObjective")]
        [DisplayName("If Completed ->")]
        public readonly string IfCompleted;

        [Tooltip("If None ->\nWhen '-> Check Objective' events are received, if this objective is state 'none' (not Active, Completed, or Locked) for the user these events will be sent.")]
        [DefaultValue("noneObjective")]
        [DisplayName("If None ->")]
        public readonly string IfNone;
        #endregion

        Action Unsubscribes = null;

        protected override void InitObjective()
        {
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
                Unsubscribes += SubscribeToAll(StateCheck, CheckObjectiveState);
                Unsubscribes += SubscribeToAll(CollectEvent, Collect);
                Unsubscribes += SubscribeToAll(CancelEvent, CancelCollect);
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

            try
            {
                if (CompleteOnSceneEnter && objective.GetState() == ObjectiveState.Active)
                {
                    objective.SetState(ObjectiveState.Completed);
                }
            }
            catch (Exception) { }
        }

        protected override void OnObjectiveActive(AgentInfo agentInfo, Objective objective, bool initialJoin = false)
        {
            base.OnObjectiveActive(agentInfo, objective);

            if (!SendJoinEvents && initialJoin) return;

            SendToAll(OnActivatedEvent, GetEventData(agentInfo));

            if (CompleteOnSceneEnter && Unsubscribes != null)
            {
                try
                {
                    objective.SetState(ObjectiveState.Completed);
                }
                catch (Exception) { }
            }
        }

        protected override void OnObjectiveLocked(AgentInfo agentInfo, Objective objective, bool initialJoin = false)
        {
            base.OnObjectiveLocked(agentInfo, objective);
            if (!SendJoinEvents && initialJoin) return;
            SendToAll(OnLockedEvent, GetEventData(agentInfo));
        }

        protected override void OnObjectiveCompleted(AgentInfo agentInfo, Objective objective, bool initialJoin = false)
        {
            base.OnObjectiveCompleted(agentInfo, objective);
            if (!SendJoinEvents && initialJoin) return;
            SendToAll(OnCompletedEvent, GetEventData(agentInfo));
        }

        protected override void OnObjectiveCollectCompleted(AgentInfo agentInfo, Objective objective, int newCount, int index = -1)
        {
            base.OnObjectiveCollectCompleted(agentInfo, objective, newCount, index);
            SendToAll(OnCollectedEvent, GetEventData(agentInfo));
        }

        protected override void OnObjectiveCollectCanceled(AgentInfo agentInfo, Objective objective, int index = -1)
        {
            base.OnObjectiveCollectCanceled(agentInfo, objective, index);
            SendToAll(OnCollectCanceledEvent, GetEventData(agentInfo));
        }

        protected override void OnObjectiveCollectStarted(AgentInfo agentInfo, Objective objective, int index = -1)
        {
            base.OnObjectiveCollectStarted(agentInfo, objective, index);
            SendToAll(OnCollectingEvent, GetEventData(agentInfo));
        }

        private void ActivateObjective(ScriptEventData sed)
        {
            Objective objective = GetObjectiveFromScriptEventData(sed);

            if (objective == null) return;

            try
            {
                if (objective.GetState() == ObjectiveState.None)
                {
                    Log.Write(LogLevel.Error, "Can't activate objective if user is not on quest.");
                    return;
                }
                objective.SetState(ObjectiveState.Active);
            }
            catch (Exception) { }
        }

        private void CheckObjectiveState(ScriptEventData sed)
        {
            Objective objective = GetObjectiveFromScriptEventData(sed);

            if (objective == null) return;

            try
            {
                switch (objective.GetState())
                {
                    case ObjectiveState.Active:
                        SendToAll(IfActive, sed.Data);
                        break;
                    case ObjectiveState.Completed:
                        SendToAll(IfCompleted, sed.Data);
                        break;
                    case ObjectiveState.Locked:
                        SendToAll(IfLocked, sed.Data);
                        break;
                    case ObjectiveState.None:
                        SendToAll(IfNone, sed.Data);
                        break;
                    default: break;
                }
            }
            catch (Exception) { }
        }

        private void LockObjective(ScriptEventData sed)
        {
            Objective objective = GetObjectiveFromScriptEventData(sed);
            if (objective == null) return;

            try
            {
                if (objective.GetState() == ObjectiveState.None)
                {
                    Log.Write(LogLevel.Error, "Can't lock objective if user is not on quest.");
                    return;
                }
                objective.SetState(ObjectiveState.Locked);
            }
            catch (Exception) { }
        }

        private void CompleteObjective(ScriptEventData sed)
        {
            Objective objective = GetObjectiveFromScriptEventData(sed);
            if (objective == null) return;

            try
            {
                if (objective.GetState() == ObjectiveState.Active)
                {
                    objective.SetState(ObjectiveState.Completed);
                }
            }
            catch (Exception) { }
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

        private void Collect(ScriptEventData sed)
        {
            Objective objective = GetObjectiveFromScriptEventData(sed);
            if (objective == null) return;

            try
            {
                CollectObjective(objective.Agent);
            }
            catch (Exception) { }
        }

        private void CancelCollect(ScriptEventData sed)
        {
            Objective objective = GetObjectiveFromScriptEventData(sed);
            if (objective == null) return;

            try
            {
                AgentPrivate agent = ScenePrivate.FindAgent(objective.Agent);
                CancelCollection(agent);
            }
            catch (Exception) { }
        }
    }
}

