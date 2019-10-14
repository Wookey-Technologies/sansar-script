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

[assembly: Tooltip("A collection of scripts that work together through named events to do common simple scripting tasks.\nEvents that start with -> respond to event names by doing something.\nEvents that end with -> will send an event after something is done.")]
namespace ScriptLibrary 
{
    // Adding any data to this will mean messages from old scripts will fail to be processed correctly in new scripts.
    public interface ISimpleData
    {
        AgentInfo AgentInfo { get; }
        ObjectId ObjectId { get; }
        ObjectId SourceObjectId { get; }

        // Extra data
        Reflective ExtraData { get; }
    }

    public class SimpleData : Reflective, ISimpleData
    {
        public SimpleData(ScriptBase script) { ExtraData = script; }
        public AgentInfo AgentInfo { get; set; }
        public ObjectId ObjectId { get; set; }
        public ObjectId SourceObjectId { get; set; }

        public Reflective ExtraData { get; }
    }

    internal class EventCollection
    {
        internal EventCollection(IEnumerable<string> eventChain, IBase script)
        {
            if (eventChain.Count() == 0) return;

            Events = eventChain.First().Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < Events.Length; ++i)
            {
                Events[i] = GenerateEventName(Events[i], script.GetGroup(), script.GetObjectId());
            }

            if (eventChain.Count() > 1)
            {
                Next = new EventCollection(eventChain.Skip(1), script);
            }
        }

        private string GenerateEventName(string eventName, string group, ObjectId objectId)
        {
            eventName = eventName.Trim();
            if (eventName.EndsWith("@"))
            {
                // Special case on@ to send the event globally (the null group) by sending w/o the @.
                eventName = eventName.Substring(0, eventName.Length - 1);
            }
            else if (group != "" && !eventName.Contains("@"))
            {
                // Append the group if there is a group and there isn't a group override set
                eventName = eventName + "@" + group;
            }

            return eventName.Replace("{object}", "{o:" + objectId + "}");
        }

        internal string[] Events;
        internal EventCollection Next = null;
        internal Action Unsubscribe = null;
    }

    internal class EventWrapper
    {
        internal EventWrapper(string eventCsv, IBase script)
        {
            ThisScript = script;
            Head = new EventCollection(eventCsv.Split(new string[] { ">>" }, StringSplitOptions.RemoveEmptyEntries), script);
        }

        internal void SendToAll(Reflective data)
        {
            Next();

            ThisScript.SimpleLog(LogLevel.Info, "Sending " + Current.Events.Length + " events: " + string.Join(", ", Current.Events));

            foreach (string eventName in Current.Events)
            {
                ThisScript.PostSimpleEvent(eventName, data);
            }
        }

        internal Action SubscribeToAll(Action<ScriptEventData> callback)
        {
            Next();

            ThisScript.SimpleLog(LogLevel.Info, "Subscribing to " + Current.Events.Length + " events: " + string.Join(", ", Current.Events));
            
            Action<ScriptEventData> wrappedCallback = callback;
            foreach (string eventName in Current.Events)
            {
                if (ThisScript.DebugEnabled())
                {
                    var sub = ThisScript.SubscribeToSimpleEvent(eventName, (ScriptEventData data) =>
                    {
                        ThisScript.SimpleLog(LogLevel.Info, "Received event " + eventName);
                        wrappedCallback(data);
                    });
                    Current.Unsubscribe += sub.Unsubscribe;
                }
                else
                {
                    var sub = ThisScript.SubscribeToSimpleEvent(eventName, wrappedCallback);
                    Current.Unsubscribe += sub.Unsubscribe;
                }
            }
            Current.Unsubscribe += () => ThisScript.UnsubscribeToSimpleEvent(Current.Events.Length);
            return Current.Unsubscribe;
        }

        public void Next()
        {
            if (Current != null && Current.Unsubscribe != null)
            {
                Current.Unsubscribe();
                Current.Unsubscribe = null;
            }

            if (Current == null) Current = Head;
            else if (Current.Next != null) Current = Current.Next;
            else if (Current != Head) Current = Head;
        }

        internal void Reset()
        {
            if (Current != null && Current.Unsubscribe != null)
            {
                Current.Unsubscribe();
                Current.Unsubscribe = null;
            }

            Current = null;
        }

        private readonly IBase ThisScript = null;
        private readonly EventCollection Head = null;
        private EventCollection Current = null;
    }

    internal interface IBase
    {
        void SimpleLog(LogLevel level, string logMessage);
        string GetGroup();
        ObjectId GetObjectId();
        bool DebugEnabled();

        void PostSimpleEvent(string name, Reflective data);

        IEventSubscription SubscribeToSimpleEvent(string Message, Action<ScriptEventData> callback);
        void UnsubscribeToSimpleEvent(int eventCount);
    }

    public interface IDebug
    {
        bool DebugSimple { get; }
        void TrackSubscribe(string name, int count = 1);
        bool TrackingSubscriptions { get; }
    }

    public class DebugDisabled : IDebug
    {
        public bool DebugSimple { get { return false; } }
        public void TrackSubscribe(string name, int count = 1) { }
        public bool TrackingSubscriptions { get { return false; } }
    }

    public abstract class LibraryBase : SceneObjectScript, IBase
    {
        [Tooltip(@"If a Group is set, will only respond and send to other SimpleScripts with the same Group tag set.
Does NOT accept CSV lists of groups.
Use {object} as the group to create a group that is unique to this object.
To send or receive events to/from a specific group from outside that group append an @ and the group name to the event name, e.g. ""on@my_group""")]
        [DefaultValue("")]
        [DisplayName("Group")]
        public string Group = "";

        [DisplayName("Fast Start")]
        [Tooltip("Fast Start\nEnabling Fast Start will skip waiting for debugger initialization. This means debug logs from early in the script's initialization may be missed or lost.\n\nWARNING: Enable only if the 1.5s delay at initialization when no debugger is in the scene is really killing you.")]
        [DefaultValue(false)]
        public bool FastStart = false;

        internal IDebug __SimpleDebugger = new DebugDisabled();
        internal string __SimpleTag = "";

        #region IBase
        public void SimpleLog(LogLevel level, string logMessage)
        {
            if (level == LogLevel.Error || __SimpleDebugger.DebugSimple) Log.Write(level, __SimpleTag, logMessage);
        }
        public string GetGroup() { return Group; }
        public ObjectId GetObjectId() { return ObjectPrivate.ObjectId; }
        public bool DebugEnabled() { return __SimpleDebugger.DebugSimple; }

        public void PostSimpleEvent(string name, Reflective data)
        {
            PostScriptEvent(name, data);
        }

        public IEventSubscription SubscribeToSimpleEvent(string Message, Action<ScriptEventData> callback)
        {
            if (__SimpleDebugger.TrackingSubscriptions) __SimpleDebugger.TrackSubscribe(ObjectPrivate.Name);
            return base.SubscribeToScriptEvent(Message, callback);
        }

        public void UnsubscribeToSimpleEvent(int eventCount)
        {
            if (__SimpleDebugger.TrackingSubscriptions) __SimpleDebugger.TrackSubscribe(ObjectPrivate.Name, eventCount * -1);
        }
        #endregion

        protected abstract void SimpleInit();
        bool foundDebugger = false;

        public sealed override void Init()
        {
            __SimpleTag = GetType().Name + " [S:" + Script.ID.ToString() + " O:" + ObjectPrivate.ObjectId.ToString() + "]";
            StartCoroutine(FindDebugger, 0.1);

            if (!FastStart)
            {
                Yield();
                int retries = 15;
                while (!foundDebugger && --retries > 0) Wait(0.1);
            }

            SimpleInit();
        }

        private void FindDebugger(double rate)
        {
            int retries = 10;
            IDebug debugger = null;
            do
            {
                debugger = ScenePrivate.FindReflective<IDebug>("Simple.Debugger").FirstOrDefault();
                if (debugger == null) Wait(rate);
            } while (debugger == null && --retries > 0);

            if (debugger != null)
            {
                __SimpleDebugger = debugger;
                return;
            }

            if (rate < 3.0)
            {
                StartCoroutine(() =>
                {
                    FindDebugger(rate * 2);
                });
            }
        }

        private Dictionary<string, EventWrapper> SendEventWrappers = new Dictionary<string, EventWrapper>();
        private Dictionary<string, EventWrapper> SubscribeEventWrappers = new Dictionary<string, EventWrapper>();

        private EventWrapper GetWrapper(Dictionary<string, EventWrapper> eventWrappers, string events)
        {
            if (string.IsNullOrWhiteSpace(events)) return null;

            EventWrapper wrapper;
            if (!eventWrappers.TryGetValue(events, out wrapper))
            {
                wrapper = new EventWrapper(events, this);
                eventWrappers[events] = wrapper;
            }
            return wrapper;
        }

        protected void SendToAll(string csv, Reflective data)
        {
            GetWrapper(SendEventWrappers, csv)?.SendToAll(data);
        }

        protected Action SubscribeToAll(string csv, Action<ScriptEventData> callback)
        {
            return GetWrapper(SubscribeEventWrappers, csv)?.SubscribeToAll(callback);
        }

        protected void ResetSendState(string csv)
        {
            GetWrapper(SendEventWrappers, csv)?.Reset();
        }

        protected void NextSendState(string csv)
        {
            GetWrapper(SendEventWrappers, csv)?.Next();
        }

        protected void ResetSubscribeState(string csv)
        {
            GetWrapper(SubscribeEventWrappers, csv)?.Reset();
        }
    }
}
