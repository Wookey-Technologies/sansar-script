//* "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."

using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

using Sansar;
using Sansar.Script;
using Sansar.Simulation;

// My Documentation


public class CustomKeySend : SceneObjectScript

{
    #region ConstantsVariables
    public readonly Interaction ComplexInteraction;

    public string ChannelOut;
    public string KeyToSend;
    public bool Debug = false;

    private string UsersToListenTo = "ALL";
    private List<string> ValidUsers = new List<string>();
    bool validUser = false;

    private AgentPrivate Hitman;
    private AgentInfo Jammer;
    private RigidBodyComponent RigidBody = null;


    #endregion

    public override void Init()
    {
        Log.Write("In CustomLooerController");
        //ComplexInteraction.SetPrompt(ComplexInteraction);
        Script.UnhandledException += UnhandledException; // Catch errors and keep running unless fatal
        //Change = "Stuff";
        if (RigidBody == null)
        {
            if (!ObjectPrivate.TryGetFirstComponent(out RigidBody))
            {
                // Since object scripts are initialized when the scene loads, no one will actually see this message.
                ScenePrivate.Chat.MessageAllUsers("There is no RigidBodyComponent attached to this object.");
                return;
            }
        }
        SubscribeToScriptEvent("SecurityList", getSecurity);
        ComplexInteractionHandler();
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

        public Reflective ExtraData { get; set;  }
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

    public interface ISendSecurityInfo
    {
        string SecurityString { get; }
    }

    private void getSecurity(ScriptEventData gotSecurity)
    {
        Log.Write("ComplexLooperController: In getSecurity");
        if (gotSecurity.Data == null)
        {
            return;
        }

        ISendSecurityInfo sendSecurityInfo = gotSecurity.Data.AsInterface<ISendSecurityInfo>();
        if (sendSecurityInfo == null)
        {
            Log.Write(LogLevel.Error, Script.ID.ToString(), "Unable to create interface, check logs for missing member(s)");
            return;
        }

        UsersToListenTo = sendSecurityInfo.SecurityString;
        Log.Write("CustomControllerLooper UsersToListenTo After getSecurity: " + UsersToListenTo);
    }

    public class SendKeyInfo : Reflective
    {
        public string iChannelOut { get; set; }
        public string iKeySent { get; set; }
    }

    private void ComplexInteractionHandler()
    {
        //Log.Write("In ComplexInteractionHandler");
        ComplexInteraction.Subscribe((InteractionData idata) =>
        {
            if (Debug)
            {
                ComplexInteraction.SetPrompt("Debug: "
                    + "\nHit:" + idata.HitPosition.ToString()
                    + "\nBy:" + ScenePrivate.FindAgent(idata.AgentId).AgentInfo.Name);
                //Vector hitPosition = idata.HitPosition;
                //Log.Write("Hit:  " + idata.HitPosition.ToString());
            }

            Log.Write("Interacting person: " + ScenePrivate.FindAgent(idata.AgentId).AgentInfo.Name);
            ValidUsers.Clear();
            ValidUsers = UsersToListenTo.Split(',').ToList();
            if (UsersToListenTo.Contains("ALL"))
            {
                Log.Write("Valid User: ALL");
                validUser = true;
            }
            else
            {
                foreach (string ValidUser in ValidUsers)
                {
                    Log.Write("ValidUser: " + ValidUser);
                    if (ScenePrivate.FindAgent(idata.AgentId).AgentInfo.Name == ValidUser.Trim())
                    {
                        validUser = true;
                    }
                }
            }
            if (!validUser)
            {
                //ComplexInteraction.SetPrompt("You Are Not Authorized to Use The Looper");
                //Vector hitPosition = idata.HitPosition;
                //Log.Write("Hit:  " + idata.HitPosition.ToString());
            }
            else
            {
                SendKeyInteraction();
                //sendSimpleMessage(KeyToSend, idata);
            }
        });
    }


    #endregion

    #region Interaction

    private void sendSimpleMessage(string msg, InteractionData data)
    {
        SimpleData sd = new SimpleData(this);
        //SimpleDataExt thisObjectDataExt = new SimpleDataExt(this);
        sd.AgentInfo = ScenePrivate.FindAgent(data.AgentId)?.AgentInfo;
        sd.ObjectId = sd.AgentInfo.ObjectId;
        sd.SourceObjectId = ObjectPrivate.ObjectId;
        // assign our data to reflective data field
        SendToAll(msg, sd);
        Log.Write("Key Message Sent");
    }

    private void SendKeyInteraction()
    {
        SendKeyInfo sendKeyInfo = new SendKeyInfo();
        sendKeyInfo.iChannelOut = ChannelOut;
        sendKeyInfo.iKeySent = KeyToSend;
        PostScriptEvent(ScriptId.AllScripts, "KeySent", sendKeyInfo);
        Log.Write("Key Interaction Sent");
    }

    #endregion
}
