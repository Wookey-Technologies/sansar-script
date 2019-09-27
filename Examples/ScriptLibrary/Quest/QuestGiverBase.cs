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

        HashSet<SessionId> activeSessionIds = new HashSet<SessionId>();

        protected override void SimpleInit()
        {
            var addUserSubscription = ScenePrivate.User.Subscribe(User.AddUser, OnAddUser);

            // for edge case where users can join scene before AddUser subscription is added
            foreach (var agent in ScenePrivate.GetAgents())
            {
                OnAddAgent(agent);
            }

            var removeUserSubscription = ScenePrivate.User.Subscribe(User.RemoveUser, (UserData data)=>
            {
                activeSessionIds.Remove(data.User);
            });

            if (!VerifyDefinition())
            {
                addUserSubscription.Unsubscribe();
                removeUserSubscription.Unsubscribe();
            }
        }

        bool VerifyDefinition()
        {
            if (QuestDefinition == null)
            {
                Log.Write(LogLevel.Error, "Quest Definition not found.");
                return false;
            }

            if (QuestDefinition.Ready)
            {
                return true;
            }

            var update = WaitFor(QuestDefinition.Update);

            if (update.Success)
            {
                SimpleLog(LogLevel.Info, $"Got quest definition: {QuestDefinition.Title}");
                return true;
            }
            else
            {
                Log.Write(LogLevel.Error, "Failed to update quest definition.");
                return false;
            }
        }

        void OnAddUser(UserData data)
        {
            AgentPrivate agent = ScenePrivate.FindAgent(data.User);
            OnAddAgent(agent);
        }

        void OnAddAgent(AgentPrivate agent)
        { 
            if (agent == null || !agent.IsValid)
            {
                return;
            }

            Quest quest = null;
            AgentInfo agentInfo = null;

            try
            {
                agentInfo = agent.AgentInfo;
            }
            catch
            {
                Log.Write(LogLevel.Error, "Failed to get agent info, user may have left experience.");
                return;
            }

            if (!VerifyDefinition())
            {
                return;
            }

            var questData = WaitFor(QuestDefinition.GetQuest, agentInfo.SessionId) as QuestDefinition.GetQuestData;
            if (questData.Success)
            {
                quest = questData.Quest;
            }

            if (quest != null && !activeSessionIds.Contains(agentInfo.SessionId))
            {
                try
                {
                    activeSessionIds.Add(agentInfo.SessionId);
                    SimpleLog(LogLevel.Info, $"agent: {agentInfo.Name}  quest: {quest.Definition.Title} state: {quest.GetState()}");

                    switch (quest.GetState())
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
                catch (Exception) { }
            }
        }

        protected void OfferQuest(ScriptEventData sed)
        {
            try { GetQuestFromScriptEventData(sed)?.Offer(); } catch (Exception) { }
        }

        protected void CompleteQuest(ScriptEventData sed)
        {
            try { GetQuestFromScriptEventData(sed)?.SetState(QuestState.Completed); } catch (Exception) { }
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
                try { questData.Quest.Offer(); } catch (Exception) { }
            }
        }

        protected virtual void OnAgentJoinExperience(AgentInfo agentInfo, Sansar.Simulation.Quest quest)
        {
        }

        protected virtual void OnAgentQuestAvailable(AgentInfo agentInfo, Sansar.Simulation.Quest quest)
        {
            try { SimpleLog(LogLevel.Info, $"agent: {agentInfo.Name}  quest: {quest.Definition.Title} Available."); } catch (Exception) { }
        }

        protected virtual void OnAgentOfferedQuest(AgentInfo agentInfo, Sansar.Simulation.Quest quest)
        {
            try { SimpleLog(LogLevel.Info, $"agent: {agentInfo.Name}  quest: {quest.Definition.Title} Offered."); } catch (Exception) { }
        }

        protected virtual void OnAgentStartedQuest(AgentInfo agentInfo, Sansar.Simulation.Quest quest)
        {
            try { SimpleLog(LogLevel.Info, $"agent: {agentInfo.Name}  quest: {quest.Definition.Title} Started."); } catch (Exception) { }
        }

        protected virtual void OnAgentCompletedQuest(AgentInfo agentInfo, Sansar.Simulation.Quest quest)
        {
            try { SimpleLog(LogLevel.Info, $"agent: {agentInfo.Name}  quest: {quest.Definition.Title} Completed."); } catch (Exception) { }
        }

        protected virtual void OnAgentResetQuest(AgentInfo agentInfo, Sansar.Simulation.Quest quest)
        {
            try { SimpleLog(LogLevel.Info, $"agent: {agentInfo.Name}  quest: {quest.Definition.Title} Reset."); } catch (Exception) { }
        }

        protected SimpleData GetEventData(AgentInfo agentInfo)
        {
            SimpleData data = new SimpleData(this);
            data.AgentInfo = agentInfo;
            return data;
        }
    }
}
