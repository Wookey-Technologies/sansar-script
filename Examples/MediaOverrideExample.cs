using System;

using Sansar;
using Sansar.Script;
using Sansar.Simulation;

public class MediaOverrideExample : SceneObjectScript
{
    private string CommandAgentMediaUrl        = "/myurl"; // e.g. /url https://www.youtube.com/embed/-tdkW64u3YA?autoplay=1&allowfullscreen=1
    private string CommandAgentMediaSizeAndUrl = "/mysizeurl"; // e.g. /sizeurl 1280 720 https://www.youtube.com/embed/-tdkW64u3YA?autoplay=1&allowfullscreen=1

    private string CommandSceneMediaUrl        = "/sceneurl"; // e.g. /url https://www.youtube.com/embed/-tdkW64u3YA?autoplay=1&allowfullscreen=1
    private string CommandSceneMediaSizeAndUrl = "/scenesizeurl"; // e.g. /sizeurl 1280 720 https://www.youtube.com/embed/-tdkW64u3YA?autoplay=1&allowfullscreen=1


    // utility function to check message against a command and number of parameters
    private bool CheckCommand(string[] tokens, string cmd, int requiredCount)
    {
        int numTokens = tokens.Length;
        if (numTokens > 0)
        {
            if (cmd.Equals(tokens[0], StringComparison.InvariantCultureIgnoreCase))
            {
                if (numTokens == requiredCount)
                {
                    return true;
                }
                else
                {
                    Log.Write(String.Format("Expected parameters for command {0}: {1}, actual: {2}", cmd, requiredCount - 1, numTokens - 1));
                }
            }
        }
        return false;
    }

    private void OnChat(ChatData data)
    {
        // Try to parse the message as a chat command and ignore it if it is not a known command
        var tokens = data.Message.Split(new char[] { ' ' });

        if (CheckCommand(tokens, CommandAgentMediaUrl, 2))
        {
            string url = tokens[1];
            AgentPrivate agent = ScenePrivate.FindAgent(data.SourceId);
            agent.OverrideMediaSource(url);
        }
        else if (CheckCommand(tokens, CommandAgentMediaSizeAndUrl, 4))
        {
            Int32 width = Int32.Parse(tokens[1]);
            Int32 height = Int32.Parse(tokens[2]);
            string url = tokens[3];
            AgentPrivate agent = ScenePrivate.FindAgent(data.SourceId);
            agent.OverrideMediaSource(url, width, height);
        }
        else if (CheckCommand(tokens, CommandSceneMediaUrl, 2))
        {
            string url = tokens[1];
            ScenePrivate.OverrideMediaSource(url);
        }
        else if (CheckCommand(tokens, CommandSceneMediaSizeAndUrl, 4))
        {
            Int32 width = Int32.Parse(tokens[1]);
            Int32 height = Int32.Parse(tokens[2]);
            string url = tokens[3];
            ScenePrivate.OverrideMediaSource(url, width, height);
        }
    } 

    public override void Init()
    {
        ScenePrivate.Chat.Subscribe(Chat.DefaultChannel, OnChat, true);
    }
}
