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

//Version used using normal collisions with an object and not 

public class TriggerKeyPressed3 : SceneObjectScript

{

    #region ConstantsVariables
    public string BaseEventName = null;
    public bool UnSubOnExit = true;
    private SessionId Jammer = new SessionId();
    AgentPrivate TheUser = null;
    List<IEventSubscription> ButtonSubscriptions = new List<IEventSubscription>();
    private bool Shifted = false;
    private AgentPrivate Hitman;
    private RigidBodyComponent rigidBody;
    private CollisionData collsionData;

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


    void SubscribeClientToButton(Client client, string Button)
    {
        ButtonSubscriptions.Add(client.SubscribeToCommand(Button, CommandAction.Pressed, CommandReceived, CommandCanceled));
        ButtonSubscriptions.Add(client.SubscribeToCommand(Button, CommandAction.Released, CommandReceived, CommandCanceled));
    }

    void UnsubscribeAllButtons()
    {
        foreach (IEventSubscription sub in ButtonSubscriptions) sub.Unsubscribe();
        ButtonSubscriptions.Clear();
    }

    void SubscribeKeyPressed(AgentPrivate agent, string Message)
    {
        if (agent == null) return;

        if (Message == "sub" && TheUser == null)
        {
            TheUser = agent;
            Client client = agent.Client;
            SubscribeClientToButton(client, "Keypad0");
            SubscribeClientToButton(client, "Keypad1");
            SubscribeClientToButton(client, "Keypad2");
            SubscribeClientToButton(client, "Keypad3");
            SubscribeClientToButton(client, "Keypad4");
            SubscribeClientToButton(client, "Keypad5");
            SubscribeClientToButton(client, "Keypad6");
            SubscribeClientToButton(client, "Keypad7");
            SubscribeClientToButton(client, "Keypad8");
            SubscribeClientToButton(client, "Keypad9");
            SubscribeClientToButton(client, "Action1");
            SubscribeClientToButton(client, "Action2");
            SubscribeClientToButton(client, "Action3");
            SubscribeClientToButton(client, "Action4");
            SubscribeClientToButton(client, "Action5");
            SubscribeClientToButton(client, "Action6");
            SubscribeClientToButton(client, "Action7");
            SubscribeClientToButton(client, "Action8");
            SubscribeClientToButton(client, "Action9");
            SubscribeClientToButton(client, "Action0");
            SubscribeClientToButton(client, "Modifier");
        }

        if (Message == "unsub")
        {
            UnsubscribeAllButtons();
            TheUser = null;
            return;
        }
    }

    void CommandReceived(CommandData Button)
    {
        if ((Button.Command == "Modifier") && (Button.Action == CommandAction.Pressed))  //shift acts a toggle
        {
            if (Shifted == true) Shifted = false;
            else Shifted = true;
        }

        if ((Button.Command == "Action1") && (Button.Action == CommandAction.Pressed) && (!(Shifted))) SendKey(BaseEventName + "1");
        else if ((Button.Command == "Action2") && (Button.Action == CommandAction.Pressed) && (!(Shifted))) SendKey(BaseEventName + "2");
        else if ((Button.Command == "Action3") && (Button.Action == CommandAction.Pressed) && (!(Shifted))) SendKey(BaseEventName + "3");
        else if ((Button.Command == "Action4") && (Button.Action == CommandAction.Pressed) && (!(Shifted))) SendKey(BaseEventName + "4");
        else if ((Button.Command == "Action5") && (Button.Action == CommandAction.Pressed) && (!(Shifted))) SendKey(BaseEventName + "5");
        else if ((Button.Command == "Action6") && (Button.Action == CommandAction.Pressed) && (!(Shifted))) SendKey(BaseEventName + "6");
        else if ((Button.Command == "Action7") && (Button.Action == CommandAction.Pressed) && (!(Shifted))) SendKey(BaseEventName + "7");
        else if ((Button.Command == "Action8") && (Button.Action == CommandAction.Pressed) && (!(Shifted))) SendKey(BaseEventName + "8");
        else if ((Button.Command == "Action9") && (Button.Action == CommandAction.Pressed) && (!(Shifted))) SendKey(BaseEventName + "9");
        else if ((Button.Command == "Action0") && (Button.Action == CommandAction.Pressed) && (!(Shifted))) SendKey(BaseEventName + "10");
        else if ((Button.Command == "Keypad0") && (Button.Action == CommandAction.Pressed) && (!(Shifted))) SendKey(BaseEventName + "11");
        else if ((Button.Command == "Keypad1") && (Button.Action == CommandAction.Pressed) && (!(Shifted))) SendKey(BaseEventName + "12");
        else if ((Button.Command == "Keypad2") && (Button.Action == CommandAction.Pressed) && (!(Shifted))) SendKey(BaseEventName + "13");
        else if ((Button.Command == "Keypad3") && (Button.Action == CommandAction.Pressed) && (!(Shifted))) SendKey(BaseEventName + "14");
        else if ((Button.Command == "Keypad4") && (Button.Action == CommandAction.Pressed) && (!(Shifted))) SendKey(BaseEventName + "15");
        else if ((Button.Command == "Keypad5") && (Button.Action == CommandAction.Pressed) && (!(Shifted))) SendKey(BaseEventName + "16");
        else if ((Button.Command == "Keypad6") && (Button.Action == CommandAction.Pressed) && (!(Shifted))) SendKey(BaseEventName + "17");
        else if ((Button.Command == "Keypad7") && (Button.Action == CommandAction.Pressed) && (!(Shifted))) SendKey(BaseEventName + "18");
        else if ((Button.Command == "Keypad8") && (Button.Action == CommandAction.Pressed) && (!(Shifted))) SendKey(BaseEventName + "19");
        else if ((Button.Command == "Keypad9") && (Button.Action == CommandAction.Pressed) && (!(Shifted))) SendKey(BaseEventName + "20");
        else if ((Button.Command == "Action1") && (Button.Action == CommandAction.Pressed) && (Shifted)) SendKey(BaseEventName + "21");
        else if ((Button.Command == "Action2") && (Button.Action == CommandAction.Pressed) && (Shifted)) SendKey(BaseEventName + "22");
        else if ((Button.Command == "Action3") && (Button.Action == CommandAction.Pressed) && (Shifted)) SendKey(BaseEventName + "23");
        else if ((Button.Command == "Action4") && (Button.Action == CommandAction.Pressed) && (Shifted)) SendKey(BaseEventName + "24");
        else if ((Button.Command == "Action5") && (Button.Action == CommandAction.Pressed) && (Shifted)) SendKey(BaseEventName + "25");
        else if ((Button.Command == "Action6") && (Button.Action == CommandAction.Pressed) && (Shifted)) SendKey(BaseEventName + "26");
        else if ((Button.Command == "Action7") && (Button.Action == CommandAction.Pressed) && (Shifted)) SendKey(BaseEventName + "27");
        else if ((Button.Command == "Action8") && (Button.Action == CommandAction.Pressed) && (Shifted)) SendKey(BaseEventName + "28");
        else if ((Button.Command == "Action9") && (Button.Action == CommandAction.Pressed) && (Shifted)) SendKey(BaseEventName + "29");
        else if ((Button.Command == "Action0") && (Button.Action == CommandAction.Pressed) && (Shifted)) SendKey(BaseEventName + "30");
        else if ((Button.Command == "Keypad0") && (Button.Action == CommandAction.Pressed) && (Shifted)) SendKey(BaseEventName + "31");
        else if ((Button.Command == "Keypad1") && (Button.Action == CommandAction.Pressed) && (Shifted)) SendKey(BaseEventName + "32");
        else if ((Button.Command == "Keypad2") && (Button.Action == CommandAction.Pressed) && (Shifted)) SendKey(BaseEventName + "33");
        else if ((Button.Command == "Keypad3") && (Button.Action == CommandAction.Pressed) && (Shifted)) SendKey(BaseEventName + "34");
        else if ((Button.Command == "Keypad4") && (Button.Action == CommandAction.Pressed) && (Shifted)) SendKey(BaseEventName + "35");
        else if ((Button.Command == "Keypad5") && (Button.Action == CommandAction.Pressed) && (Shifted)) SendKey(BaseEventName + "36");
        else if ((Button.Command == "Keypad6") && (Button.Action == CommandAction.Pressed) && (Shifted)) SendKey(BaseEventName + "37");
        else if ((Button.Command == "Keypad7") && (Button.Action == CommandAction.Pressed) && (Shifted)) SendKey(BaseEventName + "38");
        else if ((Button.Command == "Keypad8") && (Button.Action == CommandAction.Pressed) && (Shifted)) SendKey(BaseEventName + "39");
        else if ((Button.Command == "Keypad9") && (Button.Action == CommandAction.Pressed) && (Shifted)) SendKey(BaseEventName + "40");

        if ((Button.Command == "Action1") && (Button.Action == CommandAction.Released) && (!(Shifted))) SendKey(BaseEventName + "1Off");
        else if ((Button.Command == "Action2") && (Button.Action == CommandAction.Released) && (!(Shifted))) SendKey(BaseEventName + "2Off");
        else if ((Button.Command == "Action3") && (Button.Action == CommandAction.Released) && (!(Shifted))) SendKey(BaseEventName + "3Off");
        else if ((Button.Command == "Action4") && (Button.Action == CommandAction.Released) && (!(Shifted))) SendKey(BaseEventName + "4Off");
        else if ((Button.Command == "Action5") && (Button.Action == CommandAction.Released) && (!(Shifted))) SendKey(BaseEventName + "5Off");
        else if ((Button.Command == "Action6") && (Button.Action == CommandAction.Released) && (!(Shifted))) SendKey(BaseEventName + "6Off");
        else if ((Button.Command == "Action7") && (Button.Action == CommandAction.Released) && (!(Shifted))) SendKey(BaseEventName + "7Off");
        else if ((Button.Command == "Action8") && (Button.Action == CommandAction.Released) && (!(Shifted))) SendKey(BaseEventName + "8Off");
        else if ((Button.Command == "Action9") && (Button.Action == CommandAction.Released) && (!(Shifted))) SendKey(BaseEventName + "9Off");
        else if ((Button.Command == "Action0") && (Button.Action == CommandAction.Released) && (!(Shifted))) SendKey(BaseEventName + "10Off");
        else if ((Button.Command == "Keypad0") && (Button.Action == CommandAction.Released) && (!(Shifted))) SendKey(BaseEventName + "11Off");
        else if ((Button.Command == "Keypad1") && (Button.Action == CommandAction.Released) && (!(Shifted))) SendKey(BaseEventName + "12Off");
        else if ((Button.Command == "Keypad2") && (Button.Action == CommandAction.Released) && (!(Shifted))) SendKey(BaseEventName + "13Off");
        else if ((Button.Command == "Keypad3") && (Button.Action == CommandAction.Released) && (!(Shifted))) SendKey(BaseEventName + "14Off");
        else if ((Button.Command == "Keypad4") && (Button.Action == CommandAction.Released) && (!(Shifted))) SendKey(BaseEventName + "15Off");
        else if ((Button.Command == "Keypad5") && (Button.Action == CommandAction.Released) && (!(Shifted))) SendKey(BaseEventName + "16Off");
        else if ((Button.Command == "Keypad6") && (Button.Action == CommandAction.Released) && (!(Shifted))) SendKey(BaseEventName + "17Off");
        else if ((Button.Command == "Keypad7") && (Button.Action == CommandAction.Released) && (!(Shifted))) SendKey(BaseEventName + "18Off");
        else if ((Button.Command == "Keypad8") && (Button.Action == CommandAction.Released) && (!(Shifted))) SendKey(BaseEventName + "19Off");
        else if ((Button.Command == "Keypad9") && (Button.Action == CommandAction.Released) && (!(Shifted))) SendKey(BaseEventName + "2Off");
        else if ((Button.Command == "Action1") && (Button.Action == CommandAction.Released) && (Shifted)) SendKey(BaseEventName + "21Off");
        else if ((Button.Command == "Action2") && (Button.Action == CommandAction.Released) && (Shifted)) SendKey(BaseEventName + "22Off");
        else if ((Button.Command == "Action3") && (Button.Action == CommandAction.Released) && (Shifted)) SendKey(BaseEventName + "23Off");
        else if ((Button.Command == "Action4") && (Button.Action == CommandAction.Released) && (Shifted)) SendKey(BaseEventName + "24Off");
        else if ((Button.Command == "Action5") && (Button.Action == CommandAction.Released) && (Shifted)) SendKey(BaseEventName + "25Off");
        else if ((Button.Command == "Action6") && (Button.Action == CommandAction.Released) && (Shifted)) SendKey(BaseEventName + "26Off");
        else if ((Button.Command == "Action7") && (Button.Action == CommandAction.Released) && (Shifted)) SendKey(BaseEventName + "27Off");
        else if ((Button.Command == "Action8") && (Button.Action == CommandAction.Released) && (Shifted)) SendKey(BaseEventName + "28Off");
        else if ((Button.Command == "Action9") && (Button.Action == CommandAction.Released) && (Shifted)) SendKey(BaseEventName + "29Off");
        else if ((Button.Command == "Action0") && (Button.Action == CommandAction.Released) && (Shifted)) SendKey(BaseEventName + "30Off");
        else if ((Button.Command == "Keypad0") && (Button.Action == CommandAction.Released) && (Shifted)) SendKey(BaseEventName + "31Off");
        else if ((Button.Command == "Keypad1") && (Button.Action == CommandAction.Released) && (Shifted)) SendKey(BaseEventName + "32Off");
        else if ((Button.Command == "Keypad2") && (Button.Action == CommandAction.Released) && (Shifted)) SendKey(BaseEventName + "33Off");
        else if ((Button.Command == "Keypad3") && (Button.Action == CommandAction.Released) && (Shifted)) SendKey(BaseEventName + "34Off");
        else if ((Button.Command == "Keypad4") && (Button.Action == CommandAction.Released) && (Shifted)) SendKey(BaseEventName + "35Off");
        else if ((Button.Command == "Keypad5") && (Button.Action == CommandAction.Released) && (Shifted)) SendKey(BaseEventName + "36Off");
        else if ((Button.Command == "Keypad6") && (Button.Action == CommandAction.Released) && (Shifted)) SendKey(BaseEventName + "37Off");
        else if ((Button.Command == "Keypad7") && (Button.Action == CommandAction.Released) && (Shifted)) SendKey(BaseEventName + "38Off");
        else if ((Button.Command == "Keypad8") && (Button.Action == CommandAction.Released) && (Shifted)) SendKey(BaseEventName + "39Off");
        else if ((Button.Command == "Keypad9") && (Button.Action == CommandAction.Released) && (Shifted)) SendKey(BaseEventName + "40Off");
    }

    void CommandCanceled(CancelData data)
    {
        Log.Write(GetType().Name, "Subscription canceled: " + data.Message);
    }

    #endregion

    public override void Init()
    {
        Script.UnhandledException += UnhandledException; // Catch errors and keep running unless fatal
        Log.Write("In TriggerPressed3");
        if (ObjectPrivate.TryGetFirstComponent(out rigidBody))
        {
            Log.Write("Calling OnCollide");
            rigidBody.Subscribe(CollisionEventType.AllCollisions, OnCollide);
        }
        else
        {
        }
    }

    private void UnhandledException(object Sender, Exception Ex)
    {
        Log.Write(LogLevel.Error, GetType().Name, Ex.Message + "\n" + Ex.StackTrace + "\n" + Ex.Source);
        return;
    }//UnhandledException

    private void OnCollide(CollisionData Data)
    {

        Log.Write("In OnCollide");
        collsionData = Data;
        Hitman = ScenePrivate.FindAgent(Data.HitComponentId.ObjectId);
        SceneInfo info = ScenePrivate.SceneInfo;
        Jammer = Hitman.AgentInfo.SessionId;
        SubscribeKeyPressed(Hitman, "unsub");
        Wait(TimeSpan.FromSeconds(2));  //wait a couple of seconds for the keys to unsubscribe

        SubscribeKeyPressed(Hitman, "sub");
        Log.Write("Keys subscribed for Director: " + Hitman.AgentInfo.Name);
    }

    void SendKey(string KeyIn)
    {
        sendSimpleMessage(KeyIn, collsionData);
    }

    private void sendSimpleMessage(string msg, CollisionData data)
    {
        SimpleData sd = new SimpleData(this);
        sd.AgentInfo = Hitman.AgentInfo;
        sd.ObjectId = data.HitObject.ObjectId;
        sd.SourceObjectId = ObjectPrivate.ObjectId;
        SendToAll(msg, sd);
    }
}
