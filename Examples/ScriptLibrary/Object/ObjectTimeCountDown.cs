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
    [Tooltip("Manages a countdown timer and sends simple script events at specified times.")]
    [DisplayName("TimeCountdown")]
    public class ObjectTimeCountdown : ObjectBase
    {
        #region EditorProperties
        [Tooltip("The start time in the format SS, MM:SS or HH:MM:SS")]
        [DefaultValue("3:00")]
        [DisplayName("Start Time")]
        public readonly string StartingTime;

        [Tooltip("Start or Restart the timer. Can be a comma separated list of event names.")]
        [DefaultValue("on")]
        [DisplayName("-> Start/Resume")]
        public readonly string StartRestartEvent;

        [Tooltip("Stop the timer. Can be a comma separated list of event names.")]
        [DefaultValue("off")]
        [DisplayName("-> Stop")]
        public readonly string StopEvent;

        [Tooltip("The time to send command A.")]
        [DefaultValue("1:00")]
        [DisplayName("Time of Command A")]
        public readonly string CommandATime;

        [Tooltip("The events to send for the command")]
        [DefaultValue("one_minute_left")]
        [DisplayName("Command A ->")]
        public readonly string CommandAMessage;

        [Tooltip("The events to send when a timer counts down to zero and stops.")]
        [DefaultValue("done")]
        [DisplayName("Finished Command ->")]
        public readonly string FinishedCommandMessage;

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

        Action unsubscribes = null;

        SimpleData thisObjectData;
        int startingTimeInSeconds = 0;
        SortedDictionary<int, string> timerEventsBySeconds = null;

        private ICoroutine TimerCoroutine = null;

        protected override void SimpleInit()
        {
            thisObjectData = new SimpleData(this);
            thisObjectData.SourceObjectId = ObjectPrivate.ObjectId;
            thisObjectData.AgentInfo = null;
            thisObjectData.ObjectId = ObjectPrivate.ObjectId;

            timerEventsBySeconds = new SortedDictionary<int, string>();

            if (!TryParseTimePropertyToSeconds(StartingTime, out startingTimeInSeconds))
            {
                Log.Write(LogLevel.Error, __SimpleTag, "Could not convert start time '" + StartingTime + "' to seconds!");
                return;
            }

            ParseTimeCommand(CommandATime, CommandAMessage, "Command A");

            if (StartEnabled) Enable();

            SubscribeToAll(EnableEvent, (Data) => { Enable(); });
            SubscribeToAll(DisableEvent, (Data) => { Disable(); });
        }

        bool TryParseTimePropertyToSeconds(string timeStr, out int timeInSeconds)
        {
            timeInSeconds = 0;

            string[] parts = timeStr.Trim(new char[] { ' ', '.', ':' }).Split(':');

            if (parts.Length > 3 || parts.Length == 0)
                return false;

            if (parts.Length == 1)
            {
                return int.TryParse(parts[0], out timeInSeconds);
            }

            int scaleFactor = 1;
            for (int i = parts.Length - 1; i >= 0; i--)
            {
                int number;
                if (int.TryParse(parts[i], out number))
                {
                    if (number >= 0 && number < 60)
                        timeInSeconds += scaleFactor * number;
                    else
                        return false;
                }
                else
                    return false;

                scaleFactor *= 60;
            }

            return true;
        }

        void ParseTimeCommand(string commandTimeStr, string commandEvent, string commandName)
        {
            if (string.IsNullOrWhiteSpace(commandTimeStr) || string.IsNullOrWhiteSpace(commandEvent))
                return;

            int commandTime;
            if (TryParseTimePropertyToSeconds(commandTimeStr, out commandTime))
            {
                timerEventsBySeconds[commandTime] = commandEvent;
            }
            else
            {
                Log.Write(LogLevel.Warning, __SimpleTag, "Could not convert " + commandName + " time '" + commandTimeStr + "' to seconds. Message will not be sent!");
            }
        }

        void Enable()
        {
            if (unsubscribes != null)
                return;

            unsubscribes += SubscribeToAll(StartRestartEvent, (ScriptEventData data) =>
            {
                StopTimer();

                TimerCoroutine = StartCoroutine(StartTimer);
            });

            unsubscribes += SubscribeToAll(StopEvent, (ScriptEventData data) =>
            {
                StopTimer();
            });
        }

        void StopTimer()
        {
            if (TimerCoroutine != null)
            {
                TimerCoroutine.Abort();
                TimerCoroutine = null;
            }
        }

        void StartTimer()
        {
            int currentTime = startingTimeInSeconds;

            // Sort the time events from largest to smallest event time
            foreach (var timerEvent in timerEventsBySeconds.Reverse())
            {
                int intervalDuration = currentTime - timerEvent.Key;
                if (intervalDuration > 0)
                {
                    TimeSpan interval = TimeSpan.FromSeconds(intervalDuration);
                    Wait(interval);
                }

                if (intervalDuration >= 0)
                {
                    SendToAll(timerEvent.Value, thisObjectData);

                    currentTime = timerEvent.Key;
                }
            }

            if (currentTime > 0)
            {
                TimeSpan interval = TimeSpan.FromSeconds(currentTime);
                Wait(interval);
            }

            // Timer done
            SendToAll(FinishedCommandMessage, thisObjectData);
        }

        void Disable()
        {
            if (TimerCoroutine != null)
            {
                TimerCoroutine.Abort();
                TimerCoroutine = null;
            }
        }
    }
}