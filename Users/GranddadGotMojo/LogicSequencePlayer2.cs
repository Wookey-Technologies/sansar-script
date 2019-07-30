//* "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."

#define SansarBuild
#define InstrumentBuild

using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

using Sansar;
using Sansar.Script;
using Sansar.Simulation;

public class LogicSequencePlayer2 : SceneObjectScript

{
    #region ConstantsVariables

    private Action SimpleScriptSubscription;
    public string TriggerEnter = null;
    public string Sequence = null;
    public string Timing = null;
    public float Tempo = 1.0f;

    AgentInfo AgentName;
    ObjectId ComponentID;

    AgentInfo LastJammer;

    private List<string> SequenceList = new List<string>();
    private List<string> strTimingList = new List<string>();
    private List<float> TimingList = new List<float>();

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

    #endregion

    public override void Init()
    {
        //Documentation Here

        //Log.Write("Init");
        Script.UnhandledException += UnhandledException; // Catch errors and keep running unless fatal

        //Subscribe to User Control events
        SubscribeToScriptEvent(TriggerEnter, PlaySequence);
        //SubscribeToScriptEvent(SequenceName+"Replay", PlaySequence);
    }

    private void PlaySequence(ScriptEventData data)
    {
        Log.Write("In PlaySequence");
        
        SequenceList.Clear();
        SequenceList = Sequence.Split(',').ToList();
        TimingList.Clear();
        strTimingList.Clear();
        strTimingList = Timing.Split(',').ToList();
        foreach (string TimeElement in strTimingList)
        {
            TimingList.Add(float.Parse(TimeElement));
        }
        int cntr = 0;
        foreach (string SequenceElement in SequenceList)
        {
            Log.Write("playing: " + SequenceElement);
            sendSimpleMessage(SequenceElement);
            Wait(TimeSpan.FromMilliseconds(TimingList[cntr] * 1000 / Tempo));
            sendSimpleMessage(SequenceElement + "Off");
            cntr++;
        }
    }

    private void sendSimpleMessage(string msg)
    {
        SimpleData sd = new SimpleData();
        sd.AgentInfo = AgentName;
        sd.ObjectId = ComponentID;
        SendToAll(msg, sd);
    }

    private void UnhandledException(object Sender, Exception Ex)
    {
        Log.Write(LogLevel.Error, GetType().Name, Ex.Message + "\n" + Ex.StackTrace + "\n" + Ex.Source);
        return;
    }//UnhandledException

}
