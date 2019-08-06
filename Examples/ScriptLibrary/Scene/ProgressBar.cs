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
using System.Linq;

namespace ScriptLibrary
{
    [Tooltip("Adds a timed progress bar to the screen, and sends an event when completed.")]
    [DisplayName("Progress Bar")]
    public class ProgressBar : LibraryBase
    {
        #region EditorProperties

        [Tooltip(@"The text label to add to the progress bar.")]
        [DefaultValue("Progress...")]
        [DisplayName("Label")]
        public readonly string LabelString;

        [Tooltip("The color of the progress bar.")]
        [DefaultValue(1,0,0,1)]
        [DisplayName("Color")]
        public readonly Sansar.Color Color;

        [Tooltip(@"The time in seconds the progress bar will take to complete")]
        [DefaultValue(2.0f)]
        [DisplayName("Duration")]
        public readonly float Duration;

        [Tooltip(@"Start the progress bar for a single user. Must be events that contain agent data, e.g events sent from the Interaction Library script. 
The event will be ignored the bar has already been started by this script and is still in progress. Any bars being shown to the user by other scripts will be canceled. Can be a comma separated list of event names.")]
        [DefaultValue("on")]
        [DisplayName("-> Start")]
        public readonly string StartEvent;

        [Tooltip(@"Cancel the progress bar, if it was started by this script. Must be events that contain agent data, e.g events sent from the Interaction Library script. Can be a comma separated list of event names.")]
        [DefaultValue("off")]
        [DisplayName("-> Cancel")]
        public readonly string CancelEvent;

        [Tooltip(@"The events to send when the progress bar completes successfully. Can be a comma separated list of event names.")]
        [DefaultValue("")]
        [DisplayName("On Completion ->")]
        public readonly string OnCompletion;

        [Tooltip(@"The events to send when the progress bar is canceled. Can be a comma separated list of event names.")]
        [DefaultValue("")]
        [DisplayName("On Cancel ->")]
        public readonly string OnCancel;

        [Tooltip(@"If true, cancel the progress bar when the agent who triggered it moves.")]
        [DefaultValue(true)]
        [DisplayName("Cancel if agent moves.")]
        public readonly bool CancelOnMove;

        [Tooltip("Enable responding to events for this script")]
        [DefaultValue("progressbar_enable")]
        [DisplayName("-> Enable")]
        public readonly string EnableEvent;

        [Tooltip("Disable responding to events for this script")]
        [DefaultValue("progressbar_disable")]
        [DisplayName("-> Disable")]
        public readonly string DisableEvent;

        [Tooltip(@"If StartEnabled is false then this script will not respond to event to start the progress bar until an -> Enable event is received.")]
        [DefaultValue(true)]
        [DisplayName("Start Enabled")]
        public readonly bool StartEnabled = true;
        #endregion

        Action Unsubscribes = null;
        const float cAgentMoveToleranceSqr = 0.05f;
        const float cAgentMoveCheckTimestep = 0.1f;

        Dictionary<Guid, UIProgressBar> _agentProgressBars = new Dictionary<Guid, UIProgressBar>();

        protected override void SimpleInit()
        {
            if (StartEnabled) Subscribe(null);

            SubscribeToAll(EnableEvent, Subscribe);
            SubscribeToAll(DisableEvent, Unsubscribe);
        }

        private void Subscribe(ScriptEventData sed)
        {
            if (Unsubscribes == null)
            {
                Unsubscribes = SubscribeToAll(StartEvent, Start);
                Unsubscribes += SubscribeToAll(CancelEvent, Cancel);
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

        private void Start(ScriptEventData sed)
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
                    Guid agentId = agent.AgentInfo.AvatarUuid;

                    UIProgressBar thisBar;
                    if (_agentProgressBars.TryGetValue(agentId, out thisBar) && thisBar != null && thisBar.IsValid)
                    {
                        // if this script is already showing the progress bar to this agent, ignore the start event.
                        return;
                    }

                    // cancel any progress bar that has been started by another script
                    foreach (UIProgressBar otherBar in agent.Client.UI.GetProgressBars())
                    {
                        if (otherBar.IsValid)
                        {
                            otherBar.Cancel();
                        }
                    }
                  
                    UIProgressBar bar = agent.Client.UI.AddProgressBar();
                    bar.Start(LabelString, Duration, Color, (OperationCompleteEvent completeEvent) => {
                        OnBarComplete(agent.AgentInfo, completeEvent);
                    });

                    _agentProgressBars[agentId] = bar;

                    if (CancelOnMove)
                    {
                        CancelOnMoveCoroutine(agentId, agentObject, bar);
                    }
                    else
                    {
                        RemoveFromDictionaryCoroutine(agentId, bar);
                    }
                }
                catch { }
            }
            else
            {
                Log.Write(LogLevel.Error, "Could not obtain agent data from progress bar start event.");
            }
        }

        private void Cancel(ScriptEventData sed)
        {
            ISimpleData idata = sed.Data.AsInterface<ISimpleData>();
            if (idata != null && idata.AgentInfo != null)
            {
                AgentPrivate agent = ScenePrivate.FindAgent(idata.AgentInfo.SessionId);
                if (agent == null || !agent.IsValid)
                {
                    return;
                }
                
                try
                {
                    UIProgressBar progressBar;
                    if (_agentProgressBars.TryGetValue(agent.AgentInfo.AvatarUuid, out progressBar))
                    {
                        CancelBarForAgent(agent.AgentInfo.AvatarUuid, progressBar);
                    }
                }
                catch { }
            }
        }

        private void OnBarComplete(AgentInfo agentInfo, OperationCompleteEvent completeEvent)
        {
            if (completeEvent.Success)
            {
                SendToAll(OnCompletion, GetEventData(agentInfo));
            }
            else
            {
                SendToAll(OnCancel, GetEventData(agentInfo));
            }
        }

        private SimpleData GetEventData(AgentInfo agentInfo)
        {
            SimpleData eventData = new SimpleData(this);
            eventData.SourceObjectId = ObjectPrivate.ObjectId;
            eventData.AgentInfo = agentInfo;
            eventData.ObjectId = ObjectPrivate.ObjectId;
            return eventData;
        }

        private void CancelOnMoveCoroutine(Guid agentId, ObjectPrivate agentObject, UIProgressBar progressBar)
        {
            Vector startPosition;

            try
            {
                startPosition = agentObject.Position;
            }
            catch
            {
                return;
            }

            float timeCheck = 0.0f;

            bool monitoring = true;
            while (monitoring)
            {
                Vector agentPosition;
                try
                {
                    agentPosition = agentObject.Position;
                }
                catch
                {
                    return;
                }
                if ((agentPosition - startPosition).LengthSquared() > cAgentMoveToleranceSqr)
                {
                    CancelBarForAgent(agentId, progressBar);
                    return;
                }
                
                Wait(cAgentMoveCheckTimestep);

                timeCheck += cAgentMoveCheckTimestep;
                monitoring = timeCheck < Duration;
                monitoring &= progressBar != null && progressBar.IsValid;
            }

        }

        private void RemoveFromDictionaryCoroutine(Guid agentId, UIProgressBar bar)
        {
            Wait(Duration);
            if (_agentProgressBars.ContainsKey(agentId))
            {
                _agentProgressBars.Remove(agentId);
            }
        }

        private void CancelBarForAgent(Guid agentId, UIProgressBar bar)
        {
            try
            {
                if (bar.IsValid)
                {
                    bar.Cancel();
                }
            }
            catch
            { }
            
            if (_agentProgressBars.ContainsKey(agentId))
            {
                _agentProgressBars.Remove(agentId);
            }
        }
    }
}