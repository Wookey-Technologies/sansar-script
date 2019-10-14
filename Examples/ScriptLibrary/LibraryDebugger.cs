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

namespace ScriptLibrary
{
    [Tooltip("Add this anywhere to see all simple script activity in the debug console. (CTRL+D)")]
    [DisplayName(nameof(Debugger))]

    // This class is used to debug simple scripts
    public class Debugger : LibraryBase, IDebug
    {
        #region EditorProperties
        [Tooltip("If set all library scripts in the scene will log the events they receive and send to the script debug console.")]
        [DefaultValue(true)]
        [DisplayName("Debug Events")]
        public readonly bool _DebugEvents = true;

        [Tooltip("If set will regularly report memory use to the script debug console.\nWhenever the memory used changes by more than 10% since the last report it will be logged.")]
        [DefaultValue(false)]
        [DisplayName("Debug Memory")]
        public readonly bool DebugMemoryEnabled;

        [Tooltip("-> Memory Report\nSends a report to the script debug console with current memory usage when the event is received.")]
        [DefaultValue("")]
        [DisplayName("-> Memory Report")]
        public readonly string MemoryReportEvent;

        [Tooltip("-> Subscription Report\nSends a report to the script debug console with information about script event subscriptions."
            + "\nNote: may not track subscriptions from older library scripts, or non-library scripts.")]
        [DefaultValue("")]
        [DisplayName("-> Subscription Report")]
        public readonly string SubscriptionReportEvent;
        #endregion

        public bool DebugSimple { get { return _DebugEvents; } }

        protected override Context ReflectiveContexts => Context.ScenePrivate | Context.ScenePublic;
        protected override string ReflectiveName => "Simple.Debugger";
        bool TrackSubscriptionsEnabled = false;

        protected override void SimpleInit()
        {
            TrackSubscriptionsEnabled = !string.IsNullOrWhiteSpace(SubscriptionReportEvent);

            if (DebugMemoryEnabled) StartCoroutine(DebugMemory);

            SubscribeToAll(MemoryReportEvent, MemoryReport);
            SubscribeToAll(SubscriptionReportEvent, SubscribeReport);
        }

        LogLevel GetLogLevel()
        {
            if (Memory.UsedBytes > Memory.PolicyWarning) return LogLevel.Warning;
            else if (Memory.UsedBytes > Memory.PolicyLimit) return LogLevel.Error;
            return LogLevel.Info;
        }

        void DebugMemory()
        {
            uint usedBytes = Memory.UsedBytes;

            while (true)
            {
                if (usedBytes == 0)
                {
                    usedBytes = Memory.UsedBytes;
                    if (usedBytes != 0)
                    {
                        Log.Write(GetLogLevel(), "MemoryDebug", $"Current memory used is {(int)(Memory.UsedBytes / 1024)}kb, peak memory used is {(int)(Memory.PeakUsedBytes / 1024)}kb");
                    }
                }
                else if (Math.Abs(Memory.UsedBytes - usedBytes) > (usedBytes * 0.1))
                {
                    Log.Write(GetLogLevel(), "MemoryDebug", $"Current memory used is {(int)(Memory.UsedBytes / 1024)}kb, peak memory used is {(int)(Memory.PeakUsedBytes / 1024)}kb");
                    usedBytes = Memory.UsedBytes;
                    Wait(4); // Wait extra time after logging to avoid flooding on rapid changes.
                }
                Wait(1);
            }
        }

        void MemoryReport(ScriptEventData data)
        {
            Log.Write(GetLogLevel(), "MemoryReport", $"Current memory used is {(int)(Memory.UsedBytes / 1024)}kb, peak memory used is {(int)(Memory.PeakUsedBytes / 1024)}kb");
        }

        public bool TrackingSubscriptions {  get { return TrackSubscriptionsEnabled; } }

        Dictionary<string, int> Subscriptions = new Dictionary<string, int>();
        public void TrackSubscribe(string name, int count = 1)
        {
            if (!TrackSubscriptionsEnabled) return;

            if (Subscriptions.ContainsKey(name))
            {
                count += Subscriptions[name];
            }

            if (count > 0)
            {
                Subscriptions[name] = count;
            }
            else
            {
                Subscriptions.Remove(name);
            }
        }

        public void SubscribeReport(ScriptEventData data)
        {
            int total = 0;
            Log.Write(LogLevel.Info, "EventReport", "ScriptEvent Subscriptions by Object:");
            foreach(var sub in Subscriptions)
            {
                Log.Write(LogLevel.Info, "EventReport", "   " + sub.Key + " has " + sub.Value + " subscriptions.");
                total += sub.Value;
            }
            Log.Write(LogLevel.Info, "EventReport", "Total subscriptions is " + total);
        }
    }
}