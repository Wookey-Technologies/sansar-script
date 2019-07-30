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

public class LogicSequenceCheckerMultiple : SceneObjectScript

{
    #region ConstantsVariables

    //private Action SimpleScriptSubscription;
    public string SequenceName = null;
    public string BaseEventName = null;
    public int NumberOfEvents = 0;
    public string Sequence1 = null;
    public string Sequence2 = null;
    public string Sequence3 = null;
    public string Sequence4 = null;
    public string Sequence5 = null;
    public string Sequence6 = null;
    public string Sequence7 = null;
    public string Sequence8 = null;
    public string Sequence9 = null;
    public string Sequence10 = null;

    private List<string> MatchedEvent = new List<string>();
    private List<string> FailedEvent = new List<string>();

    private string SequenceCode1 = null;
    private string SequenceCode2 = null;
    private string SequenceCode3 = null;
    private string SequenceCode4 = null;
    private string SequenceCode5 = null;
    private string SequenceCode6 = null;
    private string SequenceCode7 = null;
    private string SequenceCode8 = null;
    private string SequenceCode9 = null;
    private string SequenceCode10 = null;

    private string CodesIn = null;
    private int CodesReceived = 0;

    private int NumberOfSequences = 0;
    private int NumberOfCodes = 0;
    //private string Sequence = null;

    AgentInfo AgentName;
    ObjectId ComponentID;

    //private List<string> SequenceList = new List<string>();
    //private int ElementPositionInSequence = 0;
    //private bool SequenceLoaded = false;

    #endregion

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

    public override void Init()
    {
        //Documentation Here

        Script.UnhandledException += UnhandledException; // Catch errors and keep running unless fatal
        NumberOfSequences = 0;
        //Log.Write("Init");
        //Read in Sequences
        if (Sequence1.Length > 0)
        {
            ParseLogicSequence(0, Sequence1);
            NumberOfSequences = 1;
        }
        if (Sequence2.Length > 0)
        {
            ParseLogicSequence(1, Sequence2);
            NumberOfSequences = 2;
        }
        if (Sequence3.Length > 0)
        {
            ParseLogicSequence(2, Sequence3);
            NumberOfSequences = 3;
        }
        if (Sequence4.Length > 0)
        {
            ParseLogicSequence(3, Sequence4);
            NumberOfSequences = 4;
        }
        if (Sequence5.Length > 0)
        {
            ParseLogicSequence(4, Sequence5);
            NumberOfSequences = 5;
        }
        if (Sequence6.Length > 0)
        {
            ParseLogicSequence(5, Sequence6);
            NumberOfSequences = 6;
        }
        if (Sequence7.Length > 0)
        {
            ParseLogicSequence(6, Sequence7);
            NumberOfSequences = 7;
        }
        if (Sequence8.Length > 0)
        {
            ParseLogicSequence(7, Sequence8);
            NumberOfSequences = 8;
        }
        if (Sequence9.Length > 8)
        {
            ParseLogicSequence(8, Sequence9);
            NumberOfSequences = 9;
        }
        if (Sequence10.Length > 0)
        {
            ParseLogicSequence(9, Sequence10);
            NumberOfSequences = 10;
        }

        //Subscribe to User Control events
        subscribeToMulitpleEvents(BaseEventName, NumberOfEvents);
        Wait(TimeSpan.FromSeconds(2));  //wait a couple of seconds for other scripts to get subscribes in place
        sendSimpleMessage(SequenceName);  //Send a messag to anyone instrested in logic sequence checker has been started for this sequence (i.e. LogicSequencePlayer would normally be listening for this)

    }

    private void DecodeUserInteraction(ScriptEventData data)
    {
        //Load CodeIn into CodeInArray
        if (CodesReceived > 0)
        {
            CodesIn = CodesIn + "," + data.Message;
        }
        else
        {
            CodesIn = data.Message;
        }
        CodesReceived++;
        if (CodesReceived == NumberOfCodes)
        {
            CheckSequences();
            CodesReceived = 0;
        }
    }

    private void ParseLogicSequence(int SequenceNumber, string SequenceIn)
    {
        //Log.Write("In ParseLogicSequence SequenceNumber: " + SequenceNumber + " SequenceIn: " + SequenceIn);
        List<string> SequenceArray = new List<string>();
        string SequenceCodesWorking = null;
        SequenceArray.Clear();
        SequenceArray = SequenceIn.Split(',').ToList();
        //Log.Write("SequenceArrayCount: " + SequenceArray.Count());
        MatchedEvent.Add(SequenceArray[0]);
        FailedEvent.Add(SequenceArray[1]);
        if (SequenceArray.Count() > 2)
        {
            SequenceCodesWorking = SequenceArray[2];
        }
        if (SequenceArray.Count() > 3)
        {
            SequenceCodesWorking = SequenceCodesWorking + "," + SequenceArray[3];
        }
        if (SequenceArray.Count() > 4)
        {
            SequenceCodesWorking = SequenceCodesWorking + "," + SequenceArray[4];
        }
        if (SequenceArray.Count() > 5)
        {
            SequenceCodesWorking = SequenceCodesWorking + "," + SequenceArray[5];
        }
        if (SequenceArray.Count() > 6)
        {
            SequenceCodesWorking = SequenceCodesWorking + "," + SequenceArray[6];
        }
        if (SequenceArray.Count() > 7)
        {
            SequenceCodesWorking = SequenceCodesWorking + "," + SequenceArray[7];
        }
        if (SequenceArray.Count() > 8)
        {
            SequenceCodesWorking = SequenceCodesWorking + "," + SequenceArray[8];
        }
        if (SequenceArray.Count() > 9)
        {
            SequenceCodesWorking = SequenceCodesWorking + "," + SequenceArray[9];
        }
        if (SequenceArray.Count() > 10)
        {
            SequenceCodesWorking = SequenceCodesWorking + "," + SequenceArray[10];
        }
        if (SequenceArray.Count() > 11)
        {
            SequenceCodesWorking = SequenceCodesWorking + "," + SequenceArray[11];
        }

        if (SequenceNumber == 0) SequenceCode1 = SequenceCodesWorking;
        if (SequenceNumber == 1) SequenceCode2 = SequenceCodesWorking;
        if (SequenceNumber == 2) SequenceCode3 = SequenceCodesWorking;
        if (SequenceNumber == 3) SequenceCode4 = SequenceCodesWorking;
        if (SequenceNumber == 4) SequenceCode5 = SequenceCodesWorking;
        if (SequenceNumber == 5) SequenceCode6 = SequenceCodesWorking;
        if (SequenceNumber == 6) SequenceCode7 = SequenceCodesWorking;
        if (SequenceNumber == 7) SequenceCode8 = SequenceCodesWorking;
        if (SequenceNumber == 8) SequenceCode9 = SequenceCodesWorking;
        if (SequenceNumber == 9) SequenceCode10 = SequenceCodesWorking;
        NumberOfCodes = SequenceArray.Count() - 2;
        Log.Write("Finished Parse Sequence");
    }

    private void CheckSequences()
    {
        //Log.Write("In Check Sequences");
        //Log.Write("CodesIn: " + CodesIn + " Length: " + CodesIn.Length);
        //Log.Write("FailedEvents: " + FailedEvent.Count());
        //Log.Write("MatchedEvents: " + MatchedEvent.Count());
        //Log.Write("SequenceCode1: " + SequenceCode1 + " Length: " + SequenceCode1.Length);
        if (SequenceCode1 != null)
        {
            if (SequenceCode1 == CodesIn)
            {
                sendSimpleMessage(MatchedEvent[0]);
            }
            else if (SequenceCode1.Length > 0)
            {
                sendSimpleMessage(FailedEvent[0]);
            }
        }

        if (SequenceCode2 != null)
        {
            if (SequenceCode2 == CodesIn)
            {
                sendSimpleMessage(MatchedEvent[1]);
            }
            else if (SequenceCode2.Length > 0)
            {
                sendSimpleMessage(FailedEvent[1]);
            }
        }

        if (SequenceCode3 != null)
        {
            if (SequenceCode3 == CodesIn)
            {
                sendSimpleMessage(MatchedEvent[2]);
            }
            else if (SequenceCode3.Length > 0)
            {
                sendSimpleMessage(FailedEvent[2]);
            }
        }

        if (SequenceCode4 != null)
        {
            if (SequenceCode4 == CodesIn)
            {
                sendSimpleMessage(MatchedEvent[3]);
            }
            else if (SequenceCode4.Length > 0)
            {
                sendSimpleMessage(FailedEvent[3]);
            }
        }

        if (SequenceCode5 != null)
        {
            if ((SequenceCode5 != null) && (SequenceCode5 == CodesIn))
            {
                sendSimpleMessage(MatchedEvent[4]);
            }
            else if (SequenceCode5.Length > 0)
            {
                sendSimpleMessage(FailedEvent[4]);
            }
        }

        if (SequenceCode6 != null)
        {
            if (SequenceCode6 == CodesIn)
            {
                sendSimpleMessage(MatchedEvent[5]);
            }
            else if (SequenceCode6.Length > 0)
            {
                sendSimpleMessage(FailedEvent[5]);
            }
        }

        if (SequenceCode7 != null)
        {
            if (SequenceCode7 == CodesIn)
            {
                sendSimpleMessage(MatchedEvent[6]);
            }
            else if (SequenceCode7.Length > 0)
            {
                sendSimpleMessage(FailedEvent[6]);
            }
        }

        if (SequenceCode8 != null)
        {
            if (SequenceCode8 == CodesIn)
            {
                sendSimpleMessage(MatchedEvent[7]);
            }
            else if (SequenceCode8.Length > 0)
            {
                sendSimpleMessage(FailedEvent[7]);
            }
        }

        if (SequenceCode9 != null)
        {
            if (SequenceCode9 == CodesIn)
            {
                sendSimpleMessage(MatchedEvent[8]);
            }
            else if (SequenceCode9.Length > 0)
            {
                sendSimpleMessage(FailedEvent[8]);
            }
        }

        if (SequenceCode10 != null)
        {
            if (SequenceCode10 == CodesIn)
            {
                sendSimpleMessage(MatchedEvent[9]);
            }
            else if (SequenceCode10.Length > 0)
            {
                sendSimpleMessage(FailedEvent[9]);
            }
        }

    }

    private void sendSimpleMessage(string msg)
    {
        Log.Write("Sending Msg: " + msg);
        SimpleData sd = new SimpleData(this);
        sd.AgentInfo = AgentName;
        sd.ObjectId = ComponentID;
        sd.SourceObjectId = ObjectPrivate.ObjectId;
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
