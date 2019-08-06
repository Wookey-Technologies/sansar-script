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
    [Tooltip("Sends chat messages in response to simple script events.")]
    [DisplayName(nameof(Chat))]
    public class Chat : LibraryBase
    {
        #region EditorProperties
        [Tooltip("The events to listen for")]
        [DefaultValue("on")]
        [DisplayName("-> Message A")]
        public readonly string MessageA;

        [Tooltip("The message to send")]
        [DefaultValue("It is on, {NAME}!")]
        [DisplayName("Message A Chat")]
        public readonly string MessageAText;

        [Tooltip("The events to listen for")]
        [DefaultValue("off")]
        [DisplayName("-> Message B")]
        public readonly string MessageB;

        [Tooltip("The message to send")]
        [DefaultValue("It is off!")]
        [DisplayName("Message B Chat")]
        public readonly string MessageBText;

        [Tooltip("The events to listen for")]
        [DefaultValue("")]
        [DisplayName("-> Message C")]
        public readonly string MessageC;

        [Tooltip("The message to send")]
        [DefaultValue("")]
        [DisplayName("Message C Chat")]
        public readonly string MessageCText;

        [Tooltip("Send messages privately")]
        [DefaultValue(false)]
        [DisplayName("Private Message")]
        public readonly bool DirectMessage = false;

        [Tooltip("Enable responding to events for this script")]
        [DefaultValue("chat_enable")]
        [DisplayName("-> Enable")]
        public readonly string EnableEvent;

        [Tooltip("Disable responding to events for this script")]
        [DefaultValue("chat_disable")]
        [DisplayName("-> Disable")]
        public readonly string DisableEvent;

        [Tooltip(@"If StartEnabled is true then the script will respond to interactions when the scene is loaded
If StartEnabled is false then the script will not respond to interactions until an (-> Enable) event is received.")]
        [DefaultValue(true)]
        [DisplayName("Start Enabled")]
        public readonly bool StartEnabled = true;
        #endregion

        System.Collections.Generic.Dictionary<string, string> messages = null;

        Action unsubscribes = null;

        protected override void SimpleInit()
        {
            messages = new System.Collections.Generic.Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(MessageA) && !string.IsNullOrWhiteSpace(MessageAText))
            {
                messages[MessageA] = MessageAText;
            }

            if (!string.IsNullOrWhiteSpace(MessageB) && !string.IsNullOrWhiteSpace(MessageBText))
            {
                messages[MessageB] = MessageBText;
            }

            if (!string.IsNullOrWhiteSpace(MessageC) && !string.IsNullOrWhiteSpace(MessageCText))
            {
                messages[MessageC] = MessageCText;
            }

            if (messages.Keys.Count > 0)
            {
                if (StartEnabled) SubscribeToMessages();

                SubscribeToAll(EnableEvent, (ScriptEventData data) => { SubscribeToMessages(); });
                SubscribeToAll(DisableEvent, (ScriptEventData data) => { UnsubscribeFromMessages(); });
            }
        }

        string FixupChatMessage(string inputText, ISimpleData simpleData)
        {
            if ((simpleData != null) && (simpleData.AgentInfo != null))
            {
                return inputText.Replace("{NAME}", simpleData.AgentInfo.Name);
            }
            else
            {
                return inputText;
            }
        }

        void SubscribeToMessages()
        {
            // Already subscribed
            if (unsubscribes != null)
            {
                return;
            }

            foreach (var kvp in messages)
            {
                unsubscribes += SubscribeToAll(kvp.Key, (ScriptEventData subData) =>
                {
                    ISimpleData simpleData = subData.Data?.AsInterface<ISimpleData>();

                    string chatText = FixupChatMessage(kvp.Value, simpleData);

                    if (DirectMessage)
                    {
                        try
                        {
                            if (simpleData != null)
                            {
                                var agent = ScenePrivate.FindAgent(simpleData.AgentInfo.SessionId);

                                if (agent != null && agent.IsValid)
                                {
                                    agent.SendChat(chatText);
                                }
                            }
                        }
                        catch (Exception)
                        {
                        // Agent left.
                        }
                    }
                    else
                    {
                        ScenePrivate.Chat.MessageAllUsers(chatText);
                    }
                });
            }
        }

        void UnsubscribeFromMessages()
        {
            if (unsubscribes != null)
            {
                unsubscribes();
                unsubscribes = null;
            }
        }
    }
}