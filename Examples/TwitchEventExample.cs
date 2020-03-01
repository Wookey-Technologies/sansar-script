/* This content is licensed under the terms of the Creative Commons Attribution 4.0 International License.
 * When using this content, you must:
 * •    Acknowledge that the content is from the Sansar Knowledge Base.
 * •    Include our copyright notice: "© 2018 Linden Research, Inc."
 * •    Indicate that the content is licensed under the Creative Commons Attribution-Share Alike 4.0 International License.
 * •    Include the URL for, or link to, the license summary at https://creativecommons.org/licenses/by-sa/4.0/deed.hi (and, if possible, to the complete license terms at https://creativecommons.org/licenses/by-sa/4.0/legalcode.
 * For example:
 * "This work uses content from the Sansar Knowledge Base. © 2018 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."
 */

// Attach to an animated object, and specifiy the speed, start and end frames, and playback mode (loop or ping pong)

using Sansar.Script;
using Sansar.Simulation;
using System.Collections.Generic;
using System.Linq;


// This script watches for twitch events on all agents in the scene and reports anyone above the given threshold
class TwitchEventExample : SceneObjectScript
{
    [Tooltip("Scale existing values at this rate")]
    [DefaultValue(10.0f)]
    public readonly float CooldownSeconds;

    [Tooltip("Scale existing values by this amount")]
    [DefaultValue(0.8f)]
    public readonly float CooldownRate;

    [Tooltip("Announce to chat every cooldown agents with a value over this amount")]
    [DefaultValue(0.6f)]
    public readonly float ReportThreshold;

    public class TrackedEvent
    {
        public IEventSubscription Subscription { get; set; }
        public Dictionary<TwitchEventType, float> EventIntensity { get; } = new Dictionary<TwitchEventType, float>();
    }

    private IEventSubscription timer = null;
    private Dictionary<SessionId, TrackedEvent> trackedEvents = new Dictionary<SessionId, TrackedEvent>();
    public override void Init()
    {
        ScenePrivate.User.Subscribe(User.AddUser, AddUser);
        ScenePrivate.User.Subscribe(User.RemoveUser, RemoveUser);
    }

    // Report anyone over the threshold
    private void Report(SessionId id, TwitchEventType eventType, float newIntensity)
    {
        AgentPrivate agent = ScenePrivate.FindAgent(id);

        if(agent != null)
        {
            ScenePrivate.Chat.MessageAllUsers($"{agent.AgentInfo.Name} has the top {eventType} with {newIntensity}");
        }
    }

    // Decay values over time to only report recent events
    private void OnTimer()
    {
        foreach (var trackedEvent in trackedEvents)
        {
            foreach(var eventIntensity in trackedEvent.Value.EventIntensity.ToArray())
            {
                float newIntensity = eventIntensity.Value * CooldownRate;
                trackedEvent.Value.EventIntensity[eventIntensity.Key] = newIntensity;

                if(newIntensity > ReportThreshold)
                {
                    Report(trackedEvent.Key, eventIntensity.Key, newIntensity);
                }
            }
        }
    }

    private void TwitchEvent(TwitchData data)
    {
        if (trackedEvents.TryGetValue(data.SessionId, out TrackedEvent trackedEvent))
        {
            float intensity;
            trackedEvent.EventIntensity.TryGetValue(data.EventType, out intensity);
            intensity += data.Intensity;

            trackedEvent.EventIntensity[data.EventType] = intensity;
        }
    }

    private void AddUser(UserData data)
    {
        AgentPrivate agent = ScenePrivate.FindAgent(data.User);

        TrackedEvent tracked = new TrackedEvent();

        trackedEvents[data.User] = tracked;

        tracked.Subscription = agent.Client.SubscribeToTwitch(TwitchEvent);

        if(timer == null)
        {
            timer = Timer.Create(CooldownSeconds, CooldownSeconds, OnTimer);
        }

    }
    private void RemoveUser(UserData data)
    {
        trackedEvents.Remove(data.User);

        if(trackedEvents.Count == 0)
        {
            timer.Unsubscribe();
            timer = null;
        }
    }

}


