// © 2019 Linden Research, Inc.

using Sansar.Script;
using Sansar.Simulation;
using System;
using System.Collections.Generic;

public class AudioStreamChatCommand : SceneObjectScript
{
    [Tooltip("All users can type media chat commands if this is on")]
    [DisplayName("Restricted Access")]
    [DefaultValue(true)]
    public bool RestrictedAccess;

    [Tooltip("Comma separated list of user handles for those granted restricted access")]
    [DisplayName("Restricted Access User Handles")]
    public string AllowedAgents;

    private HashSet<string> _allowedAgents;
    private Dictionary<string, string> _commandsUsage;

    public override void Init()
    {
        _allowedAgents = SplitStringIntoHashSet(AllowedAgents);

        _commandsUsage = new Dictionary<string, string>
        {
            { "/help", "" },
            { "/stream", "[url]" },
        };

        ScenePrivate.Chat.Subscribe(Chat.DefaultChannel, OnChat);
    }

    HashSet<string> SplitStringIntoHashSet(string commaSeparatedString)
    {
        HashSet<string> hash = new HashSet<string>();

        string[] separateStrings = commaSeparatedString.Split(',');
        for (int i = 0; i < separateStrings.Length; ++i)
        {
            string separateString = separateStrings[i].Trim();
            if (!string.IsNullOrWhiteSpace(separateString))
            {
                hash.Add(separateString);
            }
        }

        return hash;
    }

    bool IsAccessAllowed(AgentPrivate agent)
    {
        if (RestrictedAccess)
        {
            bool accessAllowed = false;

            if (agent != null)
            {
                // Always allow the creator of the scene
                accessAllowed |= (agent.AgentInfo.AvatarUuid == ScenePrivate.SceneInfo.AvatarUuid);

                // Authenticate other allowed agents
                accessAllowed |= _allowedAgents.Contains(agent.AgentInfo.Handle);
            }

            return accessAllowed;
        }

        return true;
    }

    void OnChat(ChatData data)
    {
        AgentPrivate agent = ScenePrivate.FindAgent(data.SourceId);
        if (!IsAccessAllowed(agent))
            return;

        string[] chatWords = data.Message.Split(' ');

        if (chatWords.Length < 1)
            return;

        string command = chatWords[0];

        if (!_commandsUsage.ContainsKey(command))
            return;

        if (command == "/help")
        {
            string helpMessage = "AudioStreamChatCommand usage:";
            foreach (var kvp in _commandsUsage)
            {
                helpMessage += "\n" + kvp.Key + " " + kvp.Value;
            }
            agent.SendChat(helpMessage);
            return;
        }

        // if (command == "/stream")
        string medialUrl = (chatWords.Length < 2 ? "" : chatWords[1]);

        bool throttled = false;

        try
        {
            ScenePrivate.OverrideAudioStream(medialUrl);

            agent.SendChat("Audio Stream URL successfully updated to " + medialUrl);
            Log.Write("New audio stream URL: " + medialUrl);
        }
        catch (ThrottleException)
        {
            throttled = true;
            Log.Write("Throttled: Unable to update audio stream URL.");
        }
        catch
        {
            // Agent left
        }

        if (throttled)
        {
            try
            {
                agent.SendChat("Audio stream URL update was throttled.  Try again.");
            }
            catch
            {
                // Agent left
            }
        }
    }
}