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
    [Tooltip("Updates audio streams in response to simple script events.")]
    [DisplayName("Audio Stream")]
    public class AudioStream : LibraryBase  // PlayStream methods require AgentPrivate
    {
        #region EditorProperties
        [Tooltip("Events to trigger media stream")]
        [DefaultValue("on")]
        [DisplayName("-> Switch to Media Stream")]
        public readonly string MediaStreamEvent;

        [Tooltip(@"The loudness the stream will be played at.")]
        [DefaultValue(50.0f)]
        [Range(0.0f, 100.0f)]
        [DisplayName("Media Loudness")]
        public readonly float MediaStreamLoudness;

        [Tooltip("Events to trigger audio stream")]
        [DefaultValue("")]
        [DisplayName("-> Switch to Audio Stream")]
        public readonly string AudioStreamEvent;

        [Tooltip(@"The loudness the stream will be played at.")]
        [DefaultValue(50.0f)]
        [Range(0.0f, 100.0f)]
        [DisplayName("Loudness")]
        public readonly float AudioStreamLoudness;

        [Tooltip("Stop the currently playing stream. Can be a comma separated list of event names.")]
        [DefaultValue("off")]
        [DisplayName("-> Stop Stream")]
        public readonly string StopEvent;

        [Tooltip(@"If false the stream will play at the same loudness for everyone.
If true and the object has an AudioComponent then the sound will come from the audio component, and follow the object if it moves.
If true and the object does not have an AudioComponent then the sound will play from the position of the object at the time the event is received and will not move.")]
        [DefaultValue(true)]
        [DisplayName("Spatialized")]
        public readonly bool Spatialized;

        [Tooltip("Private media for one agent only")]
        [DefaultValue(false)]
        [DisplayName("Private Media")]
        public readonly bool PrivateMedia = false;

        [Tooltip("Enable responding to events for this script")]
        [DefaultValue("media_enable")]
        [DisplayName("-> Enable")]
        public readonly string EnableEvent;

        [Tooltip("Disable responding to events for this script")]
        [DefaultValue("media_disable")]
        [DisplayName("-> Disable")]
        public readonly string DisableEvent;

        [Tooltip(@"If StartEnabled is true then the script will respond to interactions when the scene is loaded
If StartEnabled is false then the script will not respond to interactions until an (-> Enable) event is received.")]
        [DefaultValue(true)]
        [DisplayName("Start Enabled")]
        public readonly bool StartEnabled = true;
        #endregion

        Action unsubscribes = null;

        private AudioComponent audio;
        private PlayHandle currentPlayHandle = null;

        protected override void SimpleInit()
        {
            ObjectPrivate.TryGetFirstComponent(out audio);

            if (StartEnabled) SubscribeToMessages();

            SubscribeToAll(EnableEvent, (ScriptEventData data) => { SubscribeToMessages(); });
            SubscribeToAll(DisableEvent, (ScriptEventData data) => { UnsubscribeFromMessages(); });
        }

        void SubscribeToMessages()
        {
            // Already subscribed
            if (unsubscribes != null)
                return;

            if (!string.IsNullOrWhiteSpace(MediaStreamEvent))
                Setup(StreamChannel.MediaChannel, MediaStreamEvent, LoudnessPercentToDb(MediaStreamLoudness));
            if (!string.IsNullOrWhiteSpace(AudioStreamEvent))
                Setup(StreamChannel.AudioChannel, AudioStreamEvent, LoudnessPercentToDb(AudioStreamLoudness));
            unsubscribes += SubscribeToAll(StopEvent, (data) =>
            {
                StopSound(true);
            });
        }

        private float LoudnessPercentToDb(float loudnessPercent)
        {
            loudnessPercent = Math.Min(Math.Max(loudnessPercent, 0.0f), 100.0f);
            return 60.0f * (loudnessPercent / 100.0f) - 48.0f;
        }

        private void StopSound(bool fadeout)
        {
            if (currentPlayHandle != null)
            {
                currentPlayHandle.Stop(fadeout);
                currentPlayHandle = null;
            }
        }

        void Setup(StreamChannel channel, string eventName, float loudness)
        {
            unsubscribes += SubscribeToAll(eventName, (ScriptEventData subData) =>
            {
                StopSound(true);

                if (PrivateMedia)
                {
                    ISimpleData simpleData = subData.Data?.AsInterface<ISimpleData>();

                    if (simpleData != null && simpleData.AgentInfo != null)
                    {
                        AgentPrivate agent = ScenePrivate.FindAgent(simpleData.AgentInfo.SessionId);

                        if (agent != null && agent.IsValid)
                        {
                            try
                            {
                                if (Spatialized)
                                {
                                    if (audio != null)
                                        currentPlayHandle = agent.PlayStreamOnComponent(channel, audio, loudness);
                                    else
                                        currentPlayHandle = agent.PlayStreamAtPosition(channel, ObjectPrivate.Position, loudness);
                                }
                                else
                                    currentPlayHandle = agent.PlayStream(channel, loudness);
                            }
                            catch (ThrottleException)
                            {
                                // Throttled
                                Log.Write(LogLevel.Warning, "PlayStream request throttle hit. Stream not set.");
                            }
                            catch (NullReferenceException)
                            {
                                // User Left, ignore.
                            }
                        }
                    }
                }
                else
                {
                    try
                    {
                        if (Spatialized)
                        {
                            if (audio != null)
                                currentPlayHandle = audio.PlayStreamOnComponent(channel, loudness);
                            else
                                currentPlayHandle = ScenePrivate.PlayStreamAtPosition(channel, ObjectPrivate.Position, loudness);
                        }
                        else
                            currentPlayHandle = ScenePrivate.PlayStream(channel, loudness);
                    }
                    catch (ThrottleException)
                    {
                        // Throttled
                        Log.Write(LogLevel.Warning, "PlayStream request throttle hit. Stream not set.");
                    }
                }
            });
        }

        void UnsubscribeFromMessages()
        {
            StopSound(true);

            if (unsubscribes != null)
            {
                unsubscribes();
                unsubscribes = null;
            }
        }
    }
}