//* "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."

using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

using Sansar;
using Sansar.Script;
using Sansar.Simulation;

public class ActionSynthModule : SceneObjectScript

{
    #region ConstantsVariables
    [DefaultValue("K1")]
    [DisplayName("Input Channel: ")]
    public string InputChannel;

    public List<SoundResource> S0SoundResources = new List<SoundResource>();
    public List<float> S0Offset = new List<float>();
    public List<SoundResource> S1SoundResources = new List<SoundResource>();
    public List<float> S1Offset = new List<float>();
    public List<SoundResource> S2SoundResources = new List<SoundResource>();
    public List<float> S2Offset = new List<float>();
    public List<SoundResource> S3SoundResources = new List<SoundResource>();
    public List<float> S3Offset = new List<float>();
    public List<SoundResource> S4SoundResources = new List<SoundResource>();
    public List<float> S4Offset = new List<float>();
    public List<SoundResource> S5SoundResources = new List<SoundResource>();
    public List<float> S5Offset = new List<float>();
    public List<SoundResource> S6SoundResources = new List<SoundResource>();
    public List<float> S6Offset = new List<float>();
    public List<SoundResource> S7SoundResources = new List<SoundResource>();
    public List<float> S7Offset = new List<float>();

    public float loudnessIn = 0;
    public string EnableEvent;
    public string DisableEvent;

    private bool enabled = false;
    private string KeyIn = null;
    private PlaySettings playSettings;
    private PlayHandle playHandleSimple;

    #endregion

    #region Communication

    #region SimpleHelpers
    public interface ISimpleData
    {
        AgentInfo AgentInfo { get; }
        ObjectId ObjectId { get; }
    }

    public class SimpleData : Reflective, ISimpleData
    {
        public AgentInfo AgentInfo { get; set; }
        public ObjectId ObjectId { get; set; }
    }

    public interface IDebugger { bool DebugSimple { get; } }
    private bool __debugInitialized = false;
    private bool __SimpleDebugging = false;
    private string __SimpleTag = "";
    private void SetupSimple()
    {
        __debugInitialized = true;
        __SimpleTag = GetType().Name + " [S:" + Script.ID.ToString() + " O:" + ObjectPrivate.ObjectId.ToString() + "]";
        Wait(TimeSpan.FromSeconds(1));
        IDebugger debugger = ScenePrivate.FindReflective<IDebugger>("SimpleDebugger").FirstOrDefault();
        if (debugger != null) __SimpleDebugging = debugger.DebugSimple;
    }

    private Action SubscribeToAll(string csv, Action<ScriptEventData> callback)
    {
        if (!__debugInitialized) SetupSimple();
        if (string.IsNullOrWhiteSpace(csv)) return null;
        Action unsubscribes = null;
        string[] events = csv.Trim().Split(',');
        if (__SimpleDebugging)
        {
            Log.Write(LogLevel.Info, __SimpleTag, "Subscribing to " + events.Length + " events: " + string.Join(", ", events));
        }
        foreach (string eventName in events)
        {
            if (__SimpleDebugging)
            {
                var sub = SubscribeToScriptEvent(eventName.Trim(), (ScriptEventData data) =>
                {
                    Log.Write(LogLevel.Info, __SimpleTag, "Received event " + eventName);
                    callback(data);
                });
                unsubscribes += sub.Unsubscribe;
            }
            else
            {
                var sub = SubscribeToScriptEvent(eventName.Trim(), callback);
                unsubscribes += sub.Unsubscribe;
            }

        }
        return unsubscribes;
    }

    private void SendToAll(string csv, Reflective data)
    {
        if (!__debugInitialized) SetupSimple();
        if (string.IsNullOrWhiteSpace(csv)) return;
        string[] events = csv.Trim().Split(',');

        if (__SimpleDebugging) Log.Write(LogLevel.Info, __SimpleTag, "Sending " + events.Length + " events: " + string.Join(", ", events));
        foreach (string eventName in events)
        {
            PostScriptEvent(eventName.Trim(), data);
        }
    }
    #endregion

    public class SendKeyInfo : Reflective
    {
        public string iChannelOut { get; set; }
        public string iKeySent { get; set; }
    }

    public interface ISendKeyInfo
    {
        string iChannelOut { get; }
        string iKeySent { get; }
    }

    private void getKeyInfo(ScriptEventData gotKeyInfo)
    {
        if (enabled)
        {
            //Log.Write("ActionSynthModule: In getKeyInfo");
            if (gotKeyInfo.Data == null)
            {
                return;
            }

            ISendKeyInfo sendKeyInfo = gotKeyInfo.Data.AsInterface<ISendKeyInfo>();
            if (sendKeyInfo == null)
            {
                Log.Write(LogLevel.Error, Script.ID.ToString(), "Unable to create interface, check logs for missing member(s)");
                return;
            }
            if (sendKeyInfo.iChannelOut == InputChannel)
            {
                KeyIn = sendKeyInfo.iKeySent;
                //Log.Write("KeyIn before Trim: " + KeyIn);
                if (KeyIn.Substring(0, 1) == "X")
                {
                    //Log.Write("X Key");
                    KeyIn = KeyIn.Substring(1);
                }
                //Log.Write("KeyIn after Trim: " + KeyIn);

                switch (KeyIn)
                {
                    //case "C0": PlayNote(S0SoundResources[0], S0Offset[0]); break;
                    //case "CS0": PlayNote(S0SoundResources[1], S0Offset[1]); break;
                    //case "D0": PlayNote(S0SoundResources[2], S0Offset[2]); break;
                    //case "DS0": PlayNote(S0SoundResources[3], S0Offset[3]); break;
                    //case "E0": PlayNote(S0SoundResources[4], S0Offset[4]); break;
                    //case "F0": PlayNote(S0SoundResources[5], S0Offset[5]); break;
                    //case "FS0": PlayNote(S0SoundResources[6], S0Offset[6]); break;
                    //case "G0": PlayNote(S0SoundResources[7], S0Offset[7]); break;
                    //case "GS0": PlayNote(S0SoundResources[8], S0Offset[8]); break;
                    //case "A0": PlayNote(S0SoundResources[9], S0Offset[9]); break;
                    //case "AS0": PlayNote(S0SoundResources[10], S0Offset[10]); break;
                    //case "B0": PlayNote(S0SoundResources[11], S0Offset[11]); break;
                    //case "C1": PlayNote(S1SoundResources[0], S1Offset[0]); break;
                    //case "CS1": PlayNote(S1SoundResources[1], S1Offset[1]); break;
                    //case "D1": PlayNote(S1SoundResources[2], S1Offset[2]); break;
                    //case "DS1": PlayNote(S1SoundResources[3], S1Offset[3]); break;
                    //case "E1": PlayNote(S1SoundResources[4], S1Offset[4]); break;
                    //case "F1": PlayNote(S1SoundResources[5], S1Offset[5]); break;
                    //case "FS1": PlayNote(S1SoundResources[6], S1Offset[6]); break;
                    //case "G1": PlayNote(S1SoundResources[7], S1Offset[7]); break;
                    //case "GS1": PlayNote(S1SoundResources[8], S1Offset[8]); break;
                    //case "A1": PlayNote(S1SoundResources[9], S1Offset[9]); break;
                    //case "AS1": PlayNote(S1SoundResources[10], S1Offset[10]); break;
                    //case "B1": PlayNote(S1SoundResources[11], S1Offset[11]); break;
                    case "C2": PlayNote(S2SoundResources[0], S2Offset[0]); break;
                    case "CS2": PlayNote(S2SoundResources[1], S2Offset[1]); break;
                    case "D2": PlayNote(S2SoundResources[2], S2Offset[2]); break;
                    case "DS2": PlayNote(S2SoundResources[3], S2Offset[3]); break;
                    case "E2": PlayNote(S2SoundResources[4], S2Offset[4]); break;
                    case "F2": PlayNote(S2SoundResources[5], S2Offset[5]); break;
                    case "FS2": PlayNote(S2SoundResources[6], S2Offset[6]); break;
                    case "G2": PlayNote(S2SoundResources[7], S2Offset[7]); break;
                    case "GS2": PlayNote(S2SoundResources[8], S2Offset[8]); break;
                    case "A2": PlayNote(S2SoundResources[9], S2Offset[9]); break;
                    case "AS2": PlayNote(S2SoundResources[10], S2Offset[10]); break;
                    case "B2": PlayNote(S2SoundResources[11], S2Offset[11]); break;
                    case "C3": PlayNote(S3SoundResources[0], S3Offset[0]); break;
                    case "CS3": PlayNote(S3SoundResources[1], S3Offset[1]); break;
                    case "D3": PlayNote(S3SoundResources[2], S3Offset[2]); break;
                    case "DS3": PlayNote(S3SoundResources[3], S3Offset[3]); break;
                    case "E3": PlayNote(S3SoundResources[4], S3Offset[4]); break;
                    case "F3": PlayNote(S3SoundResources[5], S3Offset[5]); break;
                    case "FS3": PlayNote(S3SoundResources[6], S3Offset[6]); break;
                    case "G3": PlayNote(S3SoundResources[7], S3Offset[7]); break;
                    case "GS3": PlayNote(S3SoundResources[8], S3Offset[8]); break;
                    case "A3": PlayNote(S3SoundResources[9], S3Offset[9]); break;
                    case "AS3": PlayNote(S3SoundResources[10], S3Offset[10]); break;
                    case "B3": PlayNote(S3SoundResources[11], S3Offset[11]); break;
                    case "C4": PlayNote(S4SoundResources[0], S4Offset[0]); break;
                    case "CS4": PlayNote(S4SoundResources[1], S4Offset[1]); break;
                    case "D4": PlayNote(S4SoundResources[2], S4Offset[2]); break;
                    case "DS4": PlayNote(S4SoundResources[3], S4Offset[3]); break;
                    case "E4": PlayNote(S4SoundResources[4], S4Offset[4]); break;
                    case "F4": PlayNote(S4SoundResources[5], S4Offset[5]); break;
                    case "FS4": PlayNote(S4SoundResources[6], S4Offset[6]); break;
                    case "G4": PlayNote(S4SoundResources[7], S4Offset[7]); break;
                    case "GS4": PlayNote(S4SoundResources[8], S4Offset[8]); break;
                    case "A4": PlayNote(S4SoundResources[9], S4Offset[9]); break;
                    case "AS4": PlayNote(S4SoundResources[10], S4Offset[10]); break;
                    case "B4": PlayNote(S4SoundResources[11], S4Offset[11]); break;
                    case "C5": PlayNote(S5SoundResources[0], S5Offset[0]); break;
                    case "CS5": PlayNote(S5SoundResources[1], S5Offset[1]); break;
                    case "D5": PlayNote(S5SoundResources[2], S5Offset[2]); break;
                    case "DS5": PlayNote(S5SoundResources[3], S5Offset[3]); break;
                    case "E5": PlayNote(S5SoundResources[4], S5Offset[4]); break;
                    case "F5": PlayNote(S5SoundResources[5], S5Offset[5]); break;
                    case "FS5": PlayNote(S5SoundResources[6], S5Offset[6]); break;
                    case "G5": PlayNote(S5SoundResources[7], S5Offset[7]); break;
                    case "GS5": PlayNote(S5SoundResources[8], S5Offset[8]); break;
                    case "A5": PlayNote(S5SoundResources[9], S5Offset[9]); break;
                    case "AS5": PlayNote(S5SoundResources[10], S5Offset[10]); break;
                    case "B5": PlayNote(S5SoundResources[11], S5Offset[11]); break;
                    case "C6": PlayNote(S6SoundResources[0], S6Offset[0]); break;
                    case "CS6": PlayNote(S6SoundResources[1], S6Offset[1]); break;
                    case "D6": PlayNote(S6SoundResources[2], S6Offset[2]); break;
                    case "DS6": PlayNote(S6SoundResources[3], S6Offset[3]); break;
                    case "E6": PlayNote(S6SoundResources[4], S6Offset[4]); break;
                    case "F6": PlayNote(S6SoundResources[5], S6Offset[5]); break;
                    case "FS6": PlayNote(S6SoundResources[6], S6Offset[6]); break;
                    case "G6": PlayNote(S6SoundResources[7], S6Offset[7]); break;
                    case "GS6": PlayNote(S6SoundResources[8], S6Offset[8]); break;
                    case "A6": PlayNote(S6SoundResources[9], S6Offset[9]); break;
                    case "AS6": PlayNote(S6SoundResources[10], S6Offset[10]); break;
                    case "B6": PlayNote(S6SoundResources[11], S6Offset[11]); break;
                    //case "C7": PlayNote(S7SoundResources[0], S7Offset[0]); break;
                    //case "CS7": PlayNote(S7SoundResources[1], S7Offset[1]); break;
                    //case "D7": PlayNote(S7SoundResources[2], S7Offset[2]); break;
                    //case "DS7": PlayNote(S7SoundResources[3], S7Offset[3]); break;
                    //case "E7": PlayNote(S7SoundResources[4], S7Offset[4]); break;
                    //case "F7": PlayNote(S7SoundResources[5], S7Offset[5]); break;
                    //case "FS7": PlayNote(S7SoundResources[6], S7Offset[6]); break;
                    //case "G7": PlayNote(S7SoundResources[7], S7Offset[7]); break;
                    //case "GS7": PlayNote(S7SoundResources[8], S7Offset[8]); break;
                    //case "A7": PlayNote(S7SoundResources[9], S7Offset[9]); break;
                    //case "AS7": PlayNote(S7SoundResources[10], S7Offset[10]); break;
                    //case "B7": PlayNote(S7SoundResources[11], S7Offset[11]); break;
                }
            }
        }
    }

    #endregion

    public override void Init()
    {
        //Build Instrument using samples and offsets
        //Listen for Key Event
        SubscribeToScriptEvent("KeySent", getKeyInfo);
        SubscribeToAll(EnableEvent, EnableModule);
        SubscribeToAll(DisableEvent, DisableModule);
    }

    private void EnableModule(ScriptEventData sed)
    {
        enabled = true;
    }

    private void DisableModule(ScriptEventData sed)
    {
        enabled = false;
    }

    private void UnhandledException(object Sender, Exception Ex)
    {
        Log.Write(LogLevel.Error, GetType().Name, Ex.Message + "\n" + Ex.StackTrace + "\n" + Ex.Source);
        return;
    }//UnhandledException

    #region PlayNotes

    private void PlayNote(SoundResource PlaySample, float PitchShiftIn)
    {
        bool NoLoop = true;
        playSettings = NoLoop ? PlaySettings.PlayOnce : PlaySettings.Looped;

        playSettings.Loudness = loudnessIn; // set in Configuration
        playSettings.DontSync = true; // TrackDont_Sync[LoopIn2];
        playSettings.PitchShift = PitchShiftIn;
        //playHandle[LoopIn2] = ScenePrivate.PlaySound(TrackSamples[LoopIn2][PlayIndexIn], playSettings);
        playHandleSimple = ScenePrivate.PlaySound(PlaySample, playSettings);
    }

    #endregion

}
