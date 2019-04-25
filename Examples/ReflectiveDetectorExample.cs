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
using System.Linq;

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
    public class Detector : SceneObjectScript
    {
        // ITracker is the interface the shared tracker implements. Note that ReflectiveTrackerExample does not actually extend this interface
        // Reflective will generate an object a matching object as long as the target has compatible methods and properties.
        public interface ITracker
        {
            // Record a collision and return the total number of collisions for the user.
            int RecordCollision(System.Guid id);
        }

        private ITracker Tracker;
        public override void Init()
        {
            // Find the instance of the tracker. FindReflective has a type for the interface to match and takes as a string the class name of the other object.
            // In this case we want to find a script in the scene whose class is named "ReflectiveTrackerExample" and which implements the ITracker interface.
            // Because ReflectiveTrackerExample is tagged with [RegisterReflective] it should be registered before this init is called.
            // FindReflective returns an enumerable in case there are multiple scripts that match the criteria. In this case we expect there to be just one.
            Tracker = ScenePrivate.FindReflective<ITracker>("ReflectiveExample.Tracker").FirstOrDefault();

            // Error out if the tracker is not found.
            if (Tracker == null)
            {
                Log.Write(LogLevel.Error, "Could not find the Tracker.");
                return;
            }

            // Set up a collision handler for colliding with characters
            RigidBodyComponent Rigidbody;
            if (ObjectPrivate.TryGetFirstComponent(out Rigidbody))
            {
                Rigidbody.Subscribe(CollisionEventType.CharacterContact, OnHit);
            }
        }

        // Simple collision handler just records the collision with the Tracker and reports the total number of collisions.
        void OnHit(CollisionData data)
        {
            AgentPrivate agent = ScenePrivate.FindAgent(data.HitComponentId.ObjectId);
            if (agent != null)
            {
                // This will record a new collision in the Tracker script and return the new total.
                int hits = Tracker.RecordCollision(agent.AgentInfo.AvatarUuid);
                agent.SendChat($"You have hit {hits} targets.");
            }
        }

    }
}