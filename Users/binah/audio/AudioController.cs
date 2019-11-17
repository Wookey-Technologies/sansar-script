// � 2019 Linden Research, Inc.

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

    [Tooltip(@"The loudness the stream will be played at.")]
    [DefaultValue(50.0f)]
    [Range(0.0f, 100.0f)]
    [DisplayName("Loudness")]
    public readonly float AudioStreamLoudness;

    [DefaultValue(true)]
    public bool DebugLogging;

    private HashSet<string> _allowedAgents;
    private Dictionary<string, string> _commandsUsage;

    private AudioComponent audio;

    public override void Init()
    {
        ObjectPrivate.TryGetFirstComponent(out audio);

        _allowedAgents = SplitStringIntoHashSet(AllowedAgents);

        _commandsUsage = new Dictionary<string, string>
        {
            { "/help", "" },
            { "/stream", "[url]" },
            { "/stopaudio", "" },
            { "/mute", ""}
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


    //Chat Commands

    private void onShowHelp(AgentPrivate agent)
    {
        string helpMessage = "Audio Control Command usage:";
        foreach (var kvp in _commandsUsage)
        {
            helpMessage += "\n" + kvp.Key + " " + kvp.Value;
        }

        try
        {
            agent.SendChat(helpMessage);
        }
        catch
        {
            // Agent left
            if (DebugLogging) Log.Write("Possible race condition and they already logged off.");
        }
    }

    void onStream(AgentPrivate agent, string medialUrl)
    {
        bool throttled = false;

        try
        {
            ScenePrivate.OverrideAudioStream(medialUrl);

            agent.SendChat("Audio Stream URL successfully updated to " + medialUrl);
            if (DebugLogging)  Log.Write("New audio stream URL: " + medialUrl);
        } catch
        {
            throttled = true;
            if (DebugLogging)  Log.Write("Throttled: Unable to update audio stream URL.");
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

    void OnChat(ChatData data)
    {
        AgentPrivate agent = ScenePrivate.FindAgent(data.SourceId);

        if (!IsAccessAllowed(agent))
            return;

        string[] chatWords = data.Message.Split(' ');

        if (chatWords.Length < 1)
        {
            if (DebugLogging) Log.Write("Chat message not long enough.");
            return;
        }

        string command = chatWords[0];

        if (!_commandsUsage.ContainsKey(command))
        {
            if (DebugLogging) Log.Write("Command not found in dictionary." + command);
            return;
        }

        if (DebugLogging) Log.Write("Command = " + command);

        switch (command)
        {
            case "/help":
                onShowHelp(agent);
                break;
            case "/stream":
                onStream(agent, chatWords.Length < 2 ? "" : chatWords[1]);
                break;
            default: break;
        }

    }

    float LoudnessPercentToDb(float loudnessPercent)
    {
        loudnessPercent = Math.Min(Math.Max(loudnessPercent, 0.0f), 100.0f);
        return 60.0f * (loudnessPercent / 100.0f) - 48.0f;
    }
}