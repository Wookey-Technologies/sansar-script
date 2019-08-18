//* "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."

using System;
using System.Linq;

using Sansar;
using Sansar.Script;
using Sansar.Simulation;

public class TriggerVRController : SceneObjectScript

{
    #region ConstantsVariables
    //public bool Debug = false;
    public int timeDelay = 100;
    public bool MultiClick = false;

    private RigidBodyComponent RigidBody = null;


    private IEventSubscription collisionSubscription = null;
    private bool UserIn = false;
    private ObjectId ObjId;
    private AgentPrivate agent; 
    private SimpleData sd;
    private bool VR = false;
    private bool LeftTriggerPressed = false;
    private bool RightTriggerPressed = false;


    #endregion

    public override void Init()
    {
        //Log.Write("VR Init");
        Script.UnhandledException += UnhandledException; // Catch errors and keep running unless fatal

        if (RigidBody == null)
        {
            if (!ObjectPrivate.TryGetFirstComponent(out RigidBody))
            {
                // Since object scripts are initialized when the scene loads, no one will actually see this message.
                Log.Write("There is no RigidBodyComponent attached to this object.");
                //ScenePrivate.Chat.MessageAllUsers("There is no RigidBodyComponent attached to this object.");
                return;
            }
            else
            if (RigidBody.IsTriggerVolume())
            {
                Log.Write("Collision Subscription: " + collisionSubscription);
                if (collisionSubscription == null)
                {
                    collisionSubscription = RigidBody.Subscribe(CollisionEventType.AllCollisions, OnCollision, true);
                    //Log.Write("Subscribed to Collision");
                }
            }
        }
    }

    private void OnCollision(CollisionData data)
    {
        // Check to see if any agent has control
        // If no agent has control, the agent that has just entered the trigger volume is now in control
        // When leaving the agent loses control

        if (data.Phase == CollisionEventPhase.TriggerEnter)
        {
            //Log.Write("Trigger Enter");
            if (!UserIn)
            {
                //Log.Write("CollisionData Data: " + data);
                ObjId = data.HitComponentId.ObjectId;
                agent = ScenePrivate.FindAgent(ObjId);
                agent.Client.SubscribeToCommand("Trigger", CommandAction.Pressed, TriggerCommandPressed, CommandCanceled);
                agent.Client.SubscribeToCommand("Trigger", CommandAction.Released, TriggerCommandReleased, CommandCanceled);

                //Log.Write("Agent: " + agent);
                //Log.Write("Name: " + agent.AgentInfo.Name);
                UserIn = true;
                // Check to See if wearing VR Headset
                if (agent.GetControlPointEnabled(ControlPointType.GazeTarget) || agent.GetControlPointEnabled(ControlPointType.LeftTool))
                {
                    Log.Write("VR");
                    VR = true;
                    //sendSimpleMessage("VR");
                }
                else
                {
                    Log.Write("NoVR");
                    VR = false;
                    //sendSimpleMessage("NoVR");
                }
            }    
        }
        else if (data.Phase == CollisionEventPhase.TriggerExit)
        {
            ObjectId exitObjId = data.HitComponentId.ObjectId;
            AgentPrivate exitAgent = ScenePrivate.FindAgent(ObjId);
            //Log.Write("agent: " + agent);
            //Log.Write("exitObjId  " + exitObjId);
            //Log.Write("exitAgent: " + exitAgent);
            if (agent == exitAgent)
            {
                Log.Write("User has Left: " + agent.AgentInfo.Name);
                UserIn = false;
            }
            else
            {
                Log.Write("Another User had Control: " + agent.AgentInfo.Name);
            }
        }

        if (UserIn && VR) //Active User and they are wearing VR Headset
        {
            //Log.Write("User in Conrol Has VR - Left Tool: " + agent.GetControlPointEnabled(ControlPointType.LeftTool) + " - Right Tool: " + agent.GetControlPointEnabled(ControlPointType.RightTool));
            //UpdatePosition();
        }
    }

    void TriggerCommandPressed(CommandData Button)
    {
        //Log.Write(GetType().Name, "Received Trigger command " + Button.Command + ": " + Button.Action + "Control Point: " + Button.ControlPoint);
        if (Button.ControlPoint == ControlPointType.LeftTool)
        {
            //Log.Write("Left VR Controller Pushed");
            LeftTriggerPressed = true;
            UpdateLeftPosition();
        }
        else if (Button.ControlPoint == ControlPointType.RightTool)
        {
            //Log.Write("Right VR Controller Pushed");
            RightTriggerPressed = true;
            UpdateRightPosition();
        }
    }

    void TriggerCommandReleased(CommandData Button)
    {
        //Log.Write(GetType().Name, "Received Trigger command " + Button.Command + ": " + Button.Action + "Control Point: " + Button.ControlPoint);
        if (Button.ControlPoint == ControlPointType.LeftTool)
        {
            //Log.Write("Left VR Controller Released");
            LeftTriggerPressed = false;
        }
        else if (Button.ControlPoint == ControlPointType.RightTool)
        {
            //Log.Write("Right VR Controller Released");
            RightTriggerPressed = false;
        }
    }

    void CommandCanceled(CancelData data)
    {
        Log.Write(GetType().Name, "Subscription canceled: " + data.Message);
        //do nothing?
    }

    private void UpdateLeftPosition()
    {
        //Log.Write("In UpdatePosition UserIn is: " + UserIn);
        //Log.Write("Left: " + agent.GetControlPointPosition(ControlPointType.LeftTool));
        ObjectPrivate AgentObject = ScenePrivate.FindObject(agent.AgentInfo.ObjectId);

        if (MultiClick)
        {
            do
            {
                Vector AgentPosition = AgentObject.Position;
                Quaternion AgentRotation = AgentObject.Rotation;
                Vector LeftPosition = agent.GetControlPointPosition(ControlPointType.LeftTool);
                Vector WorldLeftPosition = LocalToWorld(AgentPosition, AgentRotation, LeftPosition);

                LeftActionToPerform(LeftPosition, WorldLeftPosition, AgentPosition, AgentRotation);
                Wait(TimeSpan.FromMilliseconds(timeDelay));
            }
            while (UserIn && LeftTriggerPressed);
        }
        else
        {
            //do once
            Vector AgentPosition = AgentObject.Position;
            Quaternion AgentRotation = AgentObject.Rotation;
            Vector LeftPosition = agent.GetControlPointPosition(ControlPointType.LeftTool);
            Vector WorldLeftPosition = LocalToWorld(AgentPosition, AgentRotation, LeftPosition);

            LeftActionToPerform(LeftPosition, WorldLeftPosition, AgentPosition, AgentRotation);
        }

        //Log.Write("Out of Loop");
    }

    private void LeftActionToPerform(Vector LeftPos, Vector WorldPos, Vector AgentPos, Quaternion AgentRot)
    {
        //put your unique game logic here
        Log.Write("Left Button: " + LeftTriggerPressed + " Position: " + AgentPos + " Rotation: " + AgentRot + " Left: " + LeftPos);
    }

    private void UpdateRightPosition()
    {
        //Log.Write("In UpdatePosition UserIn is: " + UserIn);
        //Log.Write("Right: " + agent.GetControlPointPosition(ControlPointType.RightTool));
        ObjectPrivate AgentObject = ScenePrivate.FindObject(agent.AgentInfo.ObjectId);

        if (MultiClick)
        {
            do
            {
                Vector AgentPosition = AgentObject.Position;
                Quaternion AgentRotation = AgentObject.Rotation;
                Vector RightPosition = agent.GetControlPointPosition(ControlPointType.RightTool);
                Vector WorldRightPosition = LocalToWorld(AgentPosition, AgentRotation, RightPosition);

                RightActionToPerform(RightPosition, WorldRightPosition, AgentPosition, AgentRotation);
                Wait(TimeSpan.FromMilliseconds(timeDelay));
            }
            while (UserIn && RightTriggerPressed);
        }
        else
        {
            //do once
            Vector AgentPosition = AgentObject.Position;
            Quaternion AgentRotation = AgentObject.Rotation;
            Vector RightPosition = agent.GetControlPointPosition(ControlPointType.RightTool);
            Vector WorldRightPosition = LocalToWorld(AgentPosition, AgentRotation, RightPosition);

            RightActionToPerform(RightPosition, WorldRightPosition, AgentPosition, AgentRotation);

        }

        //Log.Write("Out of Loop");
    }

    private void RightActionToPerform(Vector RightPos, Vector WorldPos, Vector AgentPos, Quaternion AgentRot)
    {
        //put your unique game logic here
        Log.Write("Right Button: " + RightTriggerPressed + " Position: " + AgentPos + " Rotation: " + AgentRot + " Right: " + RightPos + " World: " + WorldPos);
    }

    Vector LocalToWorld(Vector agentPos, Quaternion agentRot, Vector localPos)
    {
        //Log.Write("agentPos: " + agentPos);
        //Log.Write("agentRot: " + agentRot);
        //Log.Write("localPos: " + localPos);
        Vector WorkPos = localPos.Rotate(agentRot);
        Vector worldPos = agentPos + WorkPos;
        return worldPos;
    }

    private void UnhandledException(object Sender, Exception Ex)
    {
        Log.Write(LogLevel.Error, GetType().Name, Ex.Message + "\n" + Ex.StackTrace + "\n" + Ex.Source);
        return;
    }

    private void sendSimpleMessage(string msg)
    {
        sd = new SimpleData(this);
        sd.ObjectId = ObjId;
        sd.AgentInfo = ScenePrivate.FindAgent(sd.ObjectId)?.AgentInfo;
        sd.SourceObjectId = ObjectPrivate.ObjectId;
        SendToAll(msg, sd);
    }


    #region Communication

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

}
