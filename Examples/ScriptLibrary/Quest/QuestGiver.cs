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
    [Tooltip("Controls a quest based on script events.")]
    [DisplayName("Quest Giver Controller")]
    public class QuestGiver : QuestGiverBase
    {
        #region EditorProperties

        [Tooltip(@"Offer the quest to a single user. Must be events that contain agent data, e.g events sent from the Interaction Library script. Can be a comma separated list of event names.")]
        [DefaultValue("offer")]
        [DisplayName("-> Offer")]
        public readonly string OfferEvent;

        [Tooltip(@"Offer the quest immediatley when a user joins the experience, as long as the quest is available to them.")]
        [DefaultValue(false)]
        [DisplayName("Offer on join")]
        public readonly bool OfferOnSceneEnter;

        [Tooltip(@"Complete the quest for a single user. Must be events that contain agent data, e.g events sent from the Interaction Library script. Can be a comma separated list of event names.")]
        [DefaultValue("complete")]
        [DisplayName("-> Complete")]
        public readonly string CompleteEvent;

        [Tooltip(@"Events to send if the quest is available to a user, and hasn't been offered to them. Can be a comma separated list of event names.")]
        [DefaultValue("isAvailable")]
        [DisplayName("Available ->")]
        public readonly string AvailableEvent;

        [Tooltip(@"Events to send when a user is offered the quest. Can be a comma separated list of event names.")]
        [DefaultValue("isOffered")]
        [DisplayName("Offered ->")]
        public readonly string OfferedEvent;

        [Tooltip(@"Events to send if a user accepts the quest when it is offered to them. Can be a comma separated list of event names.")]
        [DefaultValue("isActive")]
        [DisplayName("Started ->")]
        public readonly string StartedEvent;

        [Tooltip(@"Events to send if a user completed the quest, either by completing objectives, or by this script receiving a complete event. Can be a comma separated list of event names.")]
        [DefaultValue("isComplete")]
        [DisplayName("On Completed ->")]
        public readonly string CompletedEvent;

        [Tooltip("Enable responding to events for this script")]
        [DefaultValue("quest_enable")]
        [DisplayName("-> Enable")]
        public readonly string EnableEvent;

        [Tooltip("Disable responding to events for this script")]
        [DefaultValue("quest_disable")]
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
                Unsubscribes = SubscribeToAll(CompleteEvent, CompleteQuest);
                Unsubscribes += SubscribeToAll(OfferEvent, OfferQuest);
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


        protected override void OnAgentJoinExperience(AgentInfo agentInfo, Sansar.Simulation.Quest quest)
        {
            base.OnAgentJoinExperience(agentInfo, quest);
            if (OfferOnSceneEnter)
            {
                OfferToAgent(agentInfo.SessionId);
            }
        }

        protected override void OnAgentQuestAvailable(AgentInfo agentInfo, Sansar.Simulation.Quest quest)
        {
            base.OnAgentQuestAvailable(agentInfo, quest);
            SendToAll(AvailableEvent, GetEventData(agentInfo));
        }

        protected override void OnAgentOfferedQuest(AgentInfo agentInfo, Sansar.Simulation.Quest quest)
        {
            base.OnAgentOfferedQuest(agentInfo, quest);
            SendToAll(OfferedEvent, GetEventData(agentInfo));
        }

        protected override void OnAgentStartedQuest(AgentInfo agentInfo, Sansar.Simulation.Quest quest)
        {
            base.OnAgentStartedQuest(agentInfo, quest);
            SendToAll(StartedEvent, GetEventData(agentInfo));
        }

        protected override void OnAgentCompletedQuest(AgentInfo agentInfo, Sansar.Simulation.Quest quest)
        {
            base.OnAgentCompletedQuest(agentInfo, quest);
            SendToAll(CompletedEvent, GetEventData(agentInfo));
        }
    }
}
