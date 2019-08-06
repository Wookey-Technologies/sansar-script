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
    [Tooltip("Plays a sound in response to simple script events.")]
    [DisplayName(nameof(Sound))]
    public class Sound : LibraryBase // Playing sounds at scene positions requires ScenePrivate. Could make another version that always played on the object component of rezzed objects.
    {
        #region EditorProperties
        [Tooltip("The sound resource to play")]
        [DisplayName("Sound")]
        public readonly SoundResource SoundResource;

        [Tooltip(@"If false the sound will play at the same loudness for everyone.
If true and the object has an AudioComponent then the sound will come from the audio component, and follow the object if it moves.
If true and the object does not have an AudioComponent then the sound will play from the position of the object at the time the event is received and will not move.")]
        [DefaultValue(true)]
        [DisplayName("Spatialized")]
        public readonly bool Spatialized;

        [Tooltip("If not zero the pitch will be shifted on each play by a random amount between -PitchVariance and +PitchVariance, in semitones.")]
        [DefaultValue(0.0f)]
        [Range(0.0f, 24.0f)]
        [DisplayName("Pitch Variance")]
        public readonly float PitchVariance;

        [Tooltip("If not zero the loudness will be shifted on each play by a random amount between -LoudnessVariance and +LoudnessVariance.")]
        [DefaultValue(0.0f)]
        [Range(0.0f, 100.0f)]
        [DisplayName("Loudness Variance")]
        public readonly float LoudnessVariance;

        [Tooltip("Play the sound. Can be a comma separated list of event names.")]
        [DefaultValue("on")]
        [DisplayName("-> Play Sound")]
        public readonly string PlayEvent;

        [Tooltip(@"The minimum loudness the sound will be played at. If less than Maximum Loudness the sound will play each time at a randomly chosen loudness between the two.")]
        [DefaultValue(50.0f)]
        [Range(0.0f, 100.0f)]
        [DisplayName("Loudness")]
        public readonly float Loudness;

        [Tooltip(@"Playback pitch will be shifted by this number of semitones.")]
        [DefaultValue(0.0f)]
        [Range(-24.0f, 24.0f)]
        [DisplayName("Pitch Offset")]
        public readonly float PitchOffset;

        [Tooltip(@"Alternate way to playback the sound. Can be a comma separated list of event names.")]
        [DefaultValue("")]
        [DisplayName("-> Adjust Playback")]
        public readonly string AdjustEvent;

        [Tooltip(@"Alternate playback loudness.")]
        [DefaultValue(50.0f)]
        [Range(0.0f, 100.0f)]
        [DisplayName("Adjusted Loudness")]
        public readonly float AdjustedLoudness;

        [Tooltip(@"Alternate playback pitch will be shifted by this number of semitones.")]
        [DefaultValue(0.0f)]
        [Range(-24.0f, 24.0f)]
        [DisplayName("Adjusted Pitch Offset")]
        public readonly float AdjustedPitchOffset;

        [Tooltip(@"Fade time for adjustments.")]
        [DisplayName("Interpolation Time")]
        [Range(0.0f, 5.0f)]
        public readonly float AdjustFadeTime;

        [Tooltip("Stop the currently playing sound. Can be a comma separated list of event names.")]
        [DefaultValue("off")]
        [DisplayName("-> Stop Sound")]
        public readonly string StopEvent;

        [Tooltip("Make this sound loop.")]
        [DisplayName("Looping")]
        public readonly bool Looping;

        [Tooltip("Auto-play this looping sound when the script is enabled.")]
        [DisplayName("Looping Auto-play")]
        public readonly bool LoopingAutoplay;

        [Tooltip("Enable responding to events for this script. Can be a comma separated list of event names.")]
        [DefaultValue("sound_enable")]
        [DisplayName("-> Enable")]
        public readonly string EnableEvent;

        [Tooltip("Disable responding to events for this script. Can be a comma separated list of event names.")]
        [DefaultValue("sound_disable")]
        [DisplayName("-> Disable")]
        public readonly string DisableEvent;

        [Tooltip(@"If StartEnabled is true then the script will respond to events when the scene is loaded
If StartEnabled is false then the script will not respond to events until an (-> Enable) event is received.")]
        [DefaultValue(true)]
        [DisplayName("Start Enabled")]
        public readonly bool StartEnabled = true;

        #endregion

        Action subscription = null;

        private Random rnd;
        private AudioComponent audio;
        private PlayHandle currentPlayHandle = null;
        private ICoroutine fadeCoroutine = null;
        private float fadeTime = 0.0f;
        private float previousLoudness = 0.0f;
        private float previousPitchShift = 0.0f;
        private float targetLoudness = 0.0f;
        private float targetPitchShift = 0.0f;

        protected override void SimpleInit()
        {
            if (SoundResource == null)
            {
                Log.Write(LogLevel.Error, __SimpleTag, "Sound requires a SoundResource set to work properly.");
                return;
            }

            ObjectPrivate.TryGetFirstComponent(out audio);

            rnd = new Random();

            if (StartEnabled) Subscribe(null);

            SubscribeToAll(EnableEvent, Subscribe);
            SubscribeToAll(DisableEvent, Unsubscribe);
        }

        private void Subscribe(ScriptEventData sed)
        {
            if ((subscription == null) && (SoundResource != null))
            {
                subscription = SubscribeToAll(PlayEvent, (data) =>
                {
                    PlaySound(SoundResource);
                });

                subscription += SubscribeToAll(AdjustEvent, (data) =>
                {
                    AdjustSound(AdjustedLoudness, AdjustedPitchOffset);
                });

                subscription += SubscribeToAll(StopEvent, (data) =>
                {
                    StopSound(true);
                });

                if (Looping && LoopingAutoplay && (currentPlayHandle == null))
                {
                    PlaySound(SoundResource);
                }
            }

            if (AdjustFadeTime > 0.0f)
                StartFadeCoroutine();
        }

        private void Unsubscribe(ScriptEventData sed)
        {
            StopSound(true);

            if (subscription != null)
            {
                subscription();
                subscription = null;
            }

            StopFadeCoroutine();
        }

        private void StopSound(bool fadeout)
        {
            if (currentPlayHandle != null)
            {
                currentPlayHandle.Stop(fadeout);
                currentPlayHandle = null;
            }
        }

        private float RandomNegOneToOne()
        {
            return (float)(rnd.NextDouble() * 2.0 - 1.0);
        }

        private float LoudnessPercentToDb(float loudnessPercent)
        {
            loudnessPercent = Math.Min(Math.Max(loudnessPercent, 0.0f), 100.0f);
            return 60.0f * (loudnessPercent / 100.0f) - 48.0f;
        }

        private float LoudnessDbToPercent(float loudnessDb)
        {
            float percent = (loudnessDb + 48.0f) * 100.0f / 60.0f;
            return Math.Min(Math.Max(percent, 0.0f), 100.0f);
        }

        private void PlaySound(SoundResource sound)
        {
            // Adjust the sound back to normal settings if it is a looping sound that is already playing
            if ((currentPlayHandle != null) && currentPlayHandle.IsPlaying() && Looping)
            {
                AdjustSound(Loudness, PitchOffset);
                return;
            }

            StopSound(false);

            PlaySettings playSettings = (Looping ? PlaySettings.Looped : PlaySettings.PlayOnce);
            playSettings.DontSync = false;
            playSettings.Loudness = LoudnessPercentToDb(Loudness + LoudnessVariance * RandomNegOneToOne());
            playSettings.PitchShift = PitchOffset + PitchVariance * RandomNegOneToOne();

            if (Spatialized)
            {
                if (audio != null)
                {
                    currentPlayHandle = audio.PlaySoundOnComponent(sound, playSettings);
                }
                else
                {
                    currentPlayHandle = ScenePrivate.PlaySoundAtPosition(sound, ObjectPrivate.Position, playSettings);
                }
            }
            else
            {
                currentPlayHandle = ScenePrivate.PlaySound(sound, playSettings);
            }
        }

        private void AdjustSound(float loudness, float pitchOffset)
        {
            if ((currentPlayHandle != null) && currentPlayHandle.IsPlaying())
            {
                targetLoudness = loudness + LoudnessVariance * RandomNegOneToOne();
                targetPitchShift = pitchOffset + PitchVariance * RandomNegOneToOne();

                if (AdjustFadeTime > 0.0f)
                {
                    previousLoudness = LoudnessDbToPercent(currentPlayHandle.GetLoudness());
                    previousPitchShift = currentPlayHandle.GetPitchShift();

                    fadeTime = AdjustFadeTime;
                }
                else
                {
                    fadeTime = 0.0f;

                    float targetLoudnessDb = LoudnessPercentToDb(targetLoudness);
                    currentPlayHandle.SetLoudness(targetLoudnessDb);
                    currentPlayHandle.SetPitchShift(targetPitchShift);
                }
            }
        }

        private void StartFadeCoroutine()
        {
            if ((fadeCoroutine == null) && (AdjustFadeTime > 0.0f))
            {
                fadeTime = 0.0f;

                previousLoudness = 0.0f;
                previousPitchShift = 0.0f;

                fadeCoroutine = StartCoroutine(FadeSoundAdjustments);
            }
        }

        private void StopFadeCoroutine()
        {
            if (fadeCoroutine != null)
            {
                fadeCoroutine.Abort();
                fadeCoroutine = null;
            }
        }

        private void FadeSoundAdjustments()
        {
            const float deltaTime = 0.1f;
            TimeSpan ts = TimeSpan.FromSeconds(deltaTime);

            while (true)
            {
                Wait(ts);

                if ((fadeTime > 0.0f) && (currentPlayHandle != null) && currentPlayHandle.IsPlaying())
                {
                    fadeTime = Math.Max(fadeTime - deltaTime, 0.0f);

                    float t = fadeTime / AdjustFadeTime;

                    float loudness = previousLoudness * t + targetLoudness * (1.0f - t);
                    float pitchShift = previousPitchShift * t + targetPitchShift * (1.0f - t);

                    float loudnessDb = LoudnessPercentToDb(loudness);
                    currentPlayHandle.SetLoudness(loudnessDb);
                    currentPlayHandle.SetPitchShift(pitchShift);
                }
            }
        }
    }
}
