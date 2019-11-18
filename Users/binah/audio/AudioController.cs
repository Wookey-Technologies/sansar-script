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

    private PlayHandle scenePlayHandle;

    public override void Init()
    {
        _allowedAgents = SplitStringIntoHashSet(AllowedAgents);

        _commandsUsage = new Dictionary<string, string>
        {
            { "/help", "" },
            { "/stream", "[url - leave blank to stop stream]" },
            { "/volume", "[0-100 increments of 10]" }
        };

        ScenePrivate.Chat.Subscribe(Chat.DefaultChannel, OnChat);

        scenePlayHandle = ScenePrivate.PlayStream(StreamChannel.AudioChannel, LoudnessPercentToDb(AudioStreamLoudness));
        if (DebugLogging) Log.Write("setHandle " + scenePlayHandle);
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

    float LoudnessPercentToDb(float loudnessPercent)
    {
        if (DebugLogging) Log.Write("LoudnessPercentToDb " + loudnessPercent);

        loudnessPercent = Math.Min(Math.Max(loudnessPercent, 0.0f), 100.0f);

        if (DebugLogging) Log.Write("loudnessPerscent step 1 " + loudnessPercent);

        float returnVal = 60.0f * (loudnessPercent / 100.0f) - 48.0f;

        if (DebugLogging) Log.Write("loudnessPerscent out " + returnVal.ToString());

        return returnVal;
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

    void setVolume(float volume)
    {
        if (DebugLogging) Log.Write("setVolume " + volume);

        if((scenePlayHandle != null) && scenePlayHandle.IsPlaying())
        {
            scenePlayHandle.SetLoudness(LoudnessPercentToDb(volume));
        }
    }

    void onVolume(string volume)
    {
        if (DebugLogging) Log.Write("onVolume " + volume);

        float _volume = AudioStreamLoudness;

        switch (volume)
        {
            case "0":
                _volume = 0.0f;
                break;
            case "10":
                _volume = 10.0f;
                break;
            case "20":
                _volume = 20.0f;
                break;
            case "30":
                _volume = 30.0f;
                break;
            case "40":
                _volume = 40.0f;
                break;
            case "50":
                _volume = 50.0f;
                break;
            case "60":
                _volume = 60.0f;
                break;
            case "70":
                _volume = 70.0f;
                break;
            case "80":
                _volume = 80.0f;
                break;
            case "90":
                _volume = 90.0f;
                break;
            case "100":
                _volume = 100.0f;
                break;
            default:
                break;
        }

        setVolume(_volume);
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
            case "/volume":
                onVolume(chatWords[1]);
                break;
            default: break;
        }

    }


}