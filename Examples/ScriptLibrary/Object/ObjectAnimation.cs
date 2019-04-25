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
    [Tooltip("Controls object animations in response to simple script events.")]
    [DisplayName("Animation")]
    public class ObjectAnimation : ObjectBase
    {
        #region EditorProperties
        [Tooltip("Start playing on these events. Can be a comma separated list of event names.")]
        [DefaultValue("on")]
        [DisplayName("-> Play")]
        public readonly string PlayEvent;

        [Tooltip("Pause playing on these events. Can be a comma separated list of event names.")]
        [DefaultValue("off")]
        [DisplayName("-> Pause")]
        public readonly string PauseEvent;

        [Tooltip("Start playing Range A on these events. Can be a comma separated list of event names.")]
        [DefaultValue("play_a")]
        [DisplayName("-> Play Range A")]
        public readonly string PlayRangeAEvent;

        [Tooltip("The range for A, separated by a :. ex 0:10")]
        [DisplayName("Range A")]
        public readonly string RangeA;
        private int RangeAStart = 0;
        private int RangeAEnd = 0;

        [Tooltip("Start playing on these events. Can be a comma separated list of event names.")]
        [DefaultValue("play_b")]
        [DisplayName("-> Play Range B")]
        public readonly string PlayRangeBEvent;

        [Tooltip("The range for B\nStart and end frames separated by a :. ex 0:10")]
        [DisplayName("Range B")]
        public readonly string RangeB;
        private int RangeBStart = 0;
        private int RangeBEnd = 0;

        [Tooltip("Play events restart the animation")]
        [DisplayName("Reset On Play")]
        public readonly bool ResetOnPlay;

        [Tooltip("Enable responding to events for this script")]
        [DefaultValue("animation_enable")]
        [DisplayName("-> Enable")]
        public readonly string EnableEvent;

        [Tooltip("Disable responding to events for this script")]
        [DefaultValue("animation_disable")]
        [DisplayName("-> Disable")]
        public readonly string DisableEvent;
        #endregion



        private Sansar.Simulation.Animation animation = null;
        private Action unsubscribePlay = null;
        private Action unsubscribePause = null;
        private Action unsubscribeA = null;
        private Action unsubscribeB = null;
        private AnimationParameters initialAnimationParameters;

        protected override void SimpleInit()
        {
            AnimationComponent animComponent;
            if (!ObjectPrivate.TryGetFirstComponent<AnimationComponent>(out animComponent))
            {
                Log.Write(LogLevel.Error, "SimpleAnimation.Init", "Object must have an animation added at edit time for SimpleAnimation to work");
                return;
            }
            animation = animComponent.DefaultAnimation;

            if(!string.IsNullOrWhiteSpace(RangeA))
            {
                string[] frames = RangeA.Split(new char[] { ':', ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (frames.Length != 2
                    || !int.TryParse(frames[0].Trim(), out RangeAStart)
                    || !int.TryParse(frames[1].Trim(), out RangeAEnd))
                {
                    SimpleLog(LogLevel.Error, "Invalid RangeA parameter: '" + RangeA + "'. Should be two integers separated by a colon, ex 0:10");
                    RangeAStart = RangeAEnd = 0;
                }
            }

            if (!string.IsNullOrWhiteSpace(RangeA))
            {
                string[] frames = RangeB.Split(new char[] { ':', ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (frames.Length != 2
                    || !int.TryParse(frames[0].Trim(), out RangeBStart)
                    || !int.TryParse(frames[1].Trim(), out RangeBEnd))
                {
                    SimpleLog(LogLevel.Error, "Invalid RangeB parameter: '" + RangeB + "'. Should be two integers separated by a colon, ex 0:10");
                    RangeBStart = RangeBEnd = 0;
                }
            }

            initialAnimationParameters = animation.GetParameters();

            Subscribe(null);

            if (EnableEvent != "")
            {
                SubscribeToAll(EnableEvent, Subscribe);
            }

            if (DisableEvent != "")
            {
                SubscribeToAll(DisableEvent, Unsubscribe);
            }

        }

        private void Subscribe(ScriptEventData sed)
        {
            if (unsubscribePlay == null)
            {
                unsubscribePlay = SubscribeToAll(PlayEvent, (data) =>
                {
                    if (ResetOnPlay)
                    {
                        animation.JumpToFrame(initialAnimationParameters.PlaybackSpeed > 0.0f ? 0 : animation.GetFrameCount() - 1);
                    }
                    animation.Play(initialAnimationParameters);
                });
            }
            if (unsubscribePause == null)
            {
                unsubscribePause = SubscribeToAll(PauseEvent, (data) =>
                {
                    animation.Pause();
                });
            }
            if (unsubscribeA == null)
            {
                unsubscribeA = SubscribeToAll(PlayRangeAEvent, (data) =>
                {
                    AnimationParameters animationParameters = initialAnimationParameters;
                    animationParameters.PlaybackSpeed = Math.Abs(animationParameters.PlaybackSpeed) * Math.Sign(RangeAEnd - RangeAStart);
                    if (animationParameters.PlaybackSpeed > 0.0f)
                    {
                        animationParameters.RangeStartFrame = RangeAStart;
                        animationParameters.RangeEndFrame = RangeAEnd;
                    }
                    else
                    {
                    // Backwards playback uses negative playback speed but start frame still less than end frame
                    animationParameters.RangeStartFrame = RangeAEnd;
                        animationParameters.RangeEndFrame = RangeAStart;
                    }
                    animationParameters.ClampToRange = true;
                    if (ResetOnPlay)
                    {
                        animation.JumpToFrame(RangeAStart);
                    }
                    animation.Play(animationParameters);
                });
            }
            if (unsubscribeB == null)
            {
                unsubscribeB = SubscribeToAll(PlayRangeBEvent, (data) =>
                {
                    AnimationParameters animationParameters = initialAnimationParameters;
                    animationParameters.PlaybackSpeed = Math.Abs(animationParameters.PlaybackSpeed) * Math.Sign(RangeBEnd - RangeBStart);
                    if (animationParameters.PlaybackSpeed > 0.0f)
                    {
                        animationParameters.RangeStartFrame = RangeBStart;
                        animationParameters.RangeEndFrame = RangeBEnd;
                    }
                    else
                    {
                    // Backwards playback uses negative playback speed but start frame still less than end frame
                    animationParameters.RangeStartFrame = RangeBEnd;
                        animationParameters.RangeEndFrame = RangeBStart;
                    }
                    animationParameters.ClampToRange = true;
                    if (ResetOnPlay)
                    {
                        animation.JumpToFrame(RangeBStart);
                    }
                    animation.Play(animationParameters);
                });
            }
        }

        private void Unsubscribe(ScriptEventData sed)
        {
            if (unsubscribePlay != null)
            {
                unsubscribePlay();
                unsubscribePlay = null;
            }

            if (unsubscribePause != null)
            {
                unsubscribePause();
                unsubscribePause = null;
            }

            if (unsubscribeA != null)
            {
                unsubscribeA();
                unsubscribeA = null;
            }

            if (unsubscribeB != null)
            {
                unsubscribeB();
                unsubscribeB = null;
            }
        }
    }
}