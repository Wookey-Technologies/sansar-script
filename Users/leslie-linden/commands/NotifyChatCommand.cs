using Sansar.Script;
using Sansar.Simulation;

public class NotifyChatCommand : SceneObjectScript
{
    // Public properties

    // This script can post modal notifications or system chat messages.
    // By default we want this script to use modal notifications
    [DefaultValue(true)]
    public readonly bool ModalNotifications;


    // Logic!

    public override void Init()
    {
        // Register the "OnChat" function to be called whenever a nearby chat message arrives
        ScenePrivate.Chat.Subscribe(Chat.DefaultChannel, OnChat);
    }

    void OnChat(ChatData data)
    {
        // Find the agent who wrote this chat message
        AgentPrivate owner = ScenePrivate.FindAgent(data.SourceId);

        // If the agent is the scene owner
        if ((owner != null) && (owner.AgentInfo.AvatarUuid == ScenePrivate.SceneInfo.AvatarUuid))
        {
            if (data.Message.StartsWith("/notify "))
            {
                // The message is everything after the "/notify " prefix
                var message = data.Message.Substring(8);
                if (string.IsNullOrWhiteSpace(message))
                    return;

                if (ModalNotifications)
                {
                    // Go through each agent in the scene
                    foreach (var agent in ScenePrivate.GetAgents())
                    {
                        // try/catch is usually a good idea when dealing with agents
                        try
                        {
                            // Display the message in a modal dialog
                            agent.Client.UI.ModalDialog.Show(message, "Ok", "");
                        }
                        catch {}
                    }
                }
                else
                {
                    // Just send a system message to chat instead
                    ScenePrivate.Chat.MessageAllUsers(message);
                }
            }
            else if (data.Message == "/help")
            {
                try
                {
                    // Let the owner know the notify chat command is available to them in the scene
                    owner.SendChat("/notify [message]");
                }
                catch {}
            }
        }
    }
}

