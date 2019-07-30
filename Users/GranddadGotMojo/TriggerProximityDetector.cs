/* This content is licensed under the terms of the Creative Commons Attribution 4.0 International License.
 * When using this content, you must:
 * •    Acknowledge that the content is from the Sansar Knowledge Base.
 * •    Include our copyright notice: "© 2017 Linden Research, Inc."
 * •    Indicate that the content is licensed under the Creative Commons Attribution-Share Alike 4.0 International License.
 * •    Include the URL for, or link to, the license summary at https://creativecommons.org/licenses/by-sa/4.0/deed.hi (and, if possible, to the complete license terms at https://creativecommons.org/licenses/by-sa/4.0/legalcode.
 * For example:
 * "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."
 * SimpleProximityDetector.cs created by Sansar User Gindipple 8/27/2018
 */

using Sansar.Script;
using Sansar.Simulation;
using System;
using System.Linq;

namespace SimpleScripts
{
    public class TriggerProximityDetector : SceneObjectScript
    {
        #region EditorProperties
        // The events to send on proximity detection of this object. Can be a comma separated list of event names.
        // The events sent will cycle through only the On Detection events with values set.
        [DefaultValue("on")]
        [DisplayName("On Detection ->")]
        public readonly string OnDetection;

        // How long to ignore between detections
        [DefaultValue(5)]
        [DisplayName("Seconds to ignore")]
        [Range(1, 1000)]
        public int SecondsToIgnore = 5;

        // How often to scan for detections
        [DefaultValue(1)]
        [DisplayName("Seconds between scans")]
        [Range(1, 1000)]
        public int ScanSeconds = 1;

        // How close to get before detected
        [DefaultValue(1.0)]
        [DisplayName("Detection Range")]
        public float DetectionRange = 1.0f;

        // Enable responding to events for this script
        [DefaultValue("proximity_enable")]
        [DisplayName("-> Enable")]
        public readonly string EnableEvent;

        // Disable responding to events for this script
        [DefaultValue("proximity_disable")]
        [DisplayName("-> Disable")]
        public readonly string DisableEvent;

        // If StartEnabled is true then the script will respond to interactions when the scene is loaded
        // If StartEnabled is false then the script will not respond to interactions until an (-> Enable) event is received.
        [DefaultValue(true)]
        [DisplayName("Start Enabled")]
        public readonly bool StartEnabled = true;
        #endregion

        #region SimpleHelpers
        public interface ISimpleData
        {
            AgentInfo AgentInfo { get; }
            ObjectId ObjectId { get; }
        }

        public class SimpleData : Reflective, ISimpleData
        {
            public AgentInfo AgentInfo { get; set; }
            public ObjectId ObjectId { get; set; }
        }

        public interface IDebugger { bool DebugSimple { get; } }
        private bool __debugInitialized = false;
        private bool __SimpleDebugging = false;
        private string __SimpleTag = "";
        private void SetupSimple()
        {
            __debugInitialized = true;
            __SimpleTag = GetType().Name + " [S:" + Script.ID.ToString() + " O:" + ObjectPrivate.ObjectId.ToString() + "]";
            Wait(TimeSpan.FromSeconds(1));
            IDebugger debugger = ScenePrivate.FindReflective<IDebugger>("SimpleDebugger").FirstOrDefault();
            if (debugger != null)
                __SimpleDebugging = debugger.DebugSimple;
        }

        private Action SubscribeToAll(string csv, Action<ScriptEventData> callback)
        {
            if (!__debugInitialized)
                SetupSimple();
            if (string.IsNullOrWhiteSpace(csv))
                return null;
            Action unsubscribes = null;
            string [] events = csv.Trim().Split(',');
            if (__SimpleDebugging)
            {
                Log.Write(LogLevel.Info, __SimpleTag, "Subscribing to " + events.Length + " events: " + string.Join(", ", events));
            }
            foreach (string eventName in events)
            {
                if (__SimpleDebugging)
                {
                    var sub = SubscribeToScriptEvent(eventName.Trim(), (ScriptEventData data) =>
                    {
                        Log.Write(LogLevel.Info, __SimpleTag, "Received event " + eventName);
                        callback(data);
                    });
                    unsubscribes += sub.Unsubscribe;
                }
                else
                {
                    var sub = SubscribeToScriptEvent(eventName.Trim(), callback);
                    unsubscribes += sub.Unsubscribe;
                }

            }
            return unsubscribes;
        }

        private void SendToAll(string csv, Reflective data)
        {
            if (!__debugInitialized)
                SetupSimple();
            if (string.IsNullOrWhiteSpace(csv))
                return;
            string [] events = csv.Trim().Split(',');

            if (__SimpleDebugging)
                Log.Write(LogLevel.Info, __SimpleTag, "Sending " + events.Length + " events: " + string.Join(", ", events));
            foreach (string eventName in events)
            {
                PostScriptEvent(eventName.Trim(), data);
            }
        }
        #endregion

        #region Private Variables
        DateTime LastDetectionTime = DateTime.Now;
        Action Unsubscribes = null;
        bool enabled = true;

        bool sendMessageFlag = true;
        bool AgentInDetectionRange = false;
        string Jammer = "None";
        #endregion

        public override void Init()
        {
            Script.UnhandledException += UncaughtExceptionHandler;

            if (DetectionRange < 0.0)
                DetectionRange = 1.0f;

            if (StartEnabled)
                Subscribe(null);

            SubscribeToAll(EnableEvent, Subscribe);
            SubscribeToAll(DisableEvent, Unsubscribe);

            StartCoroutine(Detection);
        }

        public void Detection()
        {
            while (true)
            {
                try
                {
                    if (enabled)
                    {
                        AgentInDetectionRange = false;
                        foreach (AgentPrivate agent in ScenePrivate.GetAgents())
                        {
                            float agentDist = (ScenePrivate.FindObject(agent.AgentInfo.ObjectId).Position - ObjectPrivate.Position).Length();
                            if (agentDist <= DetectionRange) // (DateTime.Now - LastDetectionTime).TotalSeconds >= SecondsToIgnore)
                            {
                                SimpleData sd = new SimpleData();
                                sd.AgentInfo = agent.AgentInfo;
                                //sd.ObjectId = sd.AgentInfo.ObjectId;
                                sd.ObjectId = ObjectPrivate.ObjectId;
                                AgentInDetectionRange = true;
                                //Log.Write("sendMessageFlag: " + sendMessageFlag);
                                //Log.Write("Jammer: " + Jammer);
                                if (sendMessageFlag) 
                                    if (Jammer == "None")
                                    {
                                        Log.Write("sending message");
                                        SendToAll(OnDetection, sd);
                                        sendMessageFlag = false;
                                        Jammer = agent.AgentInfo.Name;
                                    }
                                    else
                                    {
                                        if (Jammer != agent.AgentInfo.Name)
                                        {
                                            SendToAll(OnDetection, sd);
                                            sendMessageFlag = false;
                                            Jammer = agent.AgentInfo.Name;
                                        }
                                    }
                                else
                                {
                                    sendMessageFlag = false;
                                }
                                
                                //LastDetectionTime = DateTime.Now;
                            }
                            else
                            {
                                if (!AgentInDetectionRange)
                                {
                                    sendMessageFlag = true;
                                    Jammer = null;
                                }
                            }
                        }
                    }
                    Wait(TimeSpan.FromSeconds(ScanSeconds)); // check once per ScanSeconds
                }
                catch (Exception ex)
                {
                    Wait(TimeSpan.FromSeconds(ScanSeconds)); // ensure at least ScanSeconds on error
                    if (__SimpleDebugging)
                    {
                        Log.Write(LogLevel.Error, __SimpleTag, "Proximity Detection Failed: " + ex.Message);
                    }
                }
            }
        }

        private void Subscribe(ScriptEventData sed)
        {
            if (Unsubscribes == null)
            {
                enabled = true;
            }
        }

        private void Unsubscribe(ScriptEventData sed)
        {
            if (Unsubscribes != null)
            {
                enabled = false;
                Unsubscribes();
                Unsubscribes = null;
            }
        }

        private void UncaughtExceptionHandler(object sender, Exception ex)
        {
            string type;
            if (!Script.UnhandledExceptionRecoverable)
                type = " Urecoverable Exception: ";
            else
                type = " Exception: ";

            Log.Write(LogLevel.Error, GetType().Name + type + ex.Message);
        }
    }
}
