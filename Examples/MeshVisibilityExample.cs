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

class MeshVisibilityExample : SceneObjectScript
{
    [DefaultValue(0.25)]
    public readonly double FlashDelay = 0.25;

    private MeshComponent component;

    public override void Init()
    {
        if (!ObjectPrivate.TryGetFirstComponent<MeshComponent>(out component))
        {
            Log.Write("No mesh component found!");
            return;
        }

        Log.Write("Found mesh component");

        if (component.IsScriptable)
        {
            StartCoroutine(FlashVisibility, FlashDelay);
        }
        else
        {
            Log.Write("Can't change visibility of non-scriptable mesh!");
        }
    }

    private void FlashVisibility(double seconds)
    {
        // This coroutine will run for the lifetime of the script.
        while (true)
        {
            Wait(TimeSpan.FromSeconds(seconds));

            component.SetIsVisible(!component.GetIsVisible());
        }
    }
}


