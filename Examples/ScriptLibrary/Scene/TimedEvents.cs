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
    [Tooltip("Sends simple script events at timed intervals.")]
    [DisplayName("Timed Events")]
    public class TimedEvents : LibraryBase
    {
        #region EditorProperties
        [Tooltip("The delay until the timer event, in Seconds ")]
        [DefaultValue(0.0)]
        [DisplayName("Initial Delay")]
        public readonly float InitialDelay;

        [Tooltip("The interval of events, in Seconds between events ")]
        [DefaultValue(1.0)]
        [DisplayName("Interval")]
        public readonly float Interval;

        [Tooltip("If true the timer will repeat every Interval seconds. If false the timer will fire only once, after Delay seconds.")]
        [DefaultValue(true)]
        [DisplayName("Repeats")]
        public readonly bool Repeats;

        [Tooltip(@"The events to send at the timer interval, alternating with ""On Timer 2nd ->"" if both are specified. Can be a comma separated list of event names.")]
        [DefaultValue("on")]
        [DisplayName("On Timer 1st ->")]
        public readonly string OnTimer;

        [Tooltip(@"The events to send at the timer interval, alternating with ""On Timer 1st ->"" if both are specified. Can be a comma separated list of event names.")]
        [DefaultValue("off")]
        [DisplayName("On Timer 2nd ->")]
        public readonly string OnTimerAlt;

        [Tooltip("The maximum amount of random time to add to the initial delay, in Seconds ")]
        [DefaultValue(0.0)]
        [DisplayName("Max Random Additional Delay")]
        public readonly float RandomInitialDelayMax;

        [Tooltip("The maximum amount of random time to add to the interval, in Seconds ")]
        [DefaultValue(0.0)]
        [DisplayName("Max Random Additional Interval")]
        public readonly float RandomIntervalMax;

        [Tooltip("Start this timer on these events. Can be a comma separated list of event names.")]
        [DefaultValue("timer_enable")]
        [DisplayName("-> Enable")]
        public readonly string EnableEvent;

        [Tooltip("Stop this timer on these events. Can be a comma separated list of event names.")]
        [DefaultValue("timer_disable")]
        [DisplayName("-> Disable")]
        public readonly string DisableEvent;

        [Tooltip(@"If StartEnabled is true then the script will respond to events when the scene is loaded
If StartEnabled is false then the script will not respond to events until an (-> Enable) event is received.")]
        [DefaultValue(true)]
        [DisplayName("StartEnabled")]
        public readonly bool StartEnabled;

        #endregion

        SimpleData thisObjectData;
        TimeSpan initialDelayTimeSpan = TimeSpan.Zero;
        TimeSpan intervalTimeSpan = TimeSpan.Zero;
        Random rnd;

        protected override void SimpleInit()
        {
            rnd = new Random();

            bool noTimerSet = (InitialDelay <= 0.0f) && (RandomInitialDelayMax <= 0.0f) && (Interval <= 0.0f) && (RandomIntervalMax <= 0.0f);

            if (noTimerSet)
            {
                Log.Write(LogLevel.Warning, "Delay and Interval are both zero in SimpleTimedEvents, no messages will be sent.");
                return;
            }

            thisObjectData = new SimpleData(this);
            thisObjectData.SourceObjectId = ObjectPrivate.ObjectId;
            thisObjectData.AgentInfo = null;
            thisObjectData.ObjectId = ObjectPrivate.ObjectId;

            if (StartEnabled) Enable();

            SubscribeToAll(EnableEvent, (Data) => { Enable(); });
            SubscribeToAll(DisableEvent, (Data) => { Disable(); });
        }

        private IEventSubscription TimerSub = null;
        private ICoroutine TimerCoroutine = null;

        void Enable()
        {
            if (TimerSub == null && TimerCoroutine == null)
            {
                if (InitialDelay > 0.0f || RandomInitialDelayMax > 0.0f)
                    initialDelayTimeSpan = TimeSpan.FromSeconds(InitialDelay + RandomZeroToOne() * RandomInitialDelayMax);

                intervalTimeSpan = TimeSpan.FromSeconds(Interval + RandomZeroToOne() * RandomIntervalMax);

                bool hasRegularInterval = (Interval > 0.0f) && (RandomIntervalMax == 0.0f);
                bool hasRandomInterval = (RandomIntervalMax > 0.0f);

                if (hasRegularInterval && Repeats)
                {
                    bool altMessageExists = !string.IsNullOrWhiteSpace(OnTimerAlt);
                    bool sendAlt = false;

                    TimerSub = Sansar.Script.Timer.Create(initialDelayTimeSpan, intervalTimeSpan, () =>
                    {
                        if (sendAlt)
                        {
                            SendToAll(OnTimerAlt, thisObjectData);
                            sendAlt = false;
                        }
                        else
                        {
                            SendToAll(OnTimer, thisObjectData);
                            sendAlt = altMessageExists;
                        }
                    });
                }
                else if (hasRandomInterval && Repeats)
                {
                    TimerCoroutine = StartCoroutine(RepeatingRandomTimer);
                }
                else
                {
                    TimerCoroutine = StartCoroutine(NonRepeatingTimer);
                }
            }
        }

        private float RandomZeroToOne()
        {
            return (float)rnd.NextDouble();
        }

        void RepeatingRandomTimer()
        {
            Wait(initialDelayTimeSpan);
            SendToAll(OnTimer, thisObjectData);

            bool altMessageExists = !string.IsNullOrWhiteSpace(OnTimerAlt);
            bool sendAlt = altMessageExists;

            while (true)
            {
                Wait(intervalTimeSpan);

                if (sendAlt)
                {
                    SendToAll(OnTimerAlt, thisObjectData);
                    sendAlt = false;
                }
                else
                {
                    SendToAll(OnTimer, thisObjectData);
                    sendAlt = altMessageExists;
                }

                // Pick a new random time interval
                intervalTimeSpan = TimeSpan.FromSeconds(Interval + RandomZeroToOne() * RandomIntervalMax);
            }
        }

        void NonRepeatingTimer()
        {
            Wait(initialDelayTimeSpan);
            SendToAll(OnTimer, thisObjectData);

            Wait(intervalTimeSpan);
            SendToAll(OnTimerAlt, thisObjectData);

            TimerCoroutine = null;
        }

        void Disable()
        {
            if (TimerSub != null)
            {
                TimerSub.Unsubscribe();
                TimerSub = null;
            }

            if (TimerCoroutine != null)
            {
                TimerCoroutine.Abort();
                TimerCoroutine = null;
            }
        }
    }
}