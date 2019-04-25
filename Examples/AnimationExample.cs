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

using Sansar.Simulation;
using Sansar.Script;
using System;

class AnimationExample : SceneObjectScript
{
    [DefaultValue(1.0f)]
    public readonly float PlaybackSpeed = 1.0f;
    public readonly bool PingPong = false;
    public readonly int StartFrame = 0;
    [DefaultValue(100)]
    public readonly int EndFrame = 100;

    private Animation anim;
    private AnimationParameters animParams;

    public override void Init()
    {
        AnimationComponent component;
        if (!ObjectPrivate.TryGetFirstComponent<AnimationComponent>(out component))
        {
            Log.Write("No animation component");
            return;
        }
        anim = component.DefaultAnimation;

        if (anim == null)
        {
            Log.Write("This animation component has no default animation");
            return;
        }
        animParams = anim.GetParameters();

        animParams.PlaybackSpeed = PlaybackSpeed;
        animParams.PlaybackMode = PingPong ? AnimationPlaybackMode.PingPong : AnimationPlaybackMode.Loop;

        //int totalFrameCount = anim.GetFrameCount();
        animParams.RangeStartFrame = StartFrame;
        animParams.RangeEndFrame = EndFrame;
        animParams.ClampToRange = true;

        WaitFor(anim.Play);
    }
}


