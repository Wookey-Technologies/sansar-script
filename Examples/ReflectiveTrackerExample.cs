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

namespace ReflectiveExample
{
    // ReflectiveDetectorExample can be placed on multiple objects to detect collisions between the scripted object and characters
    // On collisions it will report the collision to a central ReflectiveTrackerExample which accumulates hits and then report the total to the user.
    // This enables a shared total of hits across all scripted objects.
    // Setup:
    // 1) Upload and place ReflectiveTrackerExample.cs on only 1 object in the scene.
    // 2) Upload and place ReflectiveDetectorExample.cs on as many obstacles or targets in the scene as desired.
    // Use:
    // Build and visit the scene. Bump into several colliders. The reported hit targets should increment smoothly regardless of which targets are hit in which order.

    // This is the system library script, auto-register it so it is available immediately to other scripts in the scene.
    [RegisterReflective]
    public class Tracker : SceneObjectScript
    {
        // Nothing to init here.
        public override void Init()
        {
        }

        // The dictionary of Ids to hit counts. Note that old data is not removed.
        System.Collections.Generic.Dictionary<System.Guid, int> Collisions = new System.Collections.Generic.Dictionary<System.Guid, int>();

        // Track collisions and return the total number of collisions for this user.
        public int RecordCollision(System.Guid user)
        {
            int hits = 0;
            Collisions.TryGetValue(user, out hits);
            ++hits;
            Collisions[user] = hits;
            return hits;
        }
    }
}