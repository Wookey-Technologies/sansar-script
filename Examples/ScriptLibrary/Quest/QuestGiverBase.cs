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
    public abstract class QuestGiverBase : LibraryBase
    {
        #region EditorProperties

        [Tooltip(@"The quest definition.")]
        [DisplayName("Quest")]
        public readonly QuestDefinition QuestDefinition;

        #endregion

        protected override void SimpleInit()
        {
            if (QuestDefinition == null)
            {
                Log.Write(LogLevel.Error, "Quest Definition not found.");
                return;
            }

            var update = WaitFor(QuestDefinition.Update);

            if (update.Success)
            {
                SimpleLog(LogLevel.Info, $"Got quest definition: {QuestDefinition.Title}");
            }
            else
            {
                Log.Write(LogLevel.Error, "Failed to update quest definition.");
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

            Sansar.Simulation.Quest quest = null;
            AgentInfo agentInfo = null;

            try
            {
                agentInfo = agent.AgentInfo;

                var questData = WaitFor(QuestDefinition.GetQuest, agentInfo.SessionId) as QuestDefinition.GetQuestData;
                if (questData.Success)
                {
                    quest = questData.Quest;
                }

            }
            catch
            {
                Log.Write(LogLevel.Error, "Failed to get agent info, user may have left experience.");
                return;
            }

            if (quest != null)
            {
                SimpleLog(LogLevel.Info, $"agent: {agentInfo.Name}  quest: {quest.Definition.Title} state: {quest.GetState()}");

                switch(quest.GetState())
                {
                    case QuestState.None:
                        OnAgentQuestAvailable(agentInfo, quest);
                        break;

                    case QuestState.Offered:
                        OnAgentOfferedQuest(agentInfo, quest);
                        break;

                    case QuestState.Active:
                        OnAgentStartedQuest(agentInfo, quest);
                        break;

                    case QuestState.Completed:
                        OnAgentCompletedQuest(agentInfo, quest);
                        break;
                }

                quest.Subscribe(QuestState.Offered, (QuestData d) =>
                {
                    OnAgentOfferedQuest(agentInfo, quest);
                });

                quest.Subscribe(QuestState.Active, (QuestData d) =>
                {
                    OnAgentStartedQuest(agentInfo, quest);
                });

                quest.Subscribe(QuestState.Completed, (QuestData d) =>
                {
                    OnAgentCompletedQuest(agentInfo, quest);
                });

                quest.Subscribe(QuestState.None, (QuestData d) =>
                {
                    OnAgentResetQuest(agentInfo, quest);
                });

                OnAgentJoinExperience(agentInfo, quest);
            }
        }

        protected void OfferQuest(ScriptEventData sed)
        {
            GetQuestFromScriptEventData(sed)?.Offer();
        }

        protected void CompleteQuest(ScriptEventData sed)
        {
            GetQuestFromScriptEventData(sed)?.SetState(QuestState.Completed);
        }

        Quest GetQuestFromScriptEventData(ScriptEventData sed)
        {
            ISimpleData idata = sed.Data.AsInterface<ISimpleData>();
            if (idata != null && idata.AgentInfo != null)
            {
                SessionId sessionId = idata.AgentInfo.SessionId;

                var questData = WaitFor(QuestDefinition.GetQuest, sessionId) as QuestDefinition.GetQuestData;

                if (questData.Success)
                 {
                    return questData.Quest;
                }
            }
            return null;
        }

        protected void OfferToAgent(SessionId sessionId)
        {
            var questData = WaitFor(QuestDefinition.GetQuest, sessionId) as QuestDefinition.GetQuestData;
            if (questData.Success)
            {
                questData.Quest.Offer();
            }
        }

        protected virtual void OnAgentJoinExperience(AgentInfo agentInfo, Sansar.Simulation.Quest quest)
        {
        }

        protected virtual void OnAgentQuestAvailable(AgentInfo agentInfo, Sansar.Simulation.Quest quest)
        {
            SimpleLog(LogLevel.Info, $"agent: {agentInfo.Name}  quest: {quest.Definition.Title} Available.");
        }

        protected virtual void OnAgentOfferedQuest(AgentInfo agentInfo, Sansar.Simulation.Quest quest)
        {
            SimpleLog(LogLevel.Info, $"agent: {agentInfo.Name}  quest: {quest.Definition.Title} Offered.");
        }

        protected virtual void OnAgentStartedQuest(AgentInfo agentInfo, Sansar.Simulation.Quest quest)
        {
            SimpleLog(LogLevel.Info, $"agent: {agentInfo.Name}  quest: {quest.Definition.Title} Started.");
        }

        protected virtual void OnAgentCompletedQuest(AgentInfo agentInfo, Sansar.Simulation.Quest quest)
        {
            SimpleLog(LogLevel.Info, $"agent: {agentInfo.Name}  quest: {quest.Definition.Title} Completed.");
        }

        protected virtual void OnAgentResetQuest(AgentInfo agentInfo, Sansar.Simulation.Quest quest)
        {
            SimpleLog(LogLevel.Info, $"agent: {agentInfo.Name}  quest: {quest.Definition.Title} Reset.");
        }

        protected SimpleData GetEventData(AgentInfo agentInfo)
        {
            SimpleData data = new SimpleData(this);
            data.AgentInfo = agentInfo;
            return data;
        }
    }
}
