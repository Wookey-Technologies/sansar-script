/* This content is licensed under the terms of the Creative Commons Attribution 4.0 International License.
 * When using this content, you must:
 * •    Acknowledge that the content is from the Sansar Knowledge Base.
 * •    Include our copyright notice: "© 2018 Linden Research, Inc."
 * •    Indicate that the content is licensed under the Creative Commons Attribution-Share Alike 4.0 International License.
 * •    Include the URL for, or link to, the license summary at https://creativecommons.org/licenses/by-sa/4.0/deed.hi (and, if possible, to the complete license terms at https://creativecommons.org/licenses/by-sa/4.0/legalcode.
 * For example:
 * "This work uses content from the Sansar Knowledge Base. © 2018 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."
 */
using Sansar;
using Sansar.Script;
using Sansar.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;

public class ActionComplexMover : SceneObjectScript
{

    //Script uses ObjectPrivate.Mover .... Moves from current position to a new relative position.  Triggered via simple messages.

#region ConstantsVariables
    // Public properties

    public string MoveTo1 = null;
    public string MoveTo2 = null;
    public string MoveTo3 = null;
    public string MoveTo4 = null;
    public string MoveTo5 = null;
    public string MoveTo6 = null;
    public string MoveTo7 = null;
    public string MoveTo8 = null;
    public string MoveTo9 = null;
    public string MoveTo10 = null;
    public string MoveTo11 = null;
    public string MoveTo12 = null;
    public string MoveTo13 = null;
    public string MoveTo14 = null;
    public string MoveTo15 = null;
    public string EnableEvent = null;
    public string DisableEvent = null;

    private Vector rotatedPositionOffset;
    private Vector initialPosition;

    private string[] MoveEvent = new string[15];
    private string[] DoneEvent = new string[15];
    private Vector[] MoveVector = new Vector[15];
    private Vector[] RotateVector = new Vector[15];
    private double[] OverTime = new double[15];
    private MoveMode[] MoveModeArray = new MoveMode[15];
    private string[] LoopType = new string[15];

    private Vector PositionOffset;

    #endregion

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

    public override void Init()
    {
        initialPosition = ObjectPrivate.Position;
        Log.Write("initialPosition: " + initialPosition);
        ReadInParameters();
        // Subscribe to the interaction, meaning this next block of code will be executed when the object is clicked on
    }

    private void ReadInParameters()  //doesn't really pass data.  Always passes null
    {
        //Look At Animation Strings and subscribe to events
        Log.Write("In ReadInParameters");
        if (MoveTo1.Length > 0) ParseMove(0, MoveTo1);
        if (MoveTo2.Length > 0) ParseMove(1, MoveTo2);
        if (MoveTo3.Length > 0) ParseMove(2, MoveTo3);
        if (MoveTo4.Length > 0) ParseMove(3, MoveTo4);
        if (MoveTo5.Length > 0) ParseMove(4, MoveTo5);
        if (MoveTo6.Length > 0) ParseMove(5, MoveTo6);
        if (MoveTo7.Length > 0) ParseMove(6, MoveTo7);
        if (MoveTo8.Length > 0) ParseMove(7, MoveTo8);
        if (MoveTo9.Length > 0) ParseMove(8, MoveTo9);
        if (MoveTo10.Length > 0) ParseMove(9, MoveTo10);
        if (MoveTo11.Length > 0) ParseMove(10, MoveTo11);
        if (MoveTo12.Length > 0) ParseMove(11, MoveTo12);
        if (MoveTo13.Length > 0) ParseMove(12, MoveTo13);
        if (MoveTo14.Length > 0) ParseMove(13, MoveTo14);
        if (MoveTo15.Length > 0) ParseMove(14, MoveTo15);
    }

    private void ParseMove(int MoveNumber, string MoveIn)
    {
        Log.Write("In ParseMove MoveNumber: " + MoveNumber + "  M0veIn: " + MoveIn);
        List<string> MoveArray = new List<string>();
        MoveArray.Clear();
        MoveIn.Replace(" ", string.Empty);
        MoveArray = MoveIn.Split(',').ToList();
        MoveEvent[MoveNumber] = MoveArray[0];
        DoneEvent[MoveNumber] = MoveArray[1];
        MoveVector[MoveNumber].X = float.Parse(MoveArray[2]);
        MoveVector[MoveNumber].Y = float.Parse(MoveArray[3]);
        MoveVector[MoveNumber].Z = float.Parse(MoveArray[4]);
        RotateVector[MoveNumber].X = float.Parse(MoveArray[5]);
        RotateVector[MoveNumber].Y = float.Parse(MoveArray[6]);
        RotateVector[MoveNumber].Z = float.Parse(MoveArray[7]);
        OverTime[MoveNumber] = float.Parse(MoveArray[8]);
        if (MoveArray[9] == "EaseIn") MoveModeArray[MoveNumber] = MoveMode.EaseIn;
        else if (MoveArray[9] == "EaseOut") MoveModeArray[MoveNumber] = MoveMode.EaseOut;
        else if (MoveArray[9] == "Linear") MoveModeArray[MoveNumber] = MoveMode.Linear;
        else if (MoveArray[9] == "Smoothstep") MoveModeArray[MoveNumber] = MoveMode.Smoothstep;
        LoopType[MoveNumber] = MoveArray[10];

        SubscribeToAll(MoveEvent[MoveNumber], ExecuteMovement);
        Log.Write("Subscribed to Move Event: " + MoveEvent[MoveNumber]);
    }

    private void ExecuteMovement(ScriptEventData data)
    {
        if (data.Message == MoveEvent[0]) PlayMovement(0, data);
        else if (data.Message == MoveEvent[1]) PlayMovement(1, data);
        else if (data.Message == MoveEvent[2]) PlayMovement(2, data);
        else if (data.Message == MoveEvent[3]) PlayMovement(3, data);
        else if (data.Message == MoveEvent[4]) PlayMovement(4, data);
        else if (data.Message == MoveEvent[5]) PlayMovement(5, data);
        else if (data.Message == MoveEvent[6]) PlayMovement(6, data);
        else if (data.Message == MoveEvent[7]) PlayMovement(7, data);
        else if (data.Message == MoveEvent[8]) PlayMovement(8, data);
        else if (data.Message == MoveEvent[9]) PlayMovement(9, data);
        else if (data.Message == MoveEvent[10]) PlayMovement(10, data);
        else if (data.Message == MoveEvent[11]) PlayMovement(11, data);
        else if (data.Message == MoveEvent[12]) PlayMovement(12, data);
        else if (data.Message == MoveEvent[13]) PlayMovement(13, data);
        else if (data.Message == MoveEvent[14]) PlayMovement(14, data);
    }

    private void PlayMovement(int MoveNumber, ScriptEventData data)
    {
        Log.Write("PositionOffset: " + PositionOffset);
        PositionOffset.X = MoveVector[MoveNumber].X;
        PositionOffset.Y = MoveVector[MoveNumber].Y;
        PositionOffset.Z = MoveVector[MoveNumber].Z;

        Quaternion currentRot = ObjectPrivate.Rotation;
        Log.Write("currentRot.Z: " + currentRot.Z);

        float CosAngle = (float)Math.Cos(currentRot.GetEulerAngles().Z * 57.2958 * 0.0174533);
        float SinAngle = (float)Math.Sin(currentRot.GetEulerAngles().Z * 57.2958 * 0.0174533);

        float newX = (PositionOffset.X * CosAngle) - (PositionOffset.Y * SinAngle);
        float newY = (PositionOffset.Y * CosAngle) + (PositionOffset.X * SinAngle);

        rotatedPositionOffset.X = newX;
        rotatedPositionOffset.Y = newY;
        rotatedPositionOffset.Z = PositionOffset.Z;
        Log.Write("rotatedPositionOffset: " + rotatedPositionOffset);
        Vector newVector = ObjectPrivate.Position + rotatedPositionOffset;
        if (ObjectPrivate.IsMovable)
        {
            WaitFor(ObjectPrivate.Mover.AddTranslate, newVector, OverTime[MoveNumber], MoveModeArray[MoveNumber]);
            SendToAll(DoneEvent[MoveNumber], data.Data);
        }
    }

}
