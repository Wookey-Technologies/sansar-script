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

// A simple interaction script that makes the object clickable while showing some data on hover.
public class InteractionExample : SceneObjectScript
{
    #region EditorProperties
    // This interaction will have a default prompt of "Click Me!"
    // The prompt can be changed in the object properties
    [DefaultValue("Click Me!")]
    public Interaction MyInteraction;
    #endregion

    // Track how many clicks
    int Clicks = 0;
    public override void Init()
    {
        // Save the initial prompt. It may have been changed at edit time, and will be changed more when interacted with.
        string basePrompt = MyInteraction.GetPrompt();

        // Subscribe to interaction to receive the interaction events
        MyInteraction.Subscribe( (InteractionData idata) =>
        {
            MyInteraction.SetPrompt(basePrompt + " " + (++Clicks)
                + "\nHit:" + idata.HitPosition.ToString("N2")                           // The position in world-space of the interaction
                + "\nFrom:" + idata.Origin.ToString("N2")                               // The position of the hand (VR) or camera (Desktop) that interacted
                + "\nBy:" + ScenePrivate.FindAgent(idata.AgentId).AgentInfo.Name);  // Find the name of the user who interacted
            // idata.Normal contains the object surface normal at the point the interaction happened.
            
            // Every 10 clicks disable the interaction for 3 seconds then reset the count and prompt.
            if (Clicks > 10)
            {
                MyInteraction.SetEnabled(false);
                Wait(TimeSpan.FromSeconds(3));
                Clicks = 0;
                MyInteraction.SetPrompt(basePrompt);
                MyInteraction.SetEnabled(true);
            }
        });
    }


}