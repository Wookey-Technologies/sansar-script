/* This content is licensed under the terms of the Creative Commons Attribution 4.0 International License.
 * When using this content, you must:
 * •    Acknowledge that the content is from the Sansar Knowledge Base.
 * •    Include our copyright notice: "© 2017 Linden Research, Inc."
 * •    Indicate that the content is licensed under the Creative Commons Attribution-Share Alike 4.0 International License.
 * •    Include the URL for, or link to, the license summary at https://creativecommons.org/licenses/by-sa/4.0/deed.hi (and, if possible, to the complete license terms at https://creativecommons.org/licenses/by-sa/4.0/legalcode.
 * For example:
 * "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."
 */

using Sansar.Script;
using Sansar.Simulation;
using System;

public class QuestReset : SceneObjectScript
{
    public Interaction ResetQuestInteraction;
    public QuestDefinition PlayerQuestDefinition;

    public override void Init()
    {
        ResetQuestInteraction.SetPrompt($"Reset {PlayerQuestDefinition.Title}");
        ResetQuestInteraction.Subscribe(OnClick);
    }

    public void OnClick(InteractionData data)
    {
        // Find the agent who clicked our button
        AgentPrivate agent = ScenePrivate.FindAgent(data.AgentId);

        // Get the Quest data for that agent
        Quest PlayerQuest = (WaitFor(PlayerQuestDefinition.GetQuest, agent) as QuestDefinition.GetQuestData).Quest;

        // Reset the state of each objective in the quest
        for (int i = 0; i < PlayerQuest.Objectives.Length; i++)
        {
            Objective PlayerObjective = PlayerQuest.Objectives[i];
            ObjectiveState DesiredState = (i == 0 ? ObjectiveState.Active : ObjectiveState.Locked);
            WaitFor(PlayerObjective.SetState, DesiredState);
        }

        // Re-offer the quest
        WaitFor(PlayerQuest.SetState, QuestState.Offered);
    }

}