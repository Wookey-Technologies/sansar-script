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
    [DefaultScript]
    [Tooltip("Controls a quest offered to users with an interaction.")]
    [DisplayName("Quest Giver Interaction")]
    public class QuestGiverInteraction : QuestGiverBase
    {
        [Tooltip(@"Offer Quest Interaction")]
        [DefaultValue("Click Me!")]
        [DisplayName("Offer Quest Interaction")]
        public Sansar.Simulation.Interaction Interaction;

        [Tooltip("If enabled the first objective will be activated when the quest is given.")]
        [DefaultValue(false)]
        [DisplayName("Activate First Objective")]
        public bool ActivateFirstObjective;

        [Tooltip("If enabled all objectives will be activated when the quest is given.")]
        [DefaultValue(false)]
        [DisplayName("Activate All Objectives")]
        public bool ActivateAllObjectives;

        [Tooltip(@"Events to send if a user accepts the quest when it is offered to them. Can be a comma separated list of event names.")]
        [DefaultValue("isActive")]
        [DisplayName("Started ->")]
        public readonly string StartedEvent;

        [Tooltip(@"Events to send if a user completed the quest by completing objectives. Can be a comma separated list of event names.")]
        [DefaultValue("isComplete")]
        [DisplayName("On Completed ->")]
        public readonly string CompletedEvent;

        protected override void SimpleInit()
        {
            base.SimpleInit();

            if (Interaction == null)
            {
                Log.Write(LogLevel.Error, "No interaction");
                return;
            }
            Interaction.Subscribe((InteractionData data) =>
            {
                OfferToAgent(data.AgentId);
            });
            Interaction.SetEnabled(false);
        }

        protected override void OnAgentJoinExperience(AgentInfo agentInfo, Quest quest)
        {
            base.OnAgentJoinExperience(agentInfo, quest);
            try
            {
                if (quest.GetState() == QuestState.Offered || quest.GetState() == QuestState.None)
                {
                    Interaction.SetEnabled(agentInfo.SessionId, true);
                }
            } catch (Exception) { }
}

        protected override void OnAgentResetQuest(AgentInfo agentInfo, Quest quest)
        {
            base.OnAgentResetQuest(agentInfo, quest);
            Interaction.SetEnabled(agentInfo.SessionId, true);
        }

        protected override void OnAgentQuestAvailable(AgentInfo agentInfo, Quest quest)
        {
            base.OnAgentQuestAvailable(agentInfo, quest);
            Interaction.SetEnabled(agentInfo.SessionId, true);
        }


        protected override void OnAgentStartedQuest(AgentInfo agentInfo, Quest quest)
        {
            base.OnAgentStartedQuest(agentInfo, quest);
            Interaction.SetEnabled(agentInfo.SessionId, false);

            if (ActivateFirstObjective || ActivateAllObjectives)
            {
                try
                {
                    var objectives = quest.Objectives;
                    if (objectives.Length == 0)
                    {
                        return;
                    }
                    int count = ActivateAllObjectives ? objectives.Length : 1;
                    for (int i = 0; i < count; i++)
                    {
                        if (objectives[i].GetState() == ObjectiveState.Locked)
                        {
                            objectives[i].SetState(ObjectiveState.Active);
                        }
                    }
                }
                catch (Exception) { }
            }

            SendToAll(StartedEvent, GetEventData(agentInfo));
        }

        protected override void OnAgentCompletedQuest(AgentInfo agentInfo, Sansar.Simulation.Quest quest)
        {
            base.OnAgentCompletedQuest(agentInfo, quest);
            SendToAll(CompletedEvent, GetEventData(agentInfo));
        }
    }
}
