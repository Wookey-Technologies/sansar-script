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
// MoverExample2:
//   This script makes an object spin randomly or do 360's depending on settings.
//

public class MoverExample2 : ObjectScript
{
    // Public properties

    [Tooltip("Turns per second")]
    [DefaultValue(1.0f)]
    public float SpinSpeed;

    [Tooltip("Set this to OFF to do 360's instead of random turns")]
    [DefaultValue(true)]
    public bool SpinRandomly;

    // Logic!

    public override void Init()
    {
        // No spinning requested so don't do anything further
        if (SpinSpeed == 0.0f)
        {
            return;
        }

        // Write an error to the debug console if the object is not movable
        // (The debug console is visible to the creator with the Ctrl+D keyboard shortcut)
        if (!ObjectPrivate.IsMovable)
        {
            Log.Write($"MoverExample2 script can't move {ObjectPrivate.Name} because either the 'Movable from Script' flag was not set or the object does not have 'Keyframed' physics!");
            return;
        }

        StartCoroutine(Spin);
    }

    void Spin()
    {
        // If this script is in random spin mode
        if (SpinRandomly)
        {
            Random rng = new Random();

            while (true)
            {
                // Calculate a random rotation between 90 degrees and 180 degrees
                float rotationAngle = (float) (Mathf.PiOverTwo + rng.NextDouble() * Mathf.PiOverTwo);
                rotationAngle *= (rng.NextDouble() > 0.5 ? 1.0f : -1.0f);

                // Calculate the time for this rotation based on the specified spin speed
                double timeForRotation = Math.Abs(rotationAngle / Mathf.TwoPi) / SpinSpeed;

                // Compute a quaternion to rotate the object around the up axis by the specified angle
                Quaternion rotation = Quaternion.FromAngleAxis(rotationAngle, Vector.ObjectUp);

                // Do the actual rotation, and wait for it to complete
                WaitFor(ObjectPrivate.Mover.AddRotate, rotation, timeForRotation, MoveMode.Smoothstep);
            }
        }
        else
        {
            // Calculate time for one rotation
            float timeForOneRotation = 1.0f / SpinSpeed;

            // Use this to keep track of the spin direction, and to choose whether to start in one direction or the other based on the sign of the spin speed
            float nextSpinSign = Math.Sign(SpinSpeed);

            while (true)
            {
                //
                // In this mode the script wants to turn the entire object all the way around.  If we simply request a rotation to that final rotation,
                // the object will not move at all since it is already in that rotation.  So we need to specify a few rotations around the circle to get
                // it to spin.
                //
                // This could be done using the linear move mode to move it one third of the way around, three times like so:
                //
                // ObjectPrivate.Mover.AddRotate(Quaternion.FromAngleAxis(Mathf.TwoPi * 1.0f / 3.0f, Vector.Up), timeForOneRotation / 3.0f, MoveMode.Linear);
                // ObjectPrivate.Mover.AddRotate(Quaternion.FromAngleAxis(Mathf.TwoPi * 2.0f / 3.0f, Vector.Up), timeForOneRotation / 3.0f, MoveMode.Linear);
                // WaitFor(ObjectPrivate.Mover.AddRotate,      Quaternion.FromAngleAxis(Mathf.TwoPi, Vector.Up), timeForOneRotation / 3.0f, MoveMode.Linear);
                //
                // That will be fine for constant motion in a single direction but since we want this object to stop and change directions, we need to do
                // something a bit more sophisticated so that the motion is not too jarring.
                //
                // If we were able to do this with a single rotation like the SpinRandomly case above, we could use a single Smoothstep rotation.  But since
                // we need to split it into separate rotations and we want it to take a precise amount of time, we need to carefully choose the durations and
                // rotations to line up the ease-in, linear and ease-out phases of this rotation.
                //
                // To do this we need to understand the math behind the ease-in and ease-out move modes as shown in the documentation.  The ease-in and
                // ease-out modes are proportional to t^2 so we have to find a way to line up the velocity of a constant linear rotation with the t^2 ease-in
                // and ease-out.  This ends up working out to a one third to two thirds relationship in terms of distance over the same amount of time.
                // Meaning we can spin one third of half a rotation using ease-in, then the remaining two thirds of the half rotation in linear.
                // Then we simply invert the second half, doing two thirds of the second half rotation linearly and the final one third as an ease-out.
                //
                // Even if you don't fully understand that, you can see how it works below.  All of the rotations are queue'd up even though we call them
                // one after the other and then we just wait on the final one to finish before switching directions and spinning the other way.

                ObjectPrivate.Mover.AddRotate(Quaternion.FromAngleAxis(nextSpinSign * Mathf.TwoPi * 1.0f / 6.0f, Vector.ObjectUp), timeForOneRotation * 0.25, MoveMode.EaseIn);
                ObjectPrivate.Mover.AddRotate(Quaternion.FromAngleAxis(nextSpinSign * Mathf.TwoPi * 3.0f / 6.0f, Vector.ObjectUp), timeForOneRotation * 0.25, MoveMode.Linear);
                ObjectPrivate.Mover.AddRotate(Quaternion.FromAngleAxis(nextSpinSign * Mathf.TwoPi * 5.0f / 6.0f, Vector.ObjectUp), timeForOneRotation * 0.25, MoveMode.Linear);
                WaitFor(ObjectPrivate.Mover.AddRotate,      Quaternion.FromAngleAxis(nextSpinSign * Mathf.TwoPi, Vector.ObjectUp), timeForOneRotation * 0.25, MoveMode.EaseOut);

                nextSpinSign *= -1.0f;
            }
        }
    }
}
