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
    [DisplayName(nameof(QuestObject))]
    public abstract class QuestObjectBase : LibraryBase
    {
        #region EditorProperties

        [Tooltip(@"The objective definition.")]
        [DisplayName("Objective")]
        public readonly ObjectiveDefinition ObjectiveDefinition;

        #endregion

        protected override void SimpleInit()
        {
            if (ObjectiveDefinition == null)
            {
                Log.Write(LogLevel.Error, "Objective Definition not found.");
                return;
            }

            var update = WaitFor(ObjectiveDefinition.Update);

            if (!update.Success)
            {
                Log.Write(LogLevel.Error, "Failed to update objective definition.");
                return;
            }

            ScenePrivate.User.Subscribe(User.AddUser, OnAddUser);
        }


        void OnAddUser(UserData data)
        {
            AgentPrivate agent = ScenePrivate.FindAgent(data.User);

            if (agent == null || !agent.IsValid)
            {
                return;
            }

            Objective objective = null;
            AgentInfo agentInfo = null;

            try
            {
                agentInfo = agent.AgentInfo;

                var objectiveData = WaitFor(ObjectiveDefinition.GetObjective, agentInfo.SessionId) as ObjectiveDefinition.GetObjectiveData;
                if (objectiveData.Success)
                {
                    objective = objectiveData.Objective;
                }

            }
            catch
            {
            }

            if (objective != null)
            {
                SimpleLog(LogLevel.Info, $"agent: {agentInfo.Name}  quest: {objective.Definition.Title} state: {objective.GetState()}");

                switch (objective.GetState())
                {
                    case ObjectiveState.Active:
                        OnObjectiveActive(agentInfo, objective);
                        break;

                    case ObjectiveState.Locked:
                        OnObjectiveLocked(agentInfo, objective);
                        break;

                    case ObjectiveState.Completed:
                        OnObjectiveCompleted(agentInfo, objective);
                        break;
                }

                objective.Subscribe(ObjectiveState.Active, (ObjectiveData d) =>
                {
                    OnObjectiveActive(agentInfo, objective);
                });

                objective.Subscribe(ObjectiveState.Locked, (ObjectiveData d) =>
                {
                    OnObjectiveLocked(agentInfo, objective);
                });

                objective.Subscribe(ObjectiveState.Completed, (ObjectiveData d) =>
                {
                     OnObjectiveCompleted(agentInfo, objective);
                });

                objective.Subscribe(ObjectiveState.None, (ObjectiveData d) =>
                {
                    OnObjectiveReset(agentInfo, objective);
                });

                OnObjectiveJoinExperience(agentInfo, objective);
            }
        }

        protected virtual void OnObjectiveJoinExperience(AgentInfo agentInfo, Objective objective)
        {
        }

        protected virtual void OnObjectiveActive(AgentInfo agentInfo, Objective objective)
        {
            SimpleLog(LogLevel.Info, $"agent: {agentInfo.Name}  objective: {objective.Definition.Title} Active.");
        }

        protected virtual void OnObjectiveLocked(AgentInfo agentInfo, Objective objective)
        {
            SimpleLog(LogLevel.Info, $"agent: {agentInfo.Name}  objective: {objective.Definition.Title} Locked.");
        }

        protected virtual void OnObjectiveCompleted(AgentInfo agentInfo, Objective objective)
        {
            SimpleLog(LogLevel.Info, $"agent: {agentInfo.Name}  objective: {objective.Definition.Title} Completed.");
        }

        protected virtual void OnObjectiveReset(AgentInfo agentInfo, Objective objective)
        {
            SimpleLog(LogLevel.Info, $"agent: {agentInfo.Name}  objective: {objective.Definition.Title} Reset.");
        }

        protected SimpleData GetEventData(AgentInfo agentInfo)
        {
            SimpleData data = new SimpleData(this);
            data.AgentInfo = agentInfo;
            return data;
        }

    }
}

