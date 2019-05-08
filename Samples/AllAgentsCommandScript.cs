// © 2019 Linden Research, Inc.

using Sansar;
using Sansar.Script;
using Sansar.Simulation;

public class AllAgentsCommandScript : SceneObjectScript
{
    public override void Init()
    {
        // Find each user as they enter the scene and list for commands from them
        ScenePrivate.User.Subscribe(User.AddUser, (UserData ud) =>
        {
            AgentPrivate agent = ScenePrivate.FindAgent(ud.User);
            if (agent != null)
                ListenForCommand(agent);
        });

        // Listen for commands from any users already in the scene
        foreach (var agent in ScenePrivate.GetAgents())
            ListenForCommand(agent);
    }

    void ListenForCommand(AgentPrivate agent)
    {
        agent.Client.SubscribeToCommand("Trigger", CommandAction.Pressed, (CommandData command) =>
        {
            Log.Write(agent.AgentInfo.Name + " pressed trigger!");
        },
        (canceledData) => { });
    }
}
