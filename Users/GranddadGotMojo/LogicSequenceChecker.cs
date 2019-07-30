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

public class LogicSequenceChecker : SceneObjectScript

{
    #region ConstantsVariables

    private Action SimpleScriptSubscription;
    public string SequenceName = null;
    public string MatchedEvent = "Pass";
    public string FailedEvent = "Failed";
    public string ResetCodeEvent = "ResetCode";
    public string BaseEventName = null;
    public int NumberOfEvents = 0;
    public string Sequence = null;

    AgentInfo AgentName;
    ObjectId ComponentID;

    private List<string> SequenceList = new List<string>();
    private int ElementPositionInSequence = 0;
    private bool SequenceLoaded = false;

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

        Script.UnhandledException += UnhandledException; // Catch errors and keep running unless fatal

        Log.Write("Init");
        ElementPositionInSequence = 0;

        //Subscribe to User Control events
        subscribeToMulitpleEvents(BaseEventName, NumberOfEvents);
        Wait(TimeSpan.FromSeconds(2));  //wait a couple of seconds for other scripts to get subscribes in place
        sendSimpleMessage(SequenceName);  //Send a messag to anyone instrested in logic sequence checker has been started for this sequence (i.e. LogicSequencePlayer would normally be listening for this)
    }

    private void DecodeUserInteraction(ScriptEventData data)
    {
        if (!SequenceLoaded)  //first interaction load Sequence to Check
        {
            ISimpleData sd = data.Data.AsInterface<ISimpleData>();
            AgentName = sd.AgentInfo;
            ComponentID = sd.ObjectId;
            LoadSequenceToCheck();
            SequenceLoaded = true;
        }
        if (data.Message == ResetCodeEvent)
        {
            ElementPositionInSequence = 0;
        }
        CheckSequence(data.Message);
    }

    private void LoadSequenceToCheck()
    {
        //Log.Write("In LoadSequenceToCheck");
        SequenceList.Clear();
        SequenceList = Sequence.Split(',').ToList();
    }

    private void CheckSequence(string userInteractionControlEvent)
    {
        if (ElementPositionInSequence < SequenceList.Count())
        {
            //Log.Write("ElementPositionInSequence: " + ElementPositionInSequence);
            //Log.Write("userInteractionControlEvent: " + userInteractionControlEvent);
            //Log.Write("SequenceList[ElementPositionInSequence]: " + SequenceList[ElementPositionInSequence]);
            //Log.Write("SequenceList.Count: " + SequenceList.Count());
            if (userInteractionControlEvent == SequenceList[ElementPositionInSequence])
            {
                //Log.Write("Sequence " + ElementPositionInSequence + " matched");
                sendSimpleMessage(SequenceName + "Match" + (ElementPositionInSequence + 1));
                ElementPositionInSequence++;
                if ((ElementPositionInSequence == SequenceList.Count()))  //Last element in sequence was matched so the whole sequence has been matched
                {
                    //Log.Write("SendingAttaBoy");
                    sendSimpleMessage(MatchedEvent);
                    ElementPositionInSequence = 0;  //reset so you can run logic sequence check again
                    Wait(TimeSpan.FromSeconds(4));
                    sendSimpleMessage(SequenceName + "Failed");
                }
            }
            else
            {
                //Log.Write("Sequence " + ElementPositionInSequence + " Failed");
                sendSimpleMessage(SequenceName + "Fail" + (ElementPositionInSequence + 1));
                Wait(TimeSpan.FromSeconds(2));
                sendSimpleMessage(FailedEvent);
                sendSimpleMessage(SequenceName + "Replay");
                ElementPositionInSequence = 0;
                sendSimpleMessage(SequenceName);  //Send a messag to anyone instrested in logic sequence checker has been started for this sequence (i.e. LogicSequencePlayer would normally be listening for this)
            }
        }
        else
        {
        }
    }

    private void sendSimpleMessage(string msg)
    {
        SimpleData sd = new SimpleData();
        sd.AgentInfo = AgentName;
        sd.ObjectId = ComponentID;
        SendToAll(msg, sd);
    }

    private void subscribeToMulitpleEvents(string EventBaseName, int NumberOfEvents)
    {
        if (NumberOfEvents > 0)
        {
            int cntr = 1;
            do
            {
                SubscribeToScriptEvent(EventBaseName + cntr, DecodeUserInteraction);
                cntr++;
            }
            while (cntr < NumberOfEvents);
        }
        else
        {
            SubscribeToScriptEvent(EventBaseName, DecodeUserInteraction);
        }
        
    }

    private void UnhandledException(object Sender, Exception Ex)
    {
        Log.Write(LogLevel.Error, GetType().Name, Ex.Message + "\n" + Ex.StackTrace + "\n" + Ex.Source);
        return;
    }//UnhandledException

}
