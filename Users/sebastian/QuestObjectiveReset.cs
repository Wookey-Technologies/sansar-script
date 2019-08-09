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

public class QuestObjectiveReset : SceneObjectScript
{
    public Interaction ResetObjectiveInteraction;
    public ObjectiveDefinition PlayerObjectiveDefinition;

    [DefaultValue(true)]
    public bool LockObjective;

    public override void Init()
    {
        ResetObjectiveInteraction.SetPrompt($"Reset {PlayerObjectiveDefinition.Title}");
        ResetObjectiveInteraction.Subscribe(OnClick);
    }

    public void OnClick(InteractionData data)
    {
        // Find the agent who clicked our button
        AgentPrivate agent = ScenePrivate.FindAgent(data.AgentId);

        // Get the Objective data for that agent
        Objective PlayerObjective = (WaitFor(PlayerObjectiveDefinition.GetObjective, agent) as ObjectiveDefinition.GetObjectiveData).Objective;

        ObjectiveState DesiredState = LockObjective ? ObjectiveState.Locked : ObjectiveState.Active;
        WaitFor(PlayerObjective.SetState, DesiredState);
    }

}