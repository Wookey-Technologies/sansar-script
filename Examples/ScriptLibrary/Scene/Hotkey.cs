/* This content is licensed under the terms of the Creative Commons Attribution 4.0 International License.
 * When using this content, you must:
 * •    Acknowledge that the content is from the Sansar Knowledge Base.
 * •    Include our copyright notice: "© 2017 Linden Research, Inc."
 * •    Indicate that the content is licensed under the Creative Commons Attribution-Share Alike 4.0 International License.
 * •    Include the URL for, or link to, the license summary at https://creativecommons.org/licenses/by-sa/4.0/deed.hi (and, if possible, to the complete license terms at https://creativecommons.org/licenses/by-sa/4.0/legalcode.
 * For example:
 * "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."
 */

using Sansar.Script;
using Sansar.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptLibrary
{
    [Tooltip("Sends simple script events on keyboard and controller button presses.")]
    [DisplayName(nameof(Hotkey))]
    public class Hotkey : LibraryBase // SubscribeToCommand requires Client requires AgentPrivate requires ScenePrivate
    {
        #region EditorProperties
        [Tooltip(@"The commands to listen for to send ""On Command 1 ->"" events. See CommandData documentation for the list of available commands.
A comma separated list of commands can be used to designate a key combo: all keys listed must be pressed at the same time to trigger the events.")]
        [DefaultValue("PrimaryAction")]
        [DisplayName("Command 1")]
        public readonly string Command1;

        [Tooltip("The events to send when the key or key combo from Command 1 is pressed.")]
        [DefaultValue("on")]
        [DisplayName("On Command 1 ->")]
        public readonly string Command1Event;

        [Tooltip("The events to send when the key or key combo from Command 1 is released.")]
        [DefaultValue("")]
        [DisplayName("On Command 1 Release ->")]
        public readonly string Command1ReleaseEvent;

        [Tooltip(@"The commands to listen for to send ""On Command 2 ->"" events.  See CommandData documentation for the list of available commands.
A comma separated list of commands can be used to designate a key combo: all keys listed must be pressed at the same time to trigger the events.")]
        [DefaultValue("SecondaryAction")]
        [DisplayName("Command 2")]
        public readonly string Command2;

        [Tooltip("The events to send when the key or key combo from Command 2 is pressed.")]
        [DefaultValue("off")]
        [DisplayName("On Command 2 ->")]
        public readonly string Command2Event;

        [Tooltip("The events to send when the key or key combo from Command 2 is released.")]
        [DefaultValue("")]
        [DisplayName("On Command 2 Release ->")]
        public readonly string Command2ReleaseEvent;

        [Tooltip(@"The commands to listen for to send ""On Command 3 ->"" events.  See CommandData documentation for the list of available commands.
A comma separated list of commands can be used to designate a key combo: all keys listed must be pressed at the same time to trigger the events.")]
        [DefaultValue("Confirm")]
        [DisplayName("Command 3")]
        public readonly string Command3;

        [Tooltip("The events to send when the key or key combo from Command 3 is pressed.")]
        [DefaultValue("on")]
        [DisplayName("On Command 3 ->")]
        public readonly string Command3Event;

        [Tooltip("The events to send when the key or key combo from Command 3 is released.")]
        [DefaultValue("on")]
        [DisplayName("On Command 3 Release ->")]
        public readonly string Command3ReleaseEvent;

        [Tooltip("Advanced Use: Add up to 20 hotkey subscriptions.\nEach entry describes one subscription like this: '+Trigger:event1,event2'"
            + "\nThe first character can be + for key press only, - for release only or neither for both."
            + "\nNext must be the command names followed by a colon. This can be several commands separated by commas for key combos."
            + "\nThe rest of the entry should be the events to send.")]
        [DisplayName("** Extra Commands")]
        public readonly List<string> AdvancedCommands;

        [Tooltip("If Owner Restricted is set to true, then the commands in this script only respond to key-presses by the scene owner.")]
        [DefaultValue(false)]
        [DisplayName("Owner Restricted")]
        public readonly bool OwnerOnly;

        [Tooltip("Disable responding to commands for this script")]
        [DefaultValue("hotkey_enable")]
        [DisplayName("-> Enable")]
        public readonly string EnableEvent;

        [Tooltip("Disable responding to commands for this script")]
        [DefaultValue("hotkey_disable")]
        [DisplayName("-> Disable")]
        public readonly string DisableEvent;

        [Tooltip(@"If StartEnabled is true then the script will respond to commands when the scene is loaded
If StartEnabled is false then the script will not respond to commands until an (-> Enable) event is received.")]
        [DefaultValue(true)]
        [DisplayName("Start Enabled")]
        public readonly bool StartEnabled = true;
        #endregion

        [Flags]
        enum CommandFlag
        {
            Invalid = 0,
            PrimaryAction = 0x1 << 00,
            SecondaryAction = 0x1 << 01,
            Modifier = 0x1 << 02,
            Action1 = 0x1 << 03,
            Action2 = 0x1 << 04,
            Action3 = 0x1 << 05,
            Action4 = 0x1 << 06,
            Action5 = 0x1 << 07,
            Action6 = 0x1 << 08,
            Action7 = 0x1 << 09,
            Action8 = 0x1 << 10,
            Action9 = 0x1 << 11,
            Action0 = 0x1 << 12,
            Confirm = 0x1 << 13,
            Cancel = 0x1 << 14,
            SelectLeft = 0x1 << 15,
            SelectRight = 0x1 << 16,
            SelectUp = 0x1 << 17,
            SelectDown = 0x1 << 18,
            Keypad0 = 0x1 << 19,
            Keypad1 = 0x1 << 20,
            Keypad2 = 0x1 << 21,
            Keypad3 = 0x1 << 22,
            Keypad4 = 0x1 << 23,
            Keypad5 = 0x1 << 24,
            Keypad6 = 0x1 << 25,
            Keypad7 = 0x1 << 26,
            Keypad8 = 0x1 << 27,
            Keypad9 = 0x1 << 28,
            KeypadEnter = 0x1 << 29,
            Trigger = 0x1 << 30
        }

        CommandFlag flagFromString(string command)
        {
            try
            {
                return (CommandFlag)Enum.Parse(typeof(CommandFlag), command);
            }
            catch (ArgumentException)
            {
                Log.Write(LogLevel.Warning, "Invalid command '" + command + "' in SimpleHotkey.cs");
                return CommandFlag.Invalid;
            }
        }

        Dictionary<SessionId, CommandFlag> HeldKeys = new Dictionary<SessionId, CommandFlag>();
        Dictionary<CommandFlag, string> CommandEvents = new Dictionary<CommandFlag, string>();
        Dictionary<CommandFlag, string> CommandReleaseEvents = new Dictionary<CommandFlag, string>();
        Action<Client> Subscribe = null;
        bool Enabled;

        protected override void SimpleInit()
        {
            Setup(Command1, Command1Event, Command1ReleaseEvent);
            Setup(Command2, Command2Event, Command2ReleaseEvent);
            Setup(Command3, Command3Event, Command3ReleaseEvent);
            AdvancedSetup();

            Enabled = StartEnabled;

            SubscribeToAll(EnableEvent, (data) => { Enabled = true; });
            SubscribeToAll(DisableEvent, (data) => { Enabled = false; });

            Action<SessionId> addUserAction = (SessionId userId) =>
            {
                if (OwnerOnly)
                {
                    var ap = ScenePrivate.FindAgent(userId);
                    if (ap == null || ap.AgentInfo.AvatarUuid != ScenePrivate.SceneInfo.AvatarUuid)
                    {
                        return; // early exit because they aren't the owner.
                    }
                }

                if (!HeldKeys.ContainsKey(userId))
                {
                    HeldKeys.Add(userId, 0);
                }
                Client client = ScenePrivate.FindAgent(userId)?.Client;
                if (client != null) Subscribe(client);
            };

            ScenePrivate.User.Subscribe("AddUser", (UserData data) => addUserAction(data.User));

            // Catch any users already in the scene.
            foreach(AgentPrivate agent in ScenePrivate.GetAgents())
            {
                try
                {
                    addUserAction(agent.AgentInfo.SessionId);
                } catch { };
            }

            ScenePrivate.User.Subscribe("RemoveUser", Unsubscribe);
        }

        void AdvancedSetup()
        {
            foreach(string advCmd in AdvancedCommands)
            {
                string[] cmdsplit = advCmd.Trim().Split(':');
                if (cmdsplit.Length != 2
                    || string.IsNullOrWhiteSpace(cmdsplit[0])
                    || string.IsNullOrWhiteSpace(cmdsplit[1]))
                {
                    Log.Write(LogLevel.Warning, "Advanced command is improperly formatted, must contain a : to separate command from events.");
                }

                if (cmdsplit[0][0] == '+')
                {
                    Setup(cmdsplit[0].Remove(0, 1).Trim(), cmdsplit[1], null);
                }
                else if (cmdsplit[0][0] == '-')
                {
                    Setup(cmdsplit[0].Remove(0, 1).Trim(), null, cmdsplit[1]);
                }
                else
                {
                    Setup(cmdsplit[0].Trim(), cmdsplit[1], cmdsplit[1]);
                }
            }
        }

        void Setup(string commandParam, string commandEventParam, string commandReleaseEventParam)
        {
            if (string.IsNullOrWhiteSpace(commandParam))
            {
                if (!string.IsNullOrWhiteSpace(commandEventParam))
                {
                    Log.Write(LogLevel.Warning, "Command event '" + commandEventParam + "' has no associated command in SimpleHotkey.cs");
                }
                if (!string.IsNullOrWhiteSpace(commandReleaseEventParam))
                {
                    Log.Write(LogLevel.Warning, "Command release event '" + commandReleaseEventParam + "' has no associated command in SimpleHotkey.cs");
                }
                return;
            }

            string[] commands = commandParam.Trim().Split(',');
            CommandFlag combo = 0;
            foreach (string command in commands)
            {
                CommandFlag flag = flagFromString(command);
                if (flag == CommandFlag.Invalid)
                {
                    continue;
                }
                combo |= flag;

                Subscribe += (Client client) =>
                {
                    client.SubscribeToCommand(command, CommandAction.All, OnCommand, null);
                };
            }
            if (combo == 0)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(commandEventParam))
            {
                CommandEvents.Add(combo, commandEventParam);
            }

            if (!string.IsNullOrWhiteSpace(commandReleaseEventParam))
            {
                CommandReleaseEvents.Add(combo, commandReleaseEventParam);
            }
        }

        void Unsubscribe(UserData data)
        {
            HeldKeys.Remove(data.User);
        }

        void OnCommand(CommandData data)
        {
            if (!Enabled) return;

            CommandFlag currentHotkey = flagFromString(data.Command);
            if (currentHotkey == CommandFlag.Invalid)
                return;

            if (data.Action == CommandAction.Pressed)
            {
                if ((currentHotkey & HeldKeys[data.SessionId]) == 0x0)
                {
                    string sendEvents;
                    if (CommandEvents.TryGetValue(currentHotkey, out sendEvents))
                    {
                        SimpleData sd = new SimpleData(this);
                        sd.SourceObjectId = ObjectPrivate.ObjectId;
                        sd.AgentInfo = ScenePrivate.FindAgent(data.SessionId)?.AgentInfo;
                        sd.ObjectId = (sd.AgentInfo != null) ? sd.AgentInfo.ObjectId : ObjectId.Invalid;
                        SendToAll(sendEvents, sd);
                    }

                    HeldKeys[data.SessionId] |= currentHotkey;

                    // If holding more keys, look for the key combo
                    if (HeldKeys[data.SessionId] != currentHotkey
                        && CommandEvents.TryGetValue(HeldKeys[data.SessionId], out sendEvents))
                    {
                        SimpleData sd = new SimpleData(this);
                        sd.SourceObjectId = ObjectPrivate.ObjectId;
                        sd.AgentInfo = ScenePrivate.FindAgent(data.SessionId)?.AgentInfo;
                        sd.ObjectId = (sd.AgentInfo != null) ? sd.AgentInfo.ObjectId : ObjectId.Invalid;
                        SendToAll(sendEvents, sd);
                    }
                }
            }
            else if (data.Action == CommandAction.Released)
            {
                if ((currentHotkey & HeldKeys[data.SessionId]) != 0x0)
                {
                    string sendEvents;

                    // Check for key combo release
                    if (currentHotkey != HeldKeys[data.SessionId]
                        && CommandReleaseEvents.TryGetValue(HeldKeys[data.SessionId], out sendEvents))
                    {
                        SimpleData sd = new SimpleData(this);
                        sd.SourceObjectId = ObjectPrivate.ObjectId;
                        sd.AgentInfo = ScenePrivate.FindAgent(data.SessionId)?.AgentInfo;
                        sd.ObjectId = (sd.AgentInfo != null) ? sd.AgentInfo.ObjectId : ObjectId.Invalid;
                        SendToAll(sendEvents, sd);
                    }

                    HeldKeys[data.SessionId] &= ~currentHotkey;

                    if (CommandReleaseEvents.TryGetValue(currentHotkey, out sendEvents))
                    {
                        SimpleData sd = new SimpleData(this);
                        sd.SourceObjectId = ObjectPrivate.ObjectId;
                        sd.AgentInfo = ScenePrivate.FindAgent(data.SessionId)?.AgentInfo;
                        sd.ObjectId = (sd.AgentInfo != null) ? sd.AgentInfo.ObjectId : ObjectId.Invalid;
                        SendToAll(sendEvents, sd);
                    }
                }
            }
        }
    }
}