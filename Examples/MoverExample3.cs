/* This content is licensed under the terms of the Creative Commons Attribution 4.0 International License.
 * When using this content, you must:
 * •    Acknowledge that the content is from the Sansar Knowledge Base.
 * •    Include our copyright notice: "© 2018 Linden Research, Inc."
 * •    Indicate that the content is licensed under the Creative Commons Attribution-Share Alike 4.0 International License.
 * •    Include the URL for, or link to, the license summary at https://creativecommons.org/licenses/by-sa/4.0/deed.hi (and, if possible, to the complete license terms at https://creativecommons.org/licenses/by-sa/4.0/legalcode.
 * For example:
 * "This work uses content from the Sansar Knowledge Base. © 2018 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."
 */
using Sansar;
using Sansar.Script;
using Sansar.Simulation;
using System;

//
// MoverExample3:  aka, spinning scared robot
//   This script makes a robot spin randomly and flee away from anyone who clicks on it.
//

public class MoverExample3 : SceneObjectScript
{
    // Public properties

    [DefaultValue("Get away!")]
    public Interaction MyInteraction;

    [DefaultValue(5.0f)]
    public float FleeDistance;

    [DefaultValue(2.0f)]
    public double FleeSeconds;

    [Tooltip("Turns per second")]
    [DefaultValue(1.0f)]
    public float SpinSpeed;

    // Logic!

    public override void Init()
    {
        // Write an error to the debug console if the object is not movable
        // (The debug console is visible to the creator with the Ctrl+D keyboard shortcut)
        if (!ObjectPrivate.IsMovable)
        {
            Log.Write($"MoverExample3 script can't move {ObjectPrivate.Name} because either the 'Movable from Script' flag was not set or the object does not have 'Keyframed' physics!");
            return;
        }

        MyInteraction.Subscribe((InteractionData data) =>
        {
            // Find the agent that initiated the interaction
            var agent = ScenePrivate.FindAgent(data.AgentId);

            if (agent != null)
            {
                Vector fromAgentToObject = ObjectPrivate.Position - ScenePrivate.FindObject(agent.AgentInfo.ObjectId).Position;

                // Remove any vertical difference
                fromAgentToObject.Z = 0.0f;

                if (fromAgentToObject.LengthSquared() > 0.0f)
                {
                    fromAgentToObject = fromAgentToObject.Normalized();

                    // Disable interaction
                    MyInteraction.SetEnabled(false);

                    // Clear any other queued movement
                    ObjectPrivate.Mover.StopAndClear();

                    // Flee away from the agent
                    WaitFor(ObjectPrivate.Mover.AddTranslate, ObjectPrivate.Position + fromAgentToObject * FleeDistance, FleeSeconds, MoveMode.EaseOut);

                    // Enable interaction
                    MyInteraction.SetEnabled(true);
                }
            }
        });

        // Start spinning
        if (SpinSpeed > 0.0f)
        {
            StartCoroutine(Spin);
        }
    }

    void Spin()
    {
        TimeSpan ts = TimeSpan.FromSeconds(0.1);

        Random rng = new Random();

        float spinSign = 1.0f;

        while (true)
        {
            // Don't do anything until other move commands are finished
            if (ObjectPrivate.Mover.IsMoving)
            {
                Wait(ts);
                continue;
            }

            // Swap spin directions
            spinSign *= -1.0f;

            // Calculate random rotation up to one half turn from the current orientation around the up axis
            float rotationAngle = (float) (Mathf.PiOverTwo + rng.NextDouble() * Mathf.PiOverTwo);
            var randomRotation = Quaternion.FromAngleAxis(spinSign * rotationAngle, Vector.ObjectUp) * ObjectPrivate.Rotation;

            // Calculate the time for this rotation based on the specified spin speed
            double timeForRotation = (rotationAngle / Mathf.TwoPi) / SpinSpeed;

            // Rotate and wait
            WaitFor(ObjectPrivate.Mover.AddRotate, randomRotation.Normalized(), timeForRotation, MoveMode.Smoothstep);
        }
    }
}
