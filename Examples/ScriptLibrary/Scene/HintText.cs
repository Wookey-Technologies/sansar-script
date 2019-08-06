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
using System.Collections.Generic;

namespace ScriptLibrary
{
    [Tooltip("Adds some hint text to the screen.")]
    [DisplayName("Hint Text")]
    public class HintText : LibraryBase
    {
        #region EditorProperties

        [Tooltip(@"The text to display.")]
        [DefaultValue("Hello World")]
        [DisplayName("Text")]
        public readonly string TextString;

        [Tooltip(@"The events that will show the text for a single user. Must be events that contain agent data, e.g events sent from the Interaction Library script. Can be a comma separated list of event names.")]
        [DefaultValue("on")]
        [DisplayName("-> Show")]
        public readonly string ShowEvent;

        [Tooltip(@"The events that will hide the text for a single user. Must be events that contain agent data, e.g events sent from the Interaction Library script. Can be a comma separated list of event names.")]
        [DefaultValue("off")]
        [DisplayName("-> Hide")]
        public readonly string HideEvent;

        [Tooltip(@"Length of time in seconds that the text should remain on screen. If zero, the text will remain until a Hide event is received")]
        [DefaultValue(0.0)]
        [DisplayName("Show Duration (seconds)")]
        public readonly double ShowDuration;

        [Tooltip(@"If true, remove the text when the agent who triggered it moves.")]
        [DefaultValue(false)]
        [DisplayName("Cancel if agent moves.")]
        public readonly bool CancelOnMove;

        [Tooltip("Enable responding to events for this script")]
        [DefaultValue("hinttext_enable")]
        [DisplayName("-> Enable")]
        public readonly string EnableEvent;

        [Tooltip("Disable responding to events for this script")]
        [DefaultValue("hinttext_disable")]
        [DisplayName("-> Disable")]
        public readonly string DisableEvent;

        [Tooltip(@"If StartEnabled is false then this script will not respond to event to show or hide the hint text until an -> Enable event is received.")]
        [DefaultValue(true)]
        [DisplayName("Start Enabled")]
        public readonly bool StartEnabled = true;
        #endregion

        Action Unsubscribes = null;
        Dictionary<SessionId, IEventSubscription> agentTimers = new Dictionary<SessionId, IEventSubscription>();

        const float cAgentMoveToleranceSqr = 0.05f;
        const float cAgentMoveCheckTimestep = 0.1f;

        const int cMaxTextLength = 80;

        string m_textString;

        protected override void SimpleInit()
        {
            if (StartEnabled) Subscribe(null);

            m_textString = TextString;

            if (TextString.Length > cMaxTextLength)
            {
                Log.Write(LogLevel.Warning, "HintText", "TextString too long: length is " + TextString.Length + " characters, truncating to " + cMaxTextLength);
                m_textString = TextString.Substring(0, cMaxTextLength);
            }

            SubscribeToAll(EnableEvent, Subscribe);
            SubscribeToAll(DisableEvent, Unsubscribe);
        }

        private void Subscribe(ScriptEventData sed)
        {
            if (Unsubscribes == null)
            {
                Unsubscribes = SubscribeToAll(ShowEvent, Show);
                Unsubscribes += SubscribeToAll(HideEvent, Hide);
            }
        }

        private void Unsubscribe(ScriptEventData sed)
        {
            if (Unsubscribes != null)
            {
                Unsubscribes();
                Unsubscribes = null;
            }
        }

        private void Show(ScriptEventData sed)
        {
            ISimpleData idata = sed.Data.AsInterface<ISimpleData>();
            if (idata != null && idata.AgentInfo != null)
            {
                AgentPrivate agent = ScenePrivate.FindAgent(idata.AgentInfo.SessionId);
                
                ObjectPrivate agentObject = ScenePrivate.FindObject(idata.AgentInfo.ObjectId);

                if (agent == null || agentObject == null || !agent.IsValid)
                {
                    return;
                }

                try
                {
                    SessionId sessionId = agent.AgentInfo.SessionId;
                    CancelHideTimer(sessionId);
                    agent.Client.UI.HintText = m_textString;

                    if (ShowDuration > 0.01f)
                    {
                        agentTimers.Add(sessionId, Timer.Create(ShowDuration, () => { HideForSessionId(sessionId); }));
                    }

                    if (CancelOnMove)
                    {
                        StartCoroutine(() => CancelOnMoveCoroutine(agentObject, agent, sessionId));
                    }
                }
                catch (ArgumentException)
                {
                    Log.Write(LogLevel.Error, "Error setting text, bad string?");
                }
                catch
                {
                    Log.Write(LogLevel.Warning, "Error setting text, agent might have left");
                }
                
            }
            else
            {
                Log.Write(LogLevel.Error, "Could not obtain agent data from text box show event.");
            }
        }

        private void Hide(ScriptEventData sed)
        {
            ISimpleData idata = sed.Data.AsInterface<ISimpleData>();
            if (idata != null && idata.AgentInfo != null)
            {
                SessionId sessionId = idata.AgentInfo.SessionId;
                CancelHideTimer(sessionId);
                HideForSessionId(sessionId);
            }
        }

        private void HideForSessionId(SessionId sessionId)
        {
            AgentPrivate agent = ScenePrivate.FindAgent(sessionId);
            if (agent == null || !agent.IsValid)
            {
                return;
            }
            try
            {
                agent.Client.UI.HintText = null;
            }
            catch
            {
                Log.Write(LogLevel.Warning, "Error hiding text box, agent might have left");
            }
        }

        private void CancelHideTimer(SessionId sessionId)
        {
            IEventSubscription eventSubscription;
            if (agentTimers.TryGetValue(sessionId, out eventSubscription))
            {
                eventSubscription.Unsubscribe();
                agentTimers.Remove(sessionId);
            }
        }

        private void CancelOnMoveCoroutine(ObjectPrivate agentObject, AgentPrivate agent, SessionId sessionId)
        {
            Vector startPosition;

            try
            {
                startPosition = agentObject.Position;
            }
            catch
            {
                Log.Write(LogLevel.Warning, "Error getting agent position, agent might have left");
                return;
            }

            float timeCheck = 0.0f;

            bool monitoring = true;
            while (monitoring)
            {
                Vector agentPosition;
                string hintText;
                try
                {
                    agentPosition = agentObject.Position;
                    hintText = agent.Client.UI.HintText;
                }
                catch
                {
                    Log.Write(LogLevel.Warning, "Error getting agent position, agent might have left");
                    return;
                }
                if (string.IsNullOrEmpty(hintText))
                {
                    return;
                }
                if ((agentPosition - startPosition).LengthSquared() > cAgentMoveToleranceSqr)
                {
                    try
                    {
                        agent.Client.UI.HintText = null;
                        
                    }
                    catch
                    {
                        Log.Write(LogLevel.Warning, "Error clearing text, agent might have left");
                    }
                    break;
                }

                Wait(cAgentMoveCheckTimestep);

                timeCheck += cAgentMoveCheckTimestep;

                monitoring = agent != null && agent.IsValid;
            }
            CancelHideTimer(sessionId);
        }

    }
}