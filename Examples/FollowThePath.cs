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

// Path Example will move a dynamic object along a route of points.
public class FollowThePath : SceneObjectScript
{
    #region Editor Properties
    // Higher Force_Factor will move the object quicker. Too high and the object may overshoot positions.
    [DefaultValue(10)]
    [Range(1.0, 40.0)]
    public readonly float Speed = 1;

    // There is no support for arrays or lists in the property editor.
    // Give each potential position its own variable, which we will make a list from.
    // Note: Only 10 total parameters can be shown in the editor for a script, giving us 6 points to pick, in addition to the object's initial position.
    [DisplayName("Position 1")]
    [EditorVisible]
    private Sansar.Vector Relative_Position_1 = Vector.Zero;
    [DisplayName("Position 2")]
    [EditorVisible]
    private Sansar.Vector Relative_Position_2 = Vector.Zero;
    [DisplayName("Position 3")]
    [EditorVisible]
    private Sansar.Vector Relative_Position_3 = Vector.Zero;
    [DisplayName("Position 4")]
    [EditorVisible]
    private Sansar.Vector Relative_Position_4 = Vector.Zero;
    [DisplayName("Position 5")]
    [EditorVisible]
    private Sansar.Vector Relative_Position_5 = Vector.Zero;
    [DisplayName("Position 6")]
    [EditorVisible]
    private Sansar.Vector Relative_Position_6 = Vector.Zero;
    [DisplayName("Position 7")]
    [EditorVisible]
    private Sansar.Vector Relative_Position_7 = Vector.Zero;

    // If not looping, after reaching the end it will warp to the start
    [DefaultValue(true)]
    [DisplayName("Looping")]
    public readonly bool Loop = false;

    // If set, the object will TP any user that collides with it to this position in the region.
    [DisplayName("Spawn Point")]
    public readonly Sansar.Vector On_Collide_TP_Dest;
    #endregion

    // These private members will not be shown on object properties.

    // The RigidBodyComponent is the interface for dynamic object interactions.
    private RigidBodyComponent RigidBody = null;

    // This will be our list of positions, built from the parameters and excluding any that are all zero.
    private System.Collections.Generic.List<Sansar.Vector> Positions = new System.Collections.Generic.List<Sansar.Vector>();

    // Override Init to set up event handlers and start coroutines.
    public override void Init()
    {
        // If the object had its components set in the editor they should now have the member initialized
        // [Editor support for Component types is not yet implemented]
        // The component can also be found dynamically
        if (RigidBody == null)
        {
            if (!ObjectPrivate.TryGetFirstComponent(out RigidBody))
            {
                // This example will only work on a dynamic object with a rigidbody. Bail if there isn't one.
                return;
            }
        }

        // This small Action only adds the position to the list if one of the components is not zero.
        // To set <0,0,0> as a position, set the W field on the position to any non-zero value.
        Action<Sansar.Vector> AddItem = position =>
        {
            if (position.X != 0 || position.Y != 0 || position.Z != 0 || position.W != 0)
            {
                Positions.Add(position + RigidBody.GetPosition());
            }
        };

        AddItem(new Sansar.Vector(0, 0, 0, 1));
        AddItem(Relative_Position_1);
        AddItem(Relative_Position_2);
        AddItem(Relative_Position_3);
        AddItem(Relative_Position_4);
        AddItem(Relative_Position_5);
        AddItem(Relative_Position_6);
        AddItem(Relative_Position_7);

        // Check that we got at least 2 good positions before doing anything.
        if (Positions.Count < 2)
        {
            Log.Write(Sansar.Script.LogLevel.Warning, "Not enough points set for dynamic object script. Set at least 1 position in the properties panel.");
            return;
        }

        // If there is a position set for On_Collide_TP_Dest then set up a collision coroutine to TP anyone that collides with the object.
        // To set <0,0,0> as the TP destination, set the W field on the position to any non-zero value.
        if (On_Collide_TP_Dest.X != 0 || On_Collide_TP_Dest.Y != 0 || On_Collide_TP_Dest.Z != 0 || On_Collide_TP_Dest.W != 0)
        {
            StartCoroutine(CollisionsCoroutine);
        }

        // Start up a coroutine to handle moving from position to position.
        StartCoroutine(Movement);
    }

    //     // The Math libraries are missing a vector4f magnitude. Used in Movement().
    //     private double GetMagnitude(Sansar.Vector vector)
    //     {
    //         return Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
    //     }


    // Push this object toward the next position.
    private void Movement()
    {
        // used to check if the object is close enough to the target position
        float targetDistanceSquared = (Speed / 10.0f)*(Speed / 10.0f);

        // The next_position is an index into the positions list
        int next_position = 0;
        while (true)
        {
            // Ensure a valid index first.
            if (next_position >= Positions.Count)
            {
                next_position = 0;
                if (!Loop)
                {   // If we aren't looping we snap to the first position and bump next_position so the object will move toward the second position.
                    WaitFor(RigidBody.SetPosition,Positions[next_position++]);
                }
            }

            // Get our target position and update index to point to the next position.
            Vector targetPos = Positions[next_position++];

            // Get a vector from our position to the target position
            Vector toTarget = targetPos - RigidBody.GetPosition();

            // Get the distance to our position and loop until we get "close enough"
            while (toTarget.LengthSquared() >= targetDistanceSquared)
            {
                try
                {
                    // Calculate a unit vector for the toTarget direction, then multiply by the desired speed.
                    WaitFor(RigidBody.SetLinearVelocity, toTarget.Normalized() * Speed);
                }
                catch (Exception)
                {   // If we got a bad force value it could cause an exception in SetLinearVelocity.
                    // Break out of this inner while to get the next position to work from.
                    break;
                }

                // update velocity 10 times per second.
                Wait(TimeSpan.FromSeconds(0.1f));

                // Update the vector to the target based on our new position.
                toTarget = targetPos - RigidBody.GetPosition();
            }
        }
    }

    // This coroutine will handle TPing anyone that collides with this object to a set position.
    private void CollisionsCoroutine()
    {
        while (true)
        {
            // Wait for a collision with a character
            CollisionData data = (CollisionData)WaitFor(RigidBody.Subscribe,Sansar.Simulation.CollisionEventType.CharacterContact);

            // Check that the collision is good.
            if (data.HitObject != null)
            {
                try
                {
                    // Get a full API for the object we hit, since we are a part of the scene.
                    ObjectPrivate objectprivate = ScenePrivate.FindObject(data.HitObject.ObjectId);
                    // AnimationComponents own the position of characters. To set a person's position we must do it through the animation component.
                    AnimationComponent anim;
                    if (objectprivate.TryGetFirstComponent(out anim))
                    {
                        // If there is an AnimationComponent use it to set the character's position.
                        anim.SetPosition(On_Collide_TP_Dest);
                    }
                }
                catch (System.Exception e)
                {
                    // If the person is no longer in the scene then a null reference exception is thrown: just ignore it.
                    if (!(e is System.NullReferenceException))
                    {
                        // Log other exceptions.
                        Log.Write("Got exception trying to set agent position: " + e.ToString());
                    }
                }
            }

            // Always wait after a collision before checking for more collisions to reduce duplicates
            Wait(TimeSpan.FromSeconds(0.2));
        }

    }
}