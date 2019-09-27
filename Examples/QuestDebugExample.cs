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

namespace QuestDebugExample
{
    [Tooltip("Use to help test quests, by typing commands in the chat window.")]
    public class QuestDebug : SceneObjectScript
    {
        [Tooltip("Hook this up to a quest you created in the quest creator tool.")]
        public QuestDefinition QuestDefinition;

        [Tooltip("Type this in chat to offer the quest to yourself.")]
        [DefaultValue("offer")]
        public string OfferQuestChatCommand;

        [Tooltip("Type this in chat, followed by an objective number, to complete an objective. E.g. complete 1 to complete the first objective.")]
        [DefaultValue("complete")]
        public string CompleteObjectiveChatCommand;

        [Tooltip("Type this in chat, followed by an objective number, to lock an objective. E.g. lock 1 to lock the first objective.")]
        [DefaultValue("lock")]
        public string LockObjectiveChatCommand;

        [Tooltip("Type this in chat, followed by an objective number, to unlock an objective. E.g. active 1 to unlock the first objective.")]
        [DefaultValue("active")]
        public string SetObjectiveActiveChatCommand;

        [Tooltip("Type this in chat to log the full quest status to the chat window.")]
        [DefaultValue("status")]
        public string LogStatusChatCommand;

        [Tooltip("Enable this if you want to automatically offer the quest when joining the scene.")]
        [DefaultValue(false)]
        public bool OfferOnSceneEnter;

        public override void Init()
        {
            ScenePrivate.User.Subscribe(User.AddUser, OnAddUser);
            ScenePrivate.Chat.Subscribe(Sansar.Simulation.Chat.DefaultChannel, OnChat);

            ScenePrivate.Chat.MessageAllUsers("Getting quest definition data...");
            Log.Write("Getting quest definition data...");

            var init = WaitFor(QuestDefinition.Update);
            string initMessage = "";
            if (init.Success)
            {
                initMessage = (QuestDefinition.Title + " loaded definition");
            }
            else
            {
                initMessage = " failed to load definition";
            }
            ScenePrivate.Chat.MessageAllUsers(initMessage);
            Log.Write(initMessage);
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
                OnAgentJoinExperience(agentInfo, quest);

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

                foreach (var objective in quest.Objectives)
                {
                    objective.Subscribe(ObjectiveState.Active, (ObjectiveData d) => ChatLogQuest(agentInfo.SessionId, quest));
                    objective.Subscribe(ObjectiveState.Locked, (ObjectiveData d) => ChatLogQuest(agentInfo.SessionId, quest));
                    objective.Subscribe(ObjectiveState.Completed, (ObjectiveData d) => ChatLogQuest(agentInfo.SessionId, quest));
                }

                if (OfferOnSceneEnter && (quest.GetState() == QuestState.None || quest.GetState() == QuestState.Offered))
                {
                    OfferToAgent(agentInfo.SessionId);
                }
            }
        }

        protected void OfferToAgent(SessionId sessionId)
        {
            var questData = WaitFor(QuestDefinition.GetQuest, sessionId) as QuestDefinition.GetQuestData;
            if (questData.Success)
            {
                questData.Quest.Offer();
            }
            else
            {
                ChatLog(sessionId, $"Failed to get quest data");
            }
        }

        protected void CompleteNextObjective(SessionId sessionId)
        {
            var questData = WaitFor(QuestDefinition.GetQuest, sessionId) as QuestDefinition.GetQuestData;
            if (questData.Success)
            {
                foreach(var objective in questData.Quest.Objectives)
                {
                    if (objective.GetState() != ObjectiveState.Completed)
                    {
                        objective.SetState(ObjectiveState.Completed);
                        break;
                    }
                }
            }
            else
            {
                ChatLog(sessionId, $"Failed to get quest data");
            }
        }

        void SetObjectiveState(SessionId sessionId, int objectiveIndex, ObjectiveState state)
        {
            var questData = WaitFor(QuestDefinition.GetQuest, sessionId) as QuestDefinition.GetQuestData;
            if (questData.Success )
            {
                var objectives = questData.Quest.Objectives;
                if (objectives.Count() > objectiveIndex)
                {
                    objectives[objectiveIndex].SetState(state);
                }
            }
            else
            {
                ChatLog(sessionId, $"Failed to get objective data");
            }
        }


        void OnChat( ChatData chatData)
        {
            string[] split = chatData.Message.Split(' ');
            string command = split[0];

            int index = -1;
            if (split.Length > 1)
            {
                if (int.TryParse(split[1], out index))
                {
                    index = index - 1; // start at 0
                }
            }
            if (command == OfferQuestChatCommand)
            {
                ChatLog(chatData.SourceId, $"offering quest...");
                OfferToAgent(chatData.SourceId);
            }
            else if (command == CompleteObjectiveChatCommand)
            {
                if (index >= 0)
                {
                    ChatLog(chatData.SourceId, $"completing objective {index}...");
                    SetObjectiveState(chatData.SourceId, index, ObjectiveState.Completed);
                }
                else
                {
                    ChatLog(chatData.SourceId, $"completing next objective");
                    CompleteNextObjective(chatData.SourceId);
                }
            }
            else if (command == LockObjectiveChatCommand)
            {
                if (index >= 0)
                {
                    ChatLog(chatData.SourceId, $"setting objective {index} locked...");
                    SetObjectiveState(chatData.SourceId, index, ObjectiveState.Locked);
                }
                else
                {
                    ChatLog(chatData.SourceId, $"Please supply objective number, eg: {LockObjectiveChatCommand} 1");
                }
            }
            else if (command == SetObjectiveActiveChatCommand)
            {
                if (index >= 0)
                {
                    ChatLog(chatData.SourceId, $"setting objective {index} active...");
                    SetObjectiveState(chatData.SourceId, index, ObjectiveState.Active);
                }
                else
                {
                    ChatLog(chatData.SourceId, $"Please supply objective number, eg: {SetObjectiveActiveChatCommand} 1");
                }
            }
            else if (command == LogStatusChatCommand)
            {
                var questData = WaitFor(QuestDefinition.GetQuest, chatData.SourceId) as QuestDefinition.GetQuestData;
                if (questData.Success)
                {
                    ChatLog(chatData.SourceId, $"quest status...");
                    ChatLogQuest(chatData.SourceId, questData.Quest);
                }
                else
                {
                    ChatLog(chatData.SourceId, $"Failed to get quest data");
                }
            }
            else
            {
                ChatLog(chatData.SourceId, $"unrecognized command: {command}");
            }
        }

        protected virtual void OnAgentJoinExperience(AgentInfo agentInfo, Sansar.Simulation.Quest quest)
        {
            ChatLog(agentInfo.SessionId, $"agent: {agentInfo.Name} joined experience.  quest: {quest.Definition.Title} state: {quest.GetState()}");
        }

        protected virtual void OnAgentQuestAvailable(AgentInfo agentInfo, Sansar.Simulation.Quest quest)
        {
            ChatLog(agentInfo.SessionId, $"agent: {agentInfo.Name}  quest: {quest.Definition.Title} Available.");
            ChatLogQuest(agentInfo.SessionId, quest);
        }

        protected virtual void OnAgentOfferedQuest(AgentInfo agentInfo, Sansar.Simulation.Quest quest)
        {
            ChatLog(agentInfo.SessionId, $"agent: {agentInfo.Name}  quest: {quest.Definition.Title} Offered.");
            ChatLogQuest(agentInfo.SessionId, quest);
        }

        protected virtual void OnAgentStartedQuest(AgentInfo agentInfo, Sansar.Simulation.Quest quest)
        {
            ChatLog(agentInfo.SessionId, $"agent: {agentInfo.Name}  quest: {quest.Definition.Title} Started.");
            ChatLogQuest(agentInfo.SessionId, quest);
        }

        protected virtual void OnAgentCompletedQuest(AgentInfo agentInfo, Sansar.Simulation.Quest quest)
        {
            ChatLog(agentInfo.SessionId, $"agent: {agentInfo.Name}  quest: {quest.Definition.Title} Completed.");
            ChatLogQuest(agentInfo.SessionId, quest);
        }

        protected virtual void OnAgentFailedQuest(AgentInfo agentInfo, Sansar.Simulation.Quest quest)
        {
            ChatLog(agentInfo.SessionId, $"agent: {agentInfo.Name}  quest: {quest.Definition.Title} Failed.");
            ChatLogQuest(agentInfo.SessionId, quest);
        }

        void ChatLog(SessionId sessionId, string log)
        {
            Log.Write(log);
            ScenePrivate.FindAgent(sessionId)?.SendChat(log);
        }

        void ChatLogQuest(SessionId sessionId, Quest quest)
        {
            string questStateString = $"Quest: {quest.Definition.Title} is in state: {quest.GetState()}";

            ChatLog(sessionId, questStateString);

            var objectives = quest.Objectives;

            ChatLog(sessionId, $"Objective count: {objectives.Count()}");

            for (int i=0; i<objectives.Count(); i++)
            {
                string objectiveStateString = $"       Objective: {objectives[i].Definition.Title} is in state: {objectives[i].GetState()}";
                ChatLog(sessionId, objectiveStateString);
            }
        }
    }
}
