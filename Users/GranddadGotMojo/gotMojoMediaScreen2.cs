//* "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."

#define SansarBuild

using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

using Sansar;
using Sansar.Script;
using Sansar.Simulation;

public class GotMojoMediaScreen2 : SceneObjectScript
{
    #region ConstantsVariables

    [DefaultValue(30)]
    [DisplayName("Initial Volume:")]
    public int volume = 30;

    [DefaultValue("ALL")]
    [DisplayName("Valid User List:")]
    public string UsersToListenTo = "ALL";

    [DefaultValue(1080)]
    [DisplayName("Screen Height: ")]
    public int ScreenHeight = 1080;

    [DefaultValue(1920)]
    [DisplayName("Screen Width:")]
    public int ScreenWidth = 1920;

    [DisplayName("Screen ID:")]
    public string MediaScreenID = null;

    public string Play1 = null;
    public string Play2 = null;
    public string Play3 = null;
    public string Play4 = null;
    public string Play5 = null;
    public string Play6 = null;
    public string Play7 = null;
    public string Play8 = null;
    public string Play9 = null;
    public string Play10 = null;
    public string Play11 = null;
    public string Play12 = null;
    public string Play13 = null;
    public string Play14 = null;
    public string Play15 = null;

    private List<string> PlayList = new List<string>();

    private AudioComponent audio;
    private PlayHandle currentPlayHandle;

    private List<string> ValidUsers = new List<string>();
    private string video = null;
    private string videoIn = null;
    private string EmbedVideoID = null;
    DateTime VideoStartTime = DateTime.Now;
    private double VideoCurrentTime = 0;
    private int intVideoCurrentTime = 0;
    //private double NewDb = 0;
    //private double LastDb = 0;
    //private double volume = 0;
    //private double LastVolume = 30;  //assume that starting volume is halfway between max and min
    //private PlayHandle myVolume;
    private bool IsWatchFormat = false;
    private int playListPosition = 1;
    private bool keepPlaying;

    private string Errormsg = "No Errors";
    private bool strErrors = false;
    private SessionId Jammer = new SessionId();
    //private AgentInfo AgentName;
    private Action SimpleScriptSubscription;
    AgentInfo AgentName;
    ObjectId ComponentID;

    #endregion

    public override void Init()
    {

        //myVolume = ScenePrivate.PlayStream(StreamChannel.MediaChannel, 0);
        ObjectPrivate.TryGetFirstComponent(out audio);
        if (audio == null)
        {
            Log.Write("Did Not Find Audio Component");
            ScenePrivate.Chat.MessageAllUsers("Media Screen for Youtube Viewer requires an Audio Emitter");
        }
        else
        {
            Log.Write("Found Audio Component");
            currentPlayHandle = audio.PlayStreamOnComponent(StreamChannel.MediaChannel, volume);
        }

        Log.Write("Script Started");
        PlayList.Clear();
        if (Play1.Length > 0) PlayList.Add(Play1);
        if (Play2.Length > 0) PlayList.Add(Play2);
        if (Play3.Length > 0) PlayList.Add(Play3);
        if (Play4.Length > 0) PlayList.Add(Play4);
        if (Play5.Length > 0) PlayList.Add(Play5);
        if (Play6.Length > 0) PlayList.Add(Play6);
        if (Play7.Length > 0) PlayList.Add(Play7);
        if (Play8.Length > 0) PlayList.Add(Play8);
        if (Play9.Length > 0) PlayList.Add(Play9);
        if (Play10.Length > 0) PlayList.Add(Play10);
        if (Play11.Length > 0) PlayList.Add(Play11);
        if (Play12.Length > 0) PlayList.Add(Play12);
        if (Play13.Length > 0) PlayList.Add(Play13);
        if (Play14.Length > 0) PlayList.Add(Play14);
        if (Play15.Length > 0) PlayList.Add(Play15);

        Script.UnhandledException += UnhandledException; // Catch errors and keep running unless fatal
        //ScenePrivate.Chat.MessageAllUsers(WelcomeMessage);
        ScenePrivate.Chat.Subscribe(0, GetChatCommand);
        SubscribeToScriptEvent("SendPlayShuffle", GetCommands);
        SubscribeToScriptEvent("SendPlayList", GetCommands);
        SubscribeToScriptEvent("SendStop", GetCommands);
        SubscribeToScriptEvent("SendPrevious", GetCommands);
        SubscribeToScriptEvent("SendPause", GetCommands);
        SubscribeToScriptEvent("SendResume", GetCommands);
        SubscribeToScriptEvent("SendNext", GetCommands);
        SubscribeToScriptEvent("SendForceVideo", GetCommands);
        SubscribeToScriptEvent("SendVolumeDown", GetCommands);
        SubscribeToScriptEvent("SendVolumeUp", GetCommands);
        SubscribeToScriptEvent("SendEject", GetCommands);
        SubscribeToScriptEvent("play1", GetCommands);
        SubscribeToScriptEvent("play2", GetCommands);
        SubscribeToScriptEvent("play3", GetCommands);
        SubscribeToScriptEvent("play4", GetCommands);
        SubscribeToScriptEvent("play5", GetCommands);
        SubscribeToScriptEvent("play6", GetCommands);
        SubscribeToScriptEvent("play7", GetCommands);
        SubscribeToScriptEvent("play8", GetCommands);
        SubscribeToScriptEvent("play9", GetCommands);
        SubscribeToScriptEvent("play10", GetCommands);
        SubscribeToScriptEvent("play11", GetCommands);
        SubscribeToScriptEvent("play12", GetCommands);
        SubscribeToScriptEvent("play13", GetCommands);
        SubscribeToScriptEvent("play14", GetCommands);
        SubscribeToScriptEvent("play1", GetCommands);
    }

    public void GetCommands(ScriptEventData data)
    {
        Log.Write("In Get Commmands");
        Log.Write("data: " + data.Message);
        string CmdIn = data.Message;
        string CmdOut = null;
        switch (CmdIn)
        {
            case "SendPlayShuffle":
                CmdOut = "/shuffle";
                break;
            case "SendPlayList":
                CmdOut = "/playlist";
                break;
            case "SendStop":
                CmdOut = "/stop";
                break;
            case "SendPrevious":
                CmdOut = "/previous";
                break;
            case "SendPause":
                CmdOut = "/pause";
                break;
            case "SendResume":
                CmdOut = "/resume";
                break;
            case "SendNext":
                CmdOut = "/next";
                break;
            case "SendForceVideo":
                CmdOut = "/forcecurrentvideo";
                break;
            case "SendVolumeDown":
                CmdOut = "/volumeDown";
                break;
            case "SendVolumeUp":
                CmdOut = "/volumeUp";
                break;
            case "play1":
                CmdOut = "/play1";
                break;
            case "play2":
                CmdOut = "/play2";
                break;
            case "play3":
                CmdOut = "/play3";
                break;
            case "play4":
                CmdOut = "/play4";
                break;
            case "play5":
                CmdOut = "/play5";
                break;
            case "play6":
                CmdOut = "/play6";
                break;
            case "play7":
                CmdOut = "/play7";
                break;
            case "play8":
                CmdOut = "/play8";
                break;
            case "play9":
                CmdOut = "/play9";
                break;
            case "play10":
                CmdOut = "/play10";
                break;
            case "play11":
                CmdOut = "/play11";
                break;
            case "play12":
                CmdOut = "/play12";
                break;
            case "play13":
                CmdOut = "/play13";
                break;
            case "play14":
                CmdOut = "/play14";
                break;
            case "play15":
                CmdOut = "/play15";
                break;
            default:
                Errormsg = "Command Not Found";
                Log.Write(Errormsg);
                break;
        }

        ParseCommands(CmdOut);
    }

    private void UnhandledException(object Sender, Exception Ex)
    {

        Log.Write(LogLevel.Error, GetType().Name, Ex.Message + "\n" + Ex.StackTrace + "\n" + Ex.Source);
        return;
    }//UnhandledException

    #region Communication

    #region SimpleHelpers v2
    // Update the region tag above by incrementing the version when updating anything in the region.

    // If a Group is set, will only respond and send to other SimpleScripts with the same Group tag set.
    // Does NOT accept CSV lists of groups.
    // To send or receive events to/from a specific group from outside that group prepend the group name with a > to the event name
    // my_group>on
    [DefaultValue("")]
    [DisplayName("Group")]
    public string Group = "";

    public interface ISimpleData
    {
        AgentInfo AgentInfo { get; }
        ObjectId ObjectId { get; }
        ObjectId SourceObjectId { get; }

        // Extra data
        Reflective ExtraData { get; }
    }

    public class SimpleData : Reflective, ISimpleData
    {
        public SimpleData(ScriptBase script) { ExtraData = script; }
        public AgentInfo AgentInfo { get; set; }
        public ObjectId ObjectId { get; set; }
        public ObjectId SourceObjectId { get; set; }

        public Reflective ExtraData { get; }
    }

    public interface IDebugger { bool DebugSimple { get; } }
    private bool __debugInitialized = false;
    private bool __SimpleDebugging = false;
    private string __SimpleTag = "";

    private string GenerateEventName(string eventName)
    {
        eventName = eventName.Trim();
        if (eventName.EndsWith("@"))
        {
            // Special case on@ to send the event globally (the null group) by sending w/o the @.
            return eventName.Substring(0, eventName.Length - 1);
        }
        else if (Group == "" || eventName.Contains("@"))
        {
            // No group was set or already targeting a specific group as is.
            return eventName;
        }
        else
        {
            // Append the group
            return $"{eventName}@{Group}";
        }
    }

    private void SetupSimple()
    {
        __debugInitialized = true;
        __SimpleTag = GetType().Name + " [S:" + Script.ID.ToString() + " O:" + ObjectPrivate.ObjectId.ToString() + "]";
        Wait(TimeSpan.FromSeconds(1));
        IDebugger debugger = ScenePrivate.FindReflective<IDebugger>("SimpleDebugger").FirstOrDefault();
        if (debugger != null) __SimpleDebugging = debugger.DebugSimple;
    }

    System.Collections.Generic.Dictionary<string, Func<string, Action<ScriptEventData>, Action>> __subscribeActions = new System.Collections.Generic.Dictionary<string, Func<string, Action<ScriptEventData>, Action>>();
    private Action SubscribeToAll(string csv, Action<ScriptEventData> callback)
    {
        if (!__debugInitialized) SetupSimple();
        if (string.IsNullOrWhiteSpace(csv)) return null;

        Func<string, Action<ScriptEventData>, Action> subscribeAction;
        if (__subscribeActions.TryGetValue(csv, out subscribeAction))
        {
            return subscribeAction(csv, callback);
        }

        // Simple case.
        if (!csv.Contains(">>"))
        {
            __subscribeActions[csv] = SubscribeToAllInternal;
            return SubscribeToAllInternal(csv, callback);
        }

        // Chaining
        __subscribeActions[csv] = (_csv, _callback) =>
        {
            System.Collections.Generic.List<string> chainedCommands = new System.Collections.Generic.List<string>(csv.Split(new string[] { ">>" }, StringSplitOptions.RemoveEmptyEntries));

            string initial = chainedCommands[0];
            chainedCommands.RemoveAt(0);
            chainedCommands.Add(initial);

            Action unsub = null;
            Action<ScriptEventData> wrappedCallback = null;
            wrappedCallback = (data) =>
            {
                string first = chainedCommands[0];
                chainedCommands.RemoveAt(0);
                chainedCommands.Add(first);
                if (unsub != null) unsub();
                unsub = SubscribeToAllInternal(first, wrappedCallback);
                Log.Write(LogLevel.Info, "CHAIN Subscribing to " + first);
                _callback(data);
            };

            unsub = SubscribeToAllInternal(initial, wrappedCallback);
            return unsub;
        };

        return __subscribeActions[csv](csv, callback);
    }

    private Action SubscribeToAllInternal(string csv, Action<ScriptEventData> callback)
    {
        Action unsubscribes = null;
        string[] events = csv.Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        if (__SimpleDebugging)
        {
            Log.Write(LogLevel.Info, __SimpleTag, "Subscribing to " + events.Length + " events: " + string.Join(", ", events));
        }
        Action<ScriptEventData> wrappedCallback = callback;

        foreach (string eventName in events)
        {
            if (__SimpleDebugging)
            {
                var sub = SubscribeToScriptEvent(GenerateEventName(eventName), (ScriptEventData data) =>
                {
                    Log.Write(LogLevel.Info, __SimpleTag, "Received event " + GenerateEventName(eventName));
                    wrappedCallback(data);
                });
                unsubscribes += sub.Unsubscribe;
            }
            else
            {
                var sub = SubscribeToScriptEvent(GenerateEventName(eventName), wrappedCallback);
                unsubscribes += sub.Unsubscribe;
            }
        }
        return unsubscribes;
    }

    System.Collections.Generic.Dictionary<string, Action<string, Reflective>> __sendActions = new System.Collections.Generic.Dictionary<string, Action<string, Reflective>>();
    private void SendToAll(string csv, Reflective data)
    {
        if (!__debugInitialized) SetupSimple();
        if (string.IsNullOrWhiteSpace(csv)) return;

        Action<string, Reflective> sendAction;
        if (__sendActions.TryGetValue(csv, out sendAction))
        {
            sendAction(csv, data);
            return;
        }

        // Simple case.
        if (!csv.Contains(">>"))
        {
            __sendActions[csv] = SendToAllInternal;
            SendToAllInternal(csv, data);
            return;
        }

        // Chaining
        System.Collections.Generic.List<string> chainedCommands = new System.Collections.Generic.List<string>(csv.Split(new string[] { ">>" }, StringSplitOptions.RemoveEmptyEntries));
        __sendActions[csv] = (_csv, _data) =>
        {
            string first = chainedCommands[0];
            chainedCommands.RemoveAt(0);
            chainedCommands.Add(first);

            Log.Write(LogLevel.Info, "CHAIN Sending to " + first);
            SendToAllInternal(first, _data);
        };
        __sendActions[csv](csv, data);
    }

    private void SendToAllInternal(string csv, Reflective data)
    {
        if (string.IsNullOrWhiteSpace(csv)) return;
        string[] events = csv.Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

        if (__SimpleDebugging) Log.Write(LogLevel.Info, __SimpleTag, "Sending " + events.Length + " events: " + string.Join(", ", events) + (Group != "" ? (" to group " + Group) : ""));
        foreach (string eventName in events)
        {
            PostScriptEvent(GenerateEventName(eventName), data);
        }
    }
    #endregion

    #endregion

    private void GetChatCommand(ChatData Data)
    {
        Log.Write("Chat From: " + Data.SourceId);
        Log.Write("Chat person: " + ScenePrivate.FindAgent(Data.SourceId).AgentInfo.Name);
        AgentPrivate agent = ScenePrivate.FindAgent(Data.SourceId);
        ValidUsers.Clear();
        ValidUsers  = UsersToListenTo.Split(',').ToList();
        if (UsersToListenTo.Contains("ALL"))
        {
            string DataCmd = Data.Message;
            ParseCommands(DataCmd);
        }
        else
        {
            foreach (string ValidUser in ValidUsers)
            {
                Log.Write("ValidUser: " + ValidUser);
                if (ScenePrivate.FindAgent(Data.SourceId).AgentInfo.Name == ValidUser.Trim())
                {
                    string DataCmd = Data.Message;
                    ParseCommands(DataCmd);
                }
            }
    }
}

    private void ParseCommands(string DataCmdIn)
    {
        Errormsg = "No Errors";
        Log.Write("DataCmdIn: " + DataCmdIn);
        if (DataCmdIn.Contains("/"))
        {
            strErrors = false;
            if (DataCmdIn.Contains("/forcevideo"))
            {
                //play video
                IsWatchFormat = false;
                Log.Write("video: " + video);
                Log.Write("video length: " + video.Length);
                video = DataCmdIn.Substring(12, DataCmdIn.Length - 12);
                PlayVideo(video);
            }
            if (DataCmdIn.Contains("/forcecurrentvideo"))
            {
                //play video
                IsWatchFormat = false;
                //Log.Write("videoIn: " + videoIn);
                //Log.Write("videoIn length: " + videoIn.Length);
                //video = DataCmdIn.Substring(12, DataCmdIn.Length - 12);
                video = videoIn.Trim();
                //Log.Write("video length: " + video.Length);
                PlayVideo(video);
            }
            if (DataCmdIn.Contains("/next"))
            {
                string VideoToPlay = null;
                //Log.Write("PlayList Size: " + PlayList.Count());
                //Log.Write("playListPosition: " + playListPosition);
                if (PlayList.Count() == 0)
                {
                    Log.Write("No Playlist");
                }
                else 
                if (playListPosition > PlayList.Count() - 1)
                {
                    playListPosition = 1;
                }
                else
                {
                    playListPosition++;
                }
                VideoToPlay = PlayList[playListPosition-1];
                //Log.Write("video: " + VideoToPlay);
                videoIn = VideoToPlay;
                if (VideoToPlay.Contains("/watch?v="))
                {
                    IsWatchFormat = true;
                    VideoToPlay = URLToEmbedFormat(VideoToPlay);
                }
                PlayVideo(VideoToPlay);
            }
            if (DataCmdIn.Contains("/previous"))
            {
                string VideoToPlay = null;
                Log.Write("PlayList Size: " + PlayList.Count());
                Log.Write("playListPosition: " + playListPosition);
                if (PlayList.Count() == 0)
                {
                    Log.Write("No Playlist");
                }
                else if (playListPosition < 1)
                    {
                        playListPosition = PlayList.Count()-1;
                    }
                    else
                    {
                        playListPosition--;
                    }
                VideoToPlay = PlayList[playListPosition];
                Log.Write("video: " + VideoToPlay);
                videoIn = VideoToPlay;
                if (VideoToPlay.Contains("/watch?v="))
                {
                    IsWatchFormat = true;
                    VideoToPlay = URLToEmbedFormat(VideoToPlay);
                }
                PlayVideo(VideoToPlay);
            }
            if (DataCmdIn.Contains("/playlist"))
            {
                ScenePrivate.Chat.MessageAllUsers("Playlist Not Yet Implemented");
            }
            if (DataCmdIn.Contains("/shuffle"))
            {
                ScenePrivate.Chat.MessageAllUsers("Shuffle Not Yet Implemented");
            }

            if ((DataCmdIn.Contains("/video")) || (DataCmdIn.Contains("/stream")))
            {
                //play video
                IsWatchFormat = false;
                video = DataCmdIn.Substring(7, DataCmdIn.Length - 7);
                videoIn = video;
                Log.Write("video: " + video);
                if (DataCmdIn.Contains("/watch?v="))
                {
                    IsWatchFormat = true;
                    video = URLToEmbedFormat(DataCmdIn);
                    Log.Write("New Video: " + video);
                }
                if (DataCmdIn.Contains("Youtu.be"))
                {
                    IsWatchFormat = true;
                    video = ShortenedURLToEmbedFormat(DataCmdIn);
                    Log.Write("New Video: " + video);
                }
                PlayVideo(video);
            }
            if (DataCmdIn.Contains("/play"))
            {
                //play video
                string VideoToPlay = null;
                Log.Write("DataCmdIn: " + DataCmdIn.Trim());
                switch (DataCmdIn.Trim())
                {
                    case "/play1":
                        if (Play1.Length > 0)
                        {
                            IsWatchFormat = false;
                            VideoToPlay = Play1;
                            playListPosition = 1;
                        }
                        else Log.Write("No Video in Slot Play1");
                        break;
                    case "/play2":
                        if (Play2.Length > 0)
                        {
                            IsWatchFormat = false;
                            VideoToPlay = Play2;
                            playListPosition = 2;
                        }
                        else Log.Write("No Video in Slot Play2");
                        break;
                    case "/play3":
                        if (Play3.Length > 0)
                        {
                            IsWatchFormat = false;
                            VideoToPlay = Play3;
                            playListPosition = 3;
                        }
                        else Log.Write("No Video in Slot Play3");
                        break;
                    case "/play4":
                        if (Play4.Length > 0)
                        {
                            IsWatchFormat = false;
                            VideoToPlay = Play4;
                            playListPosition = 4;
                        }
                        else
                        {
                            Log.Write("No Video in Slot Play4");
                        }
                        break;
                    case "/play5":
                        if (Play5.Length > 0)
                        {
                            IsWatchFormat = false;
                            VideoToPlay = Play5;
                            playListPosition = 5;
                        }
                        else Log.Write("No Video in Slot Play5");
                        break;
                    case "/play6":
                        if (Play6.Length > 0)
                        {
                            IsWatchFormat = false;
                            VideoToPlay = Play6;
                            playListPosition = 6;
                        }
                        else Log.Write("No Video in Slot Play6");
                        break;
                    case "/play7":
                        if (Play7.Length > 0)
                        {
                            IsWatchFormat = false;
                            VideoToPlay = Play7;
                            playListPosition = 7;
                        }
                        else Log.Write("No Video in Slot Play7");
                        break;
                    case "/play8":
                        if (Play8.Length > 0)
                        {
                            IsWatchFormat = false;
                            VideoToPlay = Play8;
                            playListPosition = 8;
                        }
                        else Log.Write("No Video in Slot Play8");
                        break;
                    case "/play9":
                        if (Play9.Length > 0)
                        {
                            IsWatchFormat = false;
                            VideoToPlay = Play9;
                            playListPosition = 9;
                        }
                        else Log.Write("No Video in Slot Play9");
                        break;
                    case "/play10":
                        if (Play10.Length > 0)
                        {
                            IsWatchFormat = false;
                            VideoToPlay = Play10;
                            playListPosition = 10;
                        }
                        else Log.Write("No Video in Slot Play10");
                        break;
                    case "/play11":
                        if (Play11.Length > 0)
                        {
                            IsWatchFormat = false;
                            VideoToPlay = Play11;
                            playListPosition = 11;
                        }
                        else Log.Write("No Video in Slot Play11");
                        break;
                    case "/play12":
                        if (Play12.Length > 0)
                        {
                            IsWatchFormat = false;
                            VideoToPlay = Play12;
                            playListPosition = 12;
                        }
                        else Log.Write("No Video in Slot Play12");
                        break;
                    case "/play13":
                        if (Play13.Length > 0)
                        {
                            IsWatchFormat = false;
                            VideoToPlay = Play13;
                            playListPosition = 13;
                        }
                        else Log.Write("No Video in Slot Play13");
                        break;
                    case "/play14":
                        if (Play14.Length > 0)
                        {
                            IsWatchFormat = false;
                            VideoToPlay = Play14;
                            playListPosition = 14;
                        }
                        else Log.Write("No Video in Slot Play14");
                        break;
                    case "/play15":
                        if (Play15.Length > 0)
                        {
                            IsWatchFormat = false;
                            VideoToPlay = Play15;
                            playListPosition = 15;
                        }
                        else Log.Write("No Video in Slot Play15");
                        break;
                    default:
                        Errormsg = "Must be Play1 thru Play15";
                        break;
                }

                Log.Write("PlayListPosition: " + playListPosition);
                Log.Write("video: " + VideoToPlay);
                videoIn = VideoToPlay;
                if (VideoToPlay.Contains("/watch?v="))
                {
                    IsWatchFormat = true;
                    VideoToPlay = URLToEmbedFormat(VideoToPlay);
                }
                PlayVideo(VideoToPlay);
            }
            if (DataCmdIn.Contains("/pause") && IsWatchFormat)
            {
                intVideoCurrentTime = (int)(DateTime.Now - VideoStartTime).TotalSeconds;
                video = "https://www.youtube.com/embed/" + EmbedVideoID + "?rel=0&end=1&controls=0&showinfo=0&autoplay=1&allowfullscreen";
                Log.Write("Video on pause: " + video);
                ScenePrivate.OverrideMediaSource(video, ScreenWidth, ScreenHeight);
            }
            if (DataCmdIn.Contains("/resume") && IsWatchFormat)
            {
                intVideoCurrentTime = (int)(DateTime.Now - VideoStartTime).TotalSeconds;
                video = "https://www.youtube.com/embed/" + EmbedVideoID + "?rel=0&start=" + intVideoCurrentTime.ToString() + "&controls=0&showinfo=0&autoplay=1&allowfullscreen";
                Log.Write("Video on resume: " + video);
                ScenePrivate.OverrideMediaSource(video, ScreenWidth, ScreenHeight);
            }
            if (DataCmdIn.Contains("/stop") && IsWatchFormat)
            {
                intVideoCurrentTime = 0;
                VideoStartTime = DateTime.Now;
                EmbedVideoID = "4wTPTh6-sSo";
                video = "https://www.youtube.com/embed/" + EmbedVideoID + "  ?rel=0&controls=0&showinfo=0&autoplay=1&allowfullscreen";
                Log.Write("Video on pause: " + video);
                ScenePrivate.OverrideMediaSource(video, ScreenWidth, ScreenHeight);
                keepPlaying = false;
            }
            if (DataCmdIn.Contains("/volumeUp"))
            {
                Log.Write("In volumeUp");
                float curLoudness = currentPlayHandle.GetLoudness();
                curLoudness = curLoudness + 5;
                currentPlayHandle.SetLoudness(curLoudness);
            }
            if (DataCmdIn.Contains("/volumeDown"))
            {
                Log.Write("In volumeDown");
                float curLoudness = currentPlayHandle.GetLoudness();
                curLoudness = curLoudness - 5;
                currentPlayHandle.SetLoudness(curLoudness);
            }
            /*

            if (DataCmdIn.Contains("/commands"))
            {
                DisplayHelp(agent);

            }
            */
        }
    }

    private void PlayVideo(string Video)
    {
        bool complete = false;
        while (complete == false)
        {
            try
            {
                VideoStartTime = DateTime.Now;
                VideoCurrentTime = 0;
                ScenePrivate.OverrideMediaSource(Video, ScreenWidth, ScreenHeight);
                complete = true;
            }
            catch (ThrottleException e)
            {
                complete = false;
                Wait(e.Interval - e.Elapsed); // Wait out the rest of the interval before trying again.
            }
            Yield();
        }//while
    }

    private string URLToEmbedFormat(string URLInWatchFormat)
    {
        string URLInEmbedFormat = null;

        int VideoIDStart = URLInWatchFormat.IndexOf("?") + 3;
        Log.Write("VideoIDStart: " + VideoIDStart);
        Log.Write("URLInWatchFormat.Length: " + URLInWatchFormat.Length);
        EmbedVideoID = URLInWatchFormat.Substring(VideoIDStart, URLInWatchFormat.Length - VideoIDStart);
        Log.Write("EmbedVideoID: " + EmbedVideoID);
        URLInEmbedFormat = "https://www.youtube.com/embed/" + EmbedVideoID + "?rel=0&controls=0&showinfo=0&autoplay=1&allowfullscreen";

        return URLInEmbedFormat;
    }

    private string ShortenedURLToEmbedFormat(string URLInWatchFormat)
    {
        string URLInEmbedFormat = null;

        int VideoIDStart = URLInWatchFormat.IndexOf(".be/") + 5;
        Log.Write("VideoIDStart: " + VideoIDStart);
        Log.Write("URLInWatchFormat.Length: " + URLInWatchFormat.Length);
        EmbedVideoID = URLInWatchFormat.Substring(VideoIDStart, URLInWatchFormat.Length - VideoIDStart);
        Log.Write("EmbedVideoID: " + EmbedVideoID);
        URLInEmbedFormat = "https://www.youtube.com/embed/" + EmbedVideoID + "?rel=0&controls=0&showinfo=0&autoplay=1&allowfullscreen";

        return URLInEmbedFormat;
    }

    private void DisplayHelp(AgentPrivate agent)
    {
        agent.SendChat("Command Summary");
        agent.SendChat("/video YouTubeURL - starts Youtube Video");
        agent.SendChat("/pause - pauses video");
        agent.SendChat("/resume - resumes paused video");
        agent.SendChat("/stop - stops video playing");
        agent.SendChat("/play1 thru /play15 - plays preconfigured videos");
        agent.SendChat("/volume 30 - sets volume to 30. This is a volume slider and volume starts at a default of 50.  You can enter from 0 to 100 as a volume.");
        agent.SendChat("/commands - displays this help menu");
        agent.SendChat("To switch Videos just type in a new /video or /playX commmand");
        agent.SendChat("");
        agent.SendChat("contact GranddadGotMojo if any questions or issues");
    }

}