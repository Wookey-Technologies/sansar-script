using Sansar.Script;
using Sansar.Simulation;

public class LowGravityChatCommandScript : SceneObjectScript
{
    public override void Init()
    {
        ScenePrivate.Chat.Subscribe(Chat.DefaultChannel, (ChatData data) =>
        {
            if (data.Message == "/setlowgrav")
            {
                AgentPrivate agent = ScenePrivate.FindAgent(data.SourceId);

                // If the agent is the scene owner
                if ((agent != null) && (agent.AgentInfo.AvatarUuid == ScenePrivate.SceneInfo.AvatarUuid))
                {
                    // Set the gravity to 15% of earth gravity
                    ScenePrivate.SetGravity(0.15f * 9.81f);

                    // Send a private acknowledgement message back to the user
                    agent.SendChat("You set low gravity!");
                }
            }
        });
    }
}
