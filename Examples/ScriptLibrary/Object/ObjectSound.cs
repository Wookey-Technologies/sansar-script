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
    [DisplayName("Sound")]
    public class ObjectSound : ObjectBase // Playing sounds at scene positions requires ScenePrivate. Could make another version that always played on the object component of rezzed objects.
    {
        #region EditorProperties
        [Tooltip("Play the sound. Can be a comma separated list of event names.")]
        [DefaultValue("on")]
        [DisplayName("-> Play Sound")]
        public readonly string PlayEvent;

        [Tooltip("The sound resource to play")]
        [DisplayName("Sound")]
        public readonly SoundResource SoundResource;

        [Tooltip(@"The minimum loudness the sound will be played at. If less than Maximum Loudness the sound will play each time at a randomly chosen loudness between the two.")]
        [DefaultValue(80.0f)]
        [Range(0.0f, 100.0f)]
        [DisplayName("Loudness")]
        public readonly float Loudness;

        [Tooltip("If not zero the pitch will be shifted on each play by a random amount between -PitchVariance and +PitchVariance, in semitones.")]
        [DefaultValue(0.0f)]
        [Range(0.0f, 24.0f)]
        [DisplayName("Pitch Variance")]
        public readonly float PitchVariance;

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

        protected override void SimpleInit()
        {
            if (SoundResource == null)
            {
                Log.Write(LogLevel.Error, __SimpleTag, "ObjectSound requires a SoundResource set to work properly.");
                return;
            }

            if (!ObjectPrivate.TryGetFirstComponent(out audio))
            {
                Log.Write(LogLevel.Error, __SimpleTag, "ObjectSound requires an emitter on the object to work properly.");
                return;
            }

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
            }
        }

        private void Unsubscribe(ScriptEventData sed)
        {
            if (subscription != null)
            {
                subscription();
                subscription = null;
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

        private void PlaySound(SoundResource sound)
        {
            PlaySettings playSettings = PlaySettings.PlayOnce;
            playSettings.Loudness = LoudnessPercentToDb(Loudness);
            playSettings.PitchShift = PitchVariance * RandomNegOneToOne();

            audio.PlaySoundOnComponent(sound, playSettings);
        }
    }
}
