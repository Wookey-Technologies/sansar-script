/* This content is licensed under the terms of the Creative Commons Attribution 4.0 International License.
 * When using this content, you must:
 * •    Acknowledge that the content is from the Sansar Knowledge Base.
 * •    Include our copyright notice: "© 2017 Linden Research, Inc."
 * •    Indicate that the content is licensed under the Creative Commons Attribution-Share Alike 4.0 International License.
 * •    Include the URL for, or link to, the license summary at https://creativecommons.org/licenses/by-sa/4.0/deed.hi (and, if possible, to the complete license terms at https://creativecommons.org/licenses/by-sa/4.0/legalcode.
 * For example:
 * "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."
 */

using Sansar;
using Sansar.Script;
using Sansar.Simulation;
using System;
using System.Linq;
using System.Collections.Generic;

public class Camera2 : SceneObjectScript
{
    #region EditorProperties
    // Start playing on these events. Can be a comma separated list of event names.
    //public string CameraManName = null;
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
    public string MoveTo16 = null;
    public string MoveTo17 = null;
    public string MoveTo18 = null;
    public string EnableEvent = null;
    public string DisableEvent = null;

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

#region Variables

    private RigidBodyComponent RigidBody;
    private string CameraMan = null;
    private string Director = null;
    private string Executor = null;
    private string LastCameraMan = null;
    private string[] MoveEvent = new string[18];
    private string[] DoneEvent = new string[18];
    private Vector[] MoveVector = new Vector[18];
    private Vector[] RotateVector = new Vector[18];
    private Vector InitialVector;
    private bool NewCameraMan = false;

    #endregion

    public override void Init()
    {
        //if (CameraManName != null) CameraMan = CameraManName;
        if (!ObjectPrivate.TryGetFirstComponent(out RigidBody))
        {
            Log.Write(LogLevel.Error, __SimpleTag, "Simple Mover requires a Rigidbody set to motion type Keyframed");
            return;
        }

        if (RigidBody.GetMotionType() != RigidBodyMotionType.MotionTypeKeyframed)
        {
            Log.Write(LogLevel.Error, __SimpleTag, "Simple Mover requires a Rigidbody set to motion type Keyframed");
            return;
        }

        SubscribeToAll("SetCameraMan", InitializeCameraMan);
        SubscribeToAll("SetDirector", InitializeDirector);

        if (EnableEvent != "")
        {
            Log.Write("Enable Event was not null: " + EnableEvent);
            SubscribeToAll(EnableEvent, ReadInParameters);
        }
        else
        {
            ReadInParameters(null);  //executes it by passing null data
        }

        if (DisableEvent != "")
        {
            SubscribeToAll(DisableEvent, Unsubscribe);
        }
    }

    private void InitializeCameraMan(ScriptEventData sed)
    {
        //Log.Write("In InitializeCameraMan");
        ISimpleData CameraData = sed.Data?.AsInterface<ISimpleData>();
        AgentPrivate CameraAgent = ScenePrivate.FindAgent(CameraData.AgentInfo.SessionId);
        CameraMan = CameraAgent.AgentInfo.Name;
        NewCameraMan = true;
        LocalTeleport(0, sed);
    }

    private void InitializeDirector(ScriptEventData sed)
    {
        Log.Write("In InitializeDirector");
        ISimpleData DirectorData = sed.Data?.AsInterface<ISimpleData>();
        AgentPrivate DirectorAgent = ScenePrivate.FindAgent(DirectorData.AgentInfo.SessionId);
        Director = DirectorAgent.AgentInfo.Name;
    }

    private void ReadInParameters(ScriptEventData sed)  //doesn't really pass data.  Always passes null
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
        if (MoveTo16.Length > 0) ParseMove(15, MoveTo16);
        if (MoveTo17.Length > 0) ParseMove(16, MoveTo17);
        //if (MoveTo18.Length > 0) ParseMove(17, MoveTo18);
    }

    private void Unsubscribe(ScriptEventData sed)
    {

    }

    private void ParseMove(int MoveNumber, string MoveIn)
    {
        Log.Write("In ParseMove MoveNumber: " + MoveNumber + "  MoveIn: " + MoveIn);
        List<string> MoveArray = new List<string>();
        MoveArray.Clear();
        MoveIn.Replace(" ", string.Empty);
        MoveArray = MoveIn.Split(',').ToList();
        MoveEvent[MoveNumber] = MoveArray[0];
        //DoneEvent[MoveNumber] = MoveArray[1];
        MoveVector[MoveNumber].X = float.Parse(MoveArray[1]);
        MoveVector[MoveNumber].Y = float.Parse(MoveArray[2]);
        MoveVector[MoveNumber].Z = float.Parse(MoveArray[3]);
        RotateVector[MoveNumber].X = 0;
        RotateVector[MoveNumber].Y = 0;
        RotateVector[MoveNumber].Z = float.Parse(MoveArray[4]);

        SubscribeToAll(MoveEvent[MoveNumber], ExecuteMovement);
        Log.Write("Move Event: " + MoveEvent[MoveNumber]);
        Log.Write("Finished ParseMove");
    }

    private void ExecuteMovement(ScriptEventData data)
    {
        Log.Write("In Execute Animation data message: " + data.Message);
        ISimpleData ExecuteData = data.Data?.AsInterface<ISimpleData>();
        AgentPrivate ExecuteAgent = ScenePrivate.FindAgent(ExecuteData.AgentInfo.SessionId);
        Executor = ExecuteAgent.AgentInfo.Name;
        Log.Write("Executor: " + Executor);
        Log.Write("Director: " + Director);
        if (Executor == Director)
        {
            if (data.Message == MoveEvent[0]) LocalTeleport(0, data);
            else if (data.Message == MoveEvent[1]) LocalTeleport(1, data);
            else if (data.Message == MoveEvent[2]) LocalTeleport(2, data);
            else if (data.Message == MoveEvent[3]) LocalTeleport(3, data);
            else if (data.Message == MoveEvent[4]) LocalTeleport(4, data);
            else if (data.Message == MoveEvent[5]) LocalTeleport(5, data);
            else if (data.Message == MoveEvent[6]) LocalTeleport(6, data);
            else if (data.Message == MoveEvent[7]) LocalTeleport(7, data);
            else if (data.Message == MoveEvent[8]) LocalTeleport(8, data);
            else if (data.Message == MoveEvent[9]) LocalTeleport(9, data);
            else if (data.Message == MoveEvent[10]) LocalTeleport(10, data);
            else if (data.Message == MoveEvent[11]) LocalTeleport(11, data);
            else if (data.Message == MoveEvent[12]) LocalTeleport(12, data);
            else if (data.Message == MoveEvent[13]) LocalTeleport(13, data);
            else if (data.Message == MoveEvent[14]) LocalTeleport(14, data);
            else if (data.Message == MoveEvent[15]) LocalTeleport(15, data);
            else if (data.Message == MoveEvent[16]) LocalTeleport(16, data);
            else if (data.Message == MoveEvent[17]) LocalTeleport(17, data);
        }
    }

    private void LocalTeleport(int MoveNumber, ScriptEventData data)
    {
        Log.Write("MoveNumber: " + MoveNumber);
        LastCameraMan = CameraMan;
        foreach (AgentPrivate agent in ScenePrivate.GetAgents())
        {
            Log.Write(agent.AgentInfo.Name);
            if (agent.AgentInfo.Name == CameraMan)
            {
                Log.Write("Camaeraman found");
                ObjectPrivate objectPrivate = ScenePrivate.FindObject(agent.AgentInfo.ObjectId);
                if (objectPrivate != null)
                {
                    AnimationComponent anim = null;
                    if (objectPrivate.TryGetFirstComponent(out anim))
                    {
                        RigidBody.SetPosition(MoveVector[MoveNumber]);
                        Wait(TimeSpan.FromSeconds(0.05));
                        anim.SetPosition(MoveVector[MoveNumber]);
                        Wait(TimeSpan.FromSeconds(0.05));
                        Quaternion rotation = Quaternion.FromEulerAngles(Mathf.RadiansPerDegree * RotateVector[MoveNumber]);
                        RigidBody.SetOrientation(rotation);
                        //PlayMovement(MoveNumber, data);

                    }
                }
            }
        }
    }

}


