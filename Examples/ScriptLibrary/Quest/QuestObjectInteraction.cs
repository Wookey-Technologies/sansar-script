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
    [DisplayName("Quest Objective Interaction")]
    public class QuestObjectInteraction : QuestObjectBase
    {
        #region EditorProperties
        [Tooltip(@"Complete Objective Interaction")]
        [DefaultValue("Click Me!")]
        [DisplayName("Complete Objective Interaction")]
        public Sansar.Simulation.Interaction Interaction;

        [Tooltip(@"Set the mesh component visibility to off when this objective is in the 'Locked' state")]
        [DefaultValue(false)]
        public bool HideWhenLocked;

        [Tooltip(@"Set the mesh component visibility to off when this objective is in the 'Completed' state")]
        [DefaultValue(false)]
        public bool HideWhenComplete;

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
        #endregion

        MeshComponent meshComponent;
        bool initiallyVisible;

        protected override void SimpleInit()
        {
            ObjectPrivate.TryGetFirstComponent<MeshComponent>(out meshComponent);
            if (meshComponent != null)
            {
                initiallyVisible = meshComponent.GetIsVisible();
            }

            base.SimpleInit();

            if (Interaction == null)
            {
                Log.Write(LogLevel.Error, "QuestObjectInteraction", "No interaction");
                return;
            }
            Interaction.Subscribe((InteractionData data) =>
            {
                CompleteObjective(data.AgentId);
            });
            Interaction.SetEnabled(false);
        }

        void CompleteObjective(SessionId sessionId)
        {
            var objectiveData = WaitFor(ObjectiveDefinition.GetObjective, sessionId) as ObjectiveDefinition.GetObjectiveData;

            if (objectiveData.Success)
            {
                objectiveData.Objective.SetState(ObjectiveState.Completed);
            }
        }

        void SetEnabledForSessionId(SessionId sessionId, bool enabled)
        {
            SimpleLog(LogLevel.Info, $"Setting objective {ObjectiveDefinition.Title} enabled {enabled}");
            Interaction.SetEnabled(sessionId, enabled);
        }

        void SetVisibleForSessionId(SessionId sessionId, bool visible)
        {
            if ( meshComponent != null && meshComponent.IsScriptable)
            {
                SimpleLog(LogLevel.Info, $"Setting objective {ObjectiveDefinition.Title} visible {visible}");
                meshComponent.SetIsVisible(sessionId, visible);
            }
            else if (HideWhenLocked || HideWhenComplete)
            {
                Log.Write(LogLevel.Error, "QuestObjectInteraction", "Object must have scriptable mesh component to be enable showing/hiding it from script.");
            }
        }

        protected override void OnObjectiveJoinExperience(AgentInfo agentInfo, Objective objective)
        {
            base.OnObjectiveJoinExperience(agentInfo, objective);
            if (objective.GetState() == ObjectiveState.Active)
            {
                SetVisibleForSessionId(agentInfo.SessionId, true);
                SetEnabledForSessionId(agentInfo.SessionId, true);
            }
            else if (objective.GetState() == ObjectiveState.Completed && HideWhenComplete)
            {
                SetVisibleForSessionId(agentInfo.SessionId, false);
            }
            else if (objective.GetState() == ObjectiveState.Locked && HideWhenLocked)
            {
                SetVisibleForSessionId(agentInfo.SessionId, false);
            }
        }

        protected override void OnObjectiveActive(AgentInfo agentInfo, Objective objective)
        {
            base.OnObjectiveActive(agentInfo, objective);
            SetVisibleForSessionId(agentInfo.SessionId, true);
            SetEnabledForSessionId(agentInfo.SessionId, true);
            SendToAll(ActivatedEvent, GetEventData(agentInfo));
        }

        protected override void OnObjectiveCompleted(AgentInfo agentInfo, Objective objective)
        {
            base.OnObjectiveCompleted(agentInfo, objective);
            SetEnabledForSessionId(agentInfo.SessionId, false);
            if (HideWhenComplete)
            {
                SetVisibleForSessionId(agentInfo.SessionId, false);
            }
            SendToAll(CompletedEvent, GetEventData(agentInfo));
        }

        protected override void OnObjectiveLocked(AgentInfo agentInfo, Objective objective)
        {
            base.OnObjectiveLocked(agentInfo, objective);
            SetEnabledForSessionId(agentInfo.SessionId, false);
            if (HideWhenLocked)
            {
                SetVisibleForSessionId(agentInfo.SessionId, false);
            }
            SendToAll(LockedEvent, GetEventData(agentInfo));
        }

        protected override void OnObjectiveReset(AgentInfo agentInfo, Objective objective)
        {
            base.OnObjectiveReset(agentInfo, objective);
            if (HideWhenLocked || HideWhenComplete)
            {
                SetVisibleForSessionId(agentInfo.SessionId, initiallyVisible);
            }
            SetEnabledForSessionId(agentInfo.SessionId, false);
        }
    }
}

