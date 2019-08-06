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
using System.Linq;

namespace ScriptLibrary
{
    [Tooltip("Sends simple script events in response to chat commands.")]
    [DisplayName("Chat Command")]
    public class ChatCommand : LibraryBase
    {
        #region EditorProperties
        [Tooltip("The command")]
        [DefaultValue("/on | Turn it on")]
        [DisplayName("Command A")]
        public readonly string CommandAText;

        [Tooltip("The events to send for the command")]
        [DefaultValue("on")]
        [DisplayName("On Command A ->")]
        public readonly string CommandAMessage;

        [Tooltip("The command")]
        [DefaultValue("/off | Turn it off")]
        [DisplayName("Command B")]
        public readonly string CommandBText;

        [Tooltip("The events to send for the command")]
        [DefaultValue("off")]
        [DisplayName("On Command B ->")]
        public readonly string CommandBMessage;

        [Tooltip("The command")]
        [DefaultValue("")]
        [DisplayName("Command C")]
        public readonly string CommandCText;

        [Tooltip("The events to send for the command")]
        [DefaultValue("")]
        [DisplayName("On Command C ->")]
        public readonly string CommandCMessage;

        [Tooltip("Commands restricted to the experience creator only")]
        [DefaultValue(true)]
        [DisplayName("Creator Only")]
        public readonly bool CreatorOnly = true;

        [Tooltip("Enable responding to events for this script")]
        [DefaultValue("command_enable")]
        [DisplayName("-> Enable")]
        public readonly string EnableEvent;

        [Tooltip("Disable responding to events for this script")]
        [DefaultValue("command_disable")]
        [DisplayName("-> Disable")]
        public readonly string DisableEvent;

        [Tooltip(@"If StartEnabled is true then the script will respond to interactions when the scene is loaded
If StartEnabled is false then the script will not respond to interactions until an (-> Enable) event is received.")]
        [DefaultValue(true)]
        [DisplayName("Start Enabled")]
        public readonly bool StartEnabled = true;
        #endregion


        bool enabled = false;
        System.Collections.Generic.Dictionary<string, string> commands = null;
        System.Collections.Generic.Dictionary<string, string> helpMessages = null;

        protected override void SimpleInit()
        {
            commands = new System.Collections.Generic.Dictionary<string, string>();
            helpMessages = new System.Collections.Generic.Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(CommandAText) && !string.IsNullOrWhiteSpace(CommandAMessage))
            {
                string command = ExtractCommand(CommandAText);
                string helpMessage = ExtractHelpMessage(CommandAText);

                commands[command] = CommandAMessage;
                helpMessages[command] = helpMessage;
            }

            if (!string.IsNullOrWhiteSpace(CommandBText) && !string.IsNullOrWhiteSpace(CommandBMessage))
            {
                string command = ExtractCommand(CommandBText);
                string helpMessage = ExtractHelpMessage(CommandBText);

                commands[command] = CommandBMessage;
                helpMessages[command] = helpMessage;
            }

            if (!string.IsNullOrWhiteSpace(CommandCText) && !string.IsNullOrWhiteSpace(CommandCMessage))
            {
                string command = ExtractCommand(CommandCText);
                string helpMessage = ExtractHelpMessage(CommandCText);

                commands[command] = CommandCMessage;
                helpMessages[command] = helpMessage;
            }

            if (commands.Keys.Count > 0)
            {
                ScenePrivate.Chat.Subscribe(Sansar.Simulation.Chat.DefaultChannel, OnChat);
            }

            enabled = StartEnabled;

            SubscribeToAll(EnableEvent, (ScriptEventData data) => { enabled = true; });
            SubscribeToAll(DisableEvent, (ScriptEventData data) => { enabled = false; });
        }

        string ExtractCommand(string commandText)
        {
            string command = commandText;
            int helpStart = commandText.IndexOf("|");
            if (helpStart > 0)
            {
                command = commandText.Substring(0, helpStart).Trim();
            }
            return command;
        }

        string ExtractHelpMessage(string commandText)
        {
            string helpMessage = "";
            int helpStart = commandText.IndexOf("|") + 1;
            if (helpStart > 0)
            {
                helpMessage = commandText.Substring(helpStart).Trim();
            }
            return helpMessage;
        }

        bool IsAccessAllowed(AgentPrivate agent)
        {
            bool accessAllowed = true;

            if (CreatorOnly)
            {
                accessAllowed = (agent != null) && (agent.AgentInfo.AvatarUuid == ScenePrivate.SceneInfo.AvatarUuid);
            }

            return accessAllowed;
        }

        bool IsHelpCommand(string command)
        {
            return (command == "/help");
        }

        string GetHelpText()
        {
            string helpText;

            if (CreatorOnly)
                helpText = "Creator-only commands:";
            else
                helpText = "Available commands:";

            foreach (var kvp in helpMessages)
            {
                helpText += "\n" + kvp.Key;

                if (!string.IsNullOrWhiteSpace(kvp.Value))
                {
                    helpText += " : " + kvp.Value;
                }
            }

            return helpText;
        }

        void OnChat(ChatData data)
        {
            if (!enabled)
            {
                return;
            }

            // If this chat message is a relevant command
            if (commands.ContainsKey(data.Message))
            {
                var agent = ScenePrivate.FindAgent(data.SourceId);

                if (IsAccessAllowed(agent))
                {
                    SimpleData sd = new SimpleData(this);
                    sd.SourceObjectId = ObjectPrivate.ObjectId;
                    sd.AgentInfo = agent?.AgentInfo;
                    if (sd.AgentInfo != null)
                        sd.ObjectId = sd.AgentInfo.ObjectId;

                    SendToAll(commands[data.Message], sd);
                }
            }
            // Else if it is the help command (and help not overridden by creator)
            else if (IsHelpCommand(data.Message) && (commands.Keys.Count > 0))
            {
                var agent = ScenePrivate.FindAgent(data.SourceId);

                if (IsAccessAllowed(agent) && (agent != null))
                {
                    agent.SendChat(GetHelpText());
                }
            }
        }
    }
}