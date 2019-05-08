// © 2019 Linden Research, Inc.

using Sansar.Script;
using Sansar.Simulation;

public class AddInteractionScript : SceneObjectScript
{
    public string Title;

    public override void Init()
    {
        ObjectPrivate.AddInteractionData addData = (ObjectPrivate.AddInteractionData)WaitFor(ObjectPrivate.AddInteraction, Title, true);

        addData.Interaction.Subscribe((InteractionData data) =>
        {
            AgentPrivate agent = ScenePrivate.FindAgent(data.AgentId);

            if (agent != null)
            {
                agent.SendChat($"Hello from {Title}!");
            }
        });
    }
}
