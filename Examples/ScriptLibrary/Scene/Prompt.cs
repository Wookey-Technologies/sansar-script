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
    [Tooltip("Displays a popup dialog in response to simple script events.")]
    [DisplayName(nameof(Prompt))]
    public class Prompt : LibraryBase // Requires Client which requires AgentPrivate which requires ScenePrivate
    {
        #region EditorProperties
        [Tooltip("Show the prompt on these events. Can be a comma separated list of event names.")]
        [DefaultValue("on")]
        [DisplayName("-> Show Prompt")]
        public readonly string ShowPromptEvent;

        [Tooltip("The message to show as the body of the prompt dialog.")]
        [DefaultValue("Turn it on?")]
        [DisplayName("Prompt Message")]
        public readonly string MessagePrompt;

        [Tooltip("The text to show in the right side button. Set to an empty string to hide this button.")]
        [DefaultValue("Yes please!")]
        [DisplayName("Right Button")]
        public readonly string RightButtonText;

        [Tooltip("The events to send when the right side button is chosen")]
        [DefaultValue("on")]
        [DisplayName("Right Button Click ->")]
        public readonly string RightButtonClickMessage;

        [Tooltip("The text to show in the left side button. Set to an empty string to hide this button.")]
        [DefaultValue("No thanks")]
        [DisplayName("Left Button")]
        public readonly string LeftButtonText;

        [Tooltip("The events to when the left side button is chosen")]
        [DefaultValue("")]
        [DisplayName("Left Button Click ->")]
        public readonly string LeftButtonClickMessage;

        [Tooltip("Enable responding to events for this script")]
        [DefaultValue("prompt_enable")]
        [DisplayName("-> Enable")]
        public readonly string EnableEvent;

        [Tooltip("Disable responding to events for this script")]
        [DefaultValue("prompt_disable")]
        [DisplayName("-> Disable")]
        public readonly string DisableEvent;

        [Tooltip(@"If StartEnabled is true then the script will respond to interactions when the scene is loaded
If StartEnabled is false then the script will not respond to interactions until an (-> Enable) event is received.")]
        [DefaultValue(true)]
        [DisplayName("Start Enabled")]
        public readonly bool StartEnabled = true;
        #endregion

        Action unsubscribes = null;

        protected override void SimpleInit()
        {
            if (string.IsNullOrWhiteSpace(LeftButtonText) && string.IsNullOrWhiteSpace(RightButtonText))
            {
                Log.Write(LogLevel.Error, __SimpleTag, "Must provide at least 1 button for SimplePrompt");
                return;
            }

            if (StartEnabled) Subscribe(null);

            SubscribeToAll(EnableEvent, Subscribe);
            SubscribeToAll(DisableEvent, (ScriptEventData data) =>
            {
                if (unsubscribes != null)
                {
                    unsubscribes();
                    unsubscribes = null;
                }
            });
        }

        private void Subscribe(ScriptEventData data)
        {
            unsubscribes = SubscribeToAll(ShowPromptEvent, (ScriptEventData subdata) =>
            {
                try
                {
                    ISimpleData simpledata = subdata.Data?.AsInterface<ISimpleData>();
                    if (simpledata != null)
                    {
                        AgentPrivate agent = ScenePrivate.FindAgent(simpledata.AgentInfo.SessionId);

                        if (agent != null && agent.IsValid)
                        {
                            agent.Client.UI.ModalDialog.Show(MessagePrompt, LeftButtonText, RightButtonText, (opc) =>
                            {
                                OnDialogResponse(agent.Client.UI.ModalDialog.Response, agent.AgentInfo.SessionId);
                            });
                        }
                    }
                }
                catch (Exception)
                {
                // Agent left.
            }
            });
        }

        private void OnDialogResponse(string response, SessionId agentId)
        {
            SimpleData sd = new SimpleData(this);
            sd.SourceObjectId = ObjectPrivate.ObjectId;
            sd.AgentInfo = ScenePrivate.FindAgent(agentId)?.AgentInfo;
            sd.ObjectId = sd.AgentInfo != null ? sd.AgentInfo.ObjectId : ObjectId.Invalid;

            if ((response == LeftButtonText) && !string.IsNullOrWhiteSpace(LeftButtonClickMessage))
            {
                SendToAll(LeftButtonClickMessage, sd);
            }

            if ((response == RightButtonText) && !string.IsNullOrWhiteSpace(RightButtonClickMessage))
            {
                SendToAll(RightButtonClickMessage, sd);
            }
        }
    }
}