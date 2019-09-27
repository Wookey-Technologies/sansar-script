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

        [Tooltip("The text to display. Supports the following substitutions:"
            + "\n{Player.Name}: The name of the player seeing the prompt."
            + "\n{Object.Name}: The name of the object the hint script is on."
            + "\n{Data.Source}: The name of the object containing the script that sent the event.")]
        [DefaultValue("Hello World")]
        [DisplayName("Text")]
        public readonly string TextString;

        [Tooltip(@"Alternate text to display to users in VR.\nLeave empty to use the same text for all users.")]
        [DefaultValue("")]
        [DisplayName("VR Text")]
        public readonly string VRTextString;

        [Tooltip(@"The events that will show the text for a single user. Must be events that contain agent data, e.g events sent from the Interaction Library script. Can be a comma separated list of event names.")]
        [DefaultValue("on")]
        [DisplayName("-> Show")]
        public readonly string ShowEvent;

        [Tooltip(@"The events that will hide the text for a single user. Must be events that contain agent data, e.g events sent from the Interaction Library script. Can be a comma separated list of event names.")]
        [DefaultValue("off")]
        [DisplayName("-> Hide")]
        public readonly string HideEvent;

        [Tooltip("Hint Triggers\nSet to Trigger Volumes in the scene to show the hint to any user within the trigger volume."
            + "\n\nNote: Hints shown due to trigger volumes will ignore show/hide events, duration, and movement canceling. They will show while the user is in the volume and hide when they leave it.")]
        [DisplayName("Hint Triggers")]
        public List<RigidBodyComponent> Triggers;

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
        string m_vrTextString;
        bool m_vrStringEnabled = false;
        SimpleData CollisionSimpleData = null;

        protected override void SimpleInit()
        {
            CollisionSimpleData = new SimpleData(this);
            CollisionSimpleData.ObjectId = ObjectPrivate.ObjectId;
            CollisionSimpleData.SourceObjectId = ObjectPrivate.ObjectId;

            if (StartEnabled) Subscribe(null);

            if (TextString.Length > cMaxTextLength)
            {
                Log.Write(LogLevel.Warning, "HintText", "TextString too long: length is " + TextString.Length + " characters, truncating to " + cMaxTextLength);
                m_textString = TextString.Substring(0, cMaxTextLength);
            }
            else
            {
                m_textString = TextString;
            }

            m_vrStringEnabled = !string.IsNullOrWhiteSpace(VRTextString);
            if (VRTextString.Length > cMaxTextLength)
            {
                Log.Write(LogLevel.Warning, "HintText", "VRTextString too long: length is " + VRTextString.Length + " characters, truncating to " + cMaxTextLength);
                m_vrTextString = VRTextString.Substring(0, cMaxTextLength);
            }
            else
            {
                m_vrTextString = VRTextString;
            }

            foreach(var trigger in Triggers)
            {
                if (trigger != null
                    && trigger.IsValid
                    && trigger.IsTriggerVolume())
                {
                    trigger.Subscribe(CollisionEventType.Trigger, (data) => OnCollide(data,trigger));
                }
            }

            SubscribeToAll(EnableEvent, Subscribe);
            SubscribeToAll(DisableEvent, Unsubscribe);
        }


        private void OnCollide(CollisionData data, RigidBodyComponent trigger)
        {
            if (data.Phase == CollisionEventPhase.TriggerEnter)
            {
                AgentPrivate agent = ScenePrivate.FindAgent(data.HitComponentId.ObjectId);
                if (agent != null)
                {
                    try
                    {
                        CancelHideTimer(agent.AgentInfo.SessionId);

                        CollisionSimpleData.SourceObjectId = trigger.ComponentId.ObjectId;
                        if (isInVr(agent))
                        {
                            agent.Client.UI.HintText = GenerateHintText(m_vrTextString, agent.AgentInfo, CollisionSimpleData);
                        }
                        else
                        {
                            agent.Client.UI.HintText = GenerateHintText(m_textString, agent.AgentInfo, CollisionSimpleData);
                        }
                    }
                    catch (Exception) { }
                }
            }
            else if (data.Phase == CollisionEventPhase.TriggerExit)
            {
                AgentPrivate agent = ScenePrivate.FindAgent(data.HitComponentId.ObjectId);
                if (agent != null)
                {
                    HideForSessionId(agent.AgentInfo.SessionId);
                }
            }
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

        private string GenerateHintText(string hint, AgentInfo info, ISimpleData data)
        {
            if (hint.Contains("{"))
            {
                hint = hint.Replace("{Player.Name}", info.Name).Replace("{Object.Name}", ObjectPrivate.Name);

                ObjectPrivate source = ScenePrivate.FindObject(data.SourceObjectId);
                if (source != null)
                {
                    hint = hint.Replace("{Data.Source}", source.Name);
                }
            }

            return hint;
        }

        bool IsDifferentPos(in Sansar.Vector a, in Sansar.Vector b) { return a.X != b.X || a.Y != b.Y || a.Z != b.Z; }
        bool IsDifferentRot(in Sansar.Quaternion a, in Sansar.Quaternion b) { return a.X != b.X || a.Y != b.Y || a.Z != b.Z || a.W != b.W; }

        // This is hacky because of bugs in GetControlPoint APIs:
        // Gaze is completely broken, never reports values.
        // LeftTool/RightTool stay enabled once VR is entered, with the values remaining the old values outside VR
        bool isInVr(AgentPrivate agent)
        {
            if (!m_vrStringEnabled) return false;

            if (agent.GetControlPointEnabled(ControlPointType.GazeTarget)
                || agent.GetControlPointEnabled(ControlPointType.RightTool)
                || agent.GetControlPointEnabled(ControlPointType.LeftTool))
            {
                Sansar.Vector leftPos = agent.GetControlPointPosition(ControlPointType.LeftTool);
                Sansar.Quaternion rightRot = agent.GetControlPointOrientation(ControlPointType.RightTool);
                Wait(0.1);
                if (IsDifferentPos(leftPos, agent.GetControlPointPosition(ControlPointType.LeftTool))
                    || IsDifferentRot(rightRot, agent.GetControlPointOrientation(ControlPointType.RightTool)))
                {
                    return true;
                }
            }

            return false;
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

                    if (isInVr(agent))
                    {
                        agent.Client.UI.HintText = GenerateHintText(m_vrTextString, agent.AgentInfo, idata);
                    }
                    else
                    {
                        agent.Client.UI.HintText = GenerateHintText(m_textString, agent.AgentInfo, idata);
                    }

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