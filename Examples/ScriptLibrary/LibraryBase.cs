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
                Events[i] = GenerateEventName(Events[i], script.GetGroup());
            }

            if (eventChain.Count() > 1)
            {
                Next = new EventCollection(eventChain.Skip(1), script);
            }
        }

        private string GenerateEventName(string eventName, string group)
        {
            eventName = eventName.Trim();
            if (eventName.EndsWith("@"))
            {
                // Special case on@ to send the event globally (the null group) by sending w/o the @.
                return eventName.Substring(0, eventName.Length - 1);
            }
            else if (group == "" || eventName.Contains("@"))
            {
                // No group was set or already targeting a specific group as is.
                return eventName;
            }
            else
            {
                // Append the group
                return eventName + "@" + group;
            }
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

        internal void SendToAll(SimpleData data)
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
            return Current.Unsubscribe;
        }

        private void Next()
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

            Current = Head;
        }

        private readonly IBase ThisScript = null;
        private readonly EventCollection Head = null;
        private EventCollection Current = null;
    }

    internal interface IBase
    {
        void SimpleLog(LogLevel level, string logMessage);
        string GetGroup();
        bool DebugEnabled();

        void PostSimpleEvent(string name, SimpleData data);

        IEventSubscription SubscribeToSimpleEvent(string Message, Action<ScriptEventData> callback);

    }

    public interface IDebug
    {
        bool DebugSimple { get; }
    }

    [Tooltip("Requires a scene object - will not work on rezzed objects.")]
    public abstract class SceneObjectBase : SceneObjectScript, IBase
    {
        [Tooltip(@"If a Group is set, will only respond and send to other SimpleScripts with the same Group tag set.
Does NOT accept CSV lists of groups.
To send or receive events to/from a specific group from outside that group append an @ and the group name to the event name, e.g. ""on@my_group""")]
        [DefaultValue("")]
        [DisplayName("Group")]
        public string Group = "";

        internal bool __SimpleDebugging = false;
        internal string __SimpleTag = "";

        #region IBase
        public void SimpleLog(LogLevel level, string logMessage)
        {
            if (__SimpleDebugging) Log.Write(level, __SimpleTag, logMessage);
        }
        public string GetGroup() { return Group; }
        public bool DebugEnabled() { return __SimpleDebugging; }

        public void PostSimpleEvent(string name, SimpleData data)
        {
            PostScriptEvent(name, data);
        }

        public IEventSubscription SubscribeToSimpleEvent(string Message, Action<ScriptEventData> callback)
        {
            return base.SubscribeToScriptEvent(Message, callback);
        }
        #endregion

        protected abstract void SimpleInit();

        public sealed override void Init()
        {
            __SimpleTag = GetType().Name + " [S:" + Script.ID.ToString() + " O:" + ObjectPrivate.ObjectId.ToString() + "]";
            Wait(TimeSpan.FromSeconds(1));
            IDebug debugger = ScenePrivate.FindReflective<IDebug>("Simple.Debugger").FirstOrDefault();
            if (debugger != null) __SimpleDebugging = debugger.DebugSimple;

            SimpleInit();
        }

        private Dictionary<string, EventWrapper> EventWrappers = new Dictionary<string, EventWrapper>();

        private EventWrapper GetWrapper(string events)
        {
            if (string.IsNullOrWhiteSpace(events)) return null;

            EventWrapper wrapper;
            if (!EventWrappers.TryGetValue(events, out wrapper))
            {
                wrapper = new EventWrapper(events, this);
                EventWrappers[events] = wrapper;
            }
            return wrapper;
        }

        protected void SendToAll(string csv, SimpleData data)
        {
            GetWrapper(csv)?.SendToAll(data);
        }

        protected Action SubscribeToAll(string csv, Action<ScriptEventData> callback)
        {
            return GetWrapper(csv)?.SubscribeToAll(callback);
        }

        protected void Reset(string csv)
        {
            GetWrapper(csv)?.Reset();
        }
    }

    [Tooltip("Will work on objects rezzed or built into the scene.")]
    public abstract class ObjectBase : ObjectScript, IBase
    {
        [Tooltip(@"If a Group is set, will only respond and send to other SimpleScripts with the same Group tag set.
Does NOT accept CSV lists of groups.
To send or receive events to/from a specific group from outside that group append an @ and the group name to the event name, e.g. ""on@my_group""")]
        [DefaultValue("")]
        [DisplayName("Group")]
        public string Group = "";

        internal bool __SimpleDebugging = false;
        internal string __SimpleTag = "";

        #region IBase
        public void SimpleLog(LogLevel level, string logMessage)
        {
            if (__SimpleDebugging) Log.Write(level, __SimpleTag, logMessage);
        }
        public string GetGroup() { return Group; }
        public bool DebugEnabled() { return __SimpleDebugging; }

        public void PostSimpleEvent(string name, SimpleData data)
        {
            PostScriptEvent(name, data);
        }

        public IEventSubscription SubscribeToSimpleEvent(string Message, Action<ScriptEventData> callback)
        {
            return base.SubscribeToScriptEvent(Message, callback);
        }
        #endregion

        protected abstract void SimpleInit();

        public sealed override void Init()
        {
            __SimpleTag = GetType().Name + " [S:" + Script.ID.ToString() + " O:" + ObjectPrivate.ObjectId.ToString() + "]";
            Wait(TimeSpan.FromSeconds(1));
            IDebug debugger = ScenePublic.FindReflective<IDebug>("Simple.Debugger").FirstOrDefault();
            if (debugger != null) __SimpleDebugging = debugger.DebugSimple;

            SimpleInit();
        }

        private Dictionary<string, EventWrapper> EventWrappers = new Dictionary<string, EventWrapper>();

        private EventWrapper GetWrapper(string events)
        {
            if (string.IsNullOrWhiteSpace(events)) return null;

            EventWrapper wrapper;
            if (!EventWrappers.TryGetValue(events, out wrapper))
            {
                wrapper = new EventWrapper(events, this);
                EventWrappers[events] = wrapper;
            }
            return wrapper;
        }

        protected void SendToAll(string csv, SimpleData data)
        {
            GetWrapper(csv)?.SendToAll(data);
        }

        protected Action SubscribeToAll(string csv, Action<ScriptEventData> callback)
        {
            return GetWrapper(csv)?.SubscribeToAll(callback);
        }

        protected void Reset(string csv)
        {
            GetWrapper(csv)?.Reset();
        }
    }
}
