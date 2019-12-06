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
    [Tooltip("Controls a quest offered to users with an interaction.")]
    [DisplayName("Quest Giver Trigger")]
    public class QuestGiverTriggerVolume : QuestGiverBase
    {
        [Tooltip("If enabled the first objective will be activated when the quest is given.")]
        [DefaultValue(false)]
        [DisplayName("Activate First Objective")]
        public bool ActivateFirstObjective;

        [Tooltip("If enabled all objectives will be activated when the quest is given.")]
        [DefaultValue(false)]
        [DisplayName("Activate All Objectives")]
        public bool ActivateAllObjectives;

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

        private RigidBodyComponent triggerVolume;
        private bool enabled;
        protected override void SimpleInit()
        {
            if (!ObjectPrivate.TryGetFirstComponent<RigidBodyComponent>(out triggerVolume) || !triggerVolume.IsTriggerVolume())
            {
                Log.Write(LogLevel.Error, "QuestGiverTriggerVolume", "Must be attached to a trigger volume.");
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

                    var questData = WaitFor(QuestDefinition.GetQuest, sessionId) as QuestDefinition.GetQuestData;
                    try
                    {
                        if (questData.Success && (questData.Quest.GetState() == QuestState.None || questData.Quest.GetState() == QuestState.Offered))
                        {
                            questData.Quest.Offer();
                        }
                    }
                    catch (Exception) { }
                }
            }
        }

        protected override void OnAgentStartedQuest(AgentInfo agentInfo, Quest quest)
        {
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
        }
    }
}
