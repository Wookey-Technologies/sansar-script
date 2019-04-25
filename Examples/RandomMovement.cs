/* This content is licensed under the terms of the Creative Commons Attribution 4.0 International License.
 * When using this content, you must:
 * •    Acknowledge that the content is from the Sansar Knowledge Base.
 * •    Include our copyright notice: "© 2018 Linden Research, Inc."
 * •    Indicate that the content is licensed under the Creative Commons Attribution-Share Alike 4.0 International License.
 * •    Include the URL for, or link to, the license summary at https://creativecommons.org/licenses/by-sa/4.0/deed.hi (and, if possible, to the complete license terms at https://creativecommons.org/licenses/by-sa/4.0/legalcode.
 * For example:
 * "This work uses content from the Sansar Knowledge Base. © 2018 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."
 */
using Sansar.Simulation;
using System;
using Sansar.Script;
using Sansar;

namespace RandomMovement
{
    // Extensions in C# let you add methods to existing classes. It is mostly a convenience.
    // Read more here: https://msdn.microsoft.com/en-us//library/bb383977.aspx
    // In this case, the Math libraries are still very thin, so we add a few that we need here.
    static class Extensions
    {
        public static float Clamp(this float val, float min, float max)
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }
    }

    // RandomObjectExample will randomly move a dynamic object around its starting position.
    public class DynamicObjectExample : SceneObjectScript
    {
        // The RigidBodyComponent is the interface for dynamic object interactions.
        private RigidBodyComponent RigidBody = null;

        // This script will set Home to the initial position when the script loads, which is when the scene first starts.
        private Sansar.Vector Home;

        // Higher speeds mean faster objects - however it is mitigated by the mass of the object.
        [DefaultValue(10)]
        [Range(1.0, 100.0)]
        [DisplayName("Object Speed")]
        public readonly float Speed = 0;

        // From 0.0 to 1.0, affects how often a new direction is chosen.
        [DefaultValue(0.75)]
        [Range(0.0, 1.0)]
        [DisplayName("Randomness")]
        public readonly float Chaos = 0;

        // When the object is further than Range from Home it will be pushed back towards home.
        [DefaultValue(10)]
        [Range(1.0, 100.0)]
        [DisplayName("Approximate Range")]
        public readonly float Range = 10;

        // Override Init to set up event handlers and start coroutines.
        public override void Init()
        {
            // When unhandled exceptions occur, they may be caught with this event handler.
            // Certain exceptions may not be recoverable at all and may cause the script to
            // immediately be removed. This will also break out of any loops in coroutines!
            Script.UnhandledException += UnhandledException;

            // If the object had its components set in the editor they should now have the member initialized
            // [Editor support for Component types is not yet implemented]
            // The component can also be found dynamically
            if (RigidBody == null)
            {
                if (!ObjectPrivate.TryGetFirstComponent(out RigidBody))
                {
                    // Since object scripts are initialized when the scene loads, no one will actually see this message.
                    Log.Write("There is no RigidBodyComponent attached to this object.");
                    return;
                }
            }

            // Initialize the Home position
            Home = RigidBody.GetPosition();

            // StartCoroutine will queue CheckForCollisions to run
            // Arguments after the coroutine method are passed as arguments to the method.
            StartCoroutine(CheckForCollisions);

            // Clamp our chaos to [0,1]
            StartCoroutine(Movement, Speed, Chaos.Clamp(0.0f, 1.0f));

        }

        // Direction is mostly managed by Movement, but collisions can modify it too so that the object bounces away from what it collides with.
        private Sansar.Vector Direction = new Sansar.Vector();

        // This is a lot of complex math on choosing some "random" movement, keeping it near home and handling error cases.
        private void Movement(float speed, float chaos)
        {
            // Initialize a random number generator.
            Random rng = new Random();

            // Pick a direction to start with - mostly in the X/Y plane, but just a little push up.
            Direction = new Sansar.Vector((float)(0.5 - rng.NextDouble()), (float)(0.5 - rng.NextDouble()), 0.02f, 0.0f);

            // This will just continually try to move the object (the Wait at the bottom is important!)
            while (true)
            {
                // Calculate how far we are from our home point.
                float distance = (RigidBody.GetPosition() - Home).Length();

                // Pick a new direction based on Chaos level, or if we have wandered off too far
                if (rng.NextDouble() <= Chaos || distance > Range)
                {
                    // If we are far from home point us at home before adjusting the position.
                    if (distance > Range)
                    {
                        // Move toward the center.
                        Direction = (Home - RigidBody.GetPosition()).Normalized();

                        // Note: still letting the randomize adjust the heading.
                    }

                    // This is the most bogus direction adjusting logic you will see today.
                    Direction.X = Direction.X + (float)(0.5 - rng.NextDouble());
                    Direction.Y = Direction.Y + (float)(0.5 - rng.NextDouble());
                    Direction.Z = 0.02f;
                    Direction = Direction.Normalized();
                }

                // AddLinearImpulse can be picky on accepted values, especially if any math above breaks or the speed is set too high.
                // It will throw an exception if it doesn't like it. This will just skip 
                try
                {
                    Log.Write("PUSH! " + Direction.ToString() + " * " + speed + " => " + (Direction * speed).ToString());
                    RigidBody.AddLinearImpulse(Direction * speed);
                }
                catch (Exception e)
                {
                    Log.Write("Exception " + e.GetType().ToString() + " in AddLinearImpulse for value: " + (Direction * speed).ToString());

                    // That direction was bad, so lets choose a new one.
                    Direction.X = (float)(0.5 - rng.NextDouble());
                    Direction.Y = (float)(0.5 - rng.NextDouble());
                    Direction.Z = 0.02f;
                    Direction = Direction.Normalized();
                }

                // Wait after each push for between 0.5 and 1.5 seconds.
                Wait(TimeSpan.FromSeconds(0.5 + rng.NextDouble()));
            }
        }

        // Putting the collision event into a coroutine helps to reduce duplicate events
        // After every collision we give a small bump then sleep this coroutine before waiting on another collision.
        // An Action event handler would get a large queue of events for colliding with the same object which is harder to deal with.
        private void CheckForCollisions()
        {
            while (true)
            {
                // This will block the coroutine until a collision happens
                CollisionData data = (CollisionData)WaitFor(RigidBody.Subscribe, CollisionEventType.AllCollisions, Sansar.Script.ComponentId.Invalid);

                if (data.HitObject == null)
                {
                    // This is slightly more common, collided with something were no object was given from the physics system.
                    // Again, just sleep a little and continue.
                    Log.Write("Hit nothing? " + data.HitComponentId);
                    Wait(TimeSpan.FromSeconds(0.2));
                    continue;
                }

                // This position - collision object position gives a vector away from the object we collided with.
                // This is not "away from the collision point". Complex shapes or large objects will confuse this simple math.
                Sansar.Vector direction = ObjectPrivate.Position - data.HitObject.Position;
                direction = direction.Normalized();

                if (Math.Abs(direction.X) < 0.1 && Math.Abs(direction.Y) < 0.1)
                {
                    // This object is mostly above or below us: it is probably the floor.
                    // This is overly simplistic and will fail for large objects or sloped floors.
                    // Sleep a little and continue.
                    Wait(TimeSpan.FromSeconds(0.2));
                    continue;
                }

                // direction is now pointing _away_ from what we collided with, set it as our Direction for Movement.
                Direction = direction;

                try
                {
                    // Apply an immediate bump away from what we collided with.
                    Log.Write("Bump! " + Direction.ToString() + " * " + Speed + " => " + (Direction * Speed).ToString());
                    RigidBody.AddLinearImpulse(Direction * Speed);
                }
                catch (Exception e)
                {
                    Log.Write("Collision Exception " + e.GetType().ToString() + " in AddLinearImpulse for value: " + (Direction * Speed).ToString());
                }

                // Wait before checking for more collisions to avoid duplicate collisions and give a chance to separate from the other object.
                Wait(TimeSpan.FromSeconds(0.2));
            }
        }

        private void UnhandledException(object sender, Exception e)
        {
            // Depending on the script scheduling policy, the exception may or may not be recoverable.
            // An unrecoverable exception that can be handled will only be given a short time to run
            // so the handler method needs to be kept small.
            // Any exception thrown from this method will terminate the script regardless of the value
            // of UnhandledExceptionRecoverable
            if (!Script.UnhandledExceptionRecoverable)
            {
                Log.Write("Unrecoverable exception, this is useless.");
                Log.Write(e.ToString());
            }
            else
            {
                Log.Write("Exception!");
                Log.Write(e.ToString());

                // While we have recovered, whatever coroutine had the exception will have stopped.
                // The script will still run so logs can be recovered.
            }
        }
    }

}