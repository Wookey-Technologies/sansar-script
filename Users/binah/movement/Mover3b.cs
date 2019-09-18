/* This content is licensed under the terms of the Creative Commons Attribution 4.0 International License.
 * When using this content, you must:
 * �    Acknowledge that the content is from the Sansar Knowledge Base.
 * �    Include our copyright notice: "� 2018 Linden Research, Inc."
 * �    Indicate that the content is licensed under the Creative Commons Attribution-Share Alike 4.0 International License.
 * �    Include the URL for, or link to, the license summary at https://creativecommons.org/licenses/by-sa/4.0/deed.hi (and, if possible, to the complete license terms at https://creativecommons.org/licenses/by-sa/4.0/legalcode.
 * For example:
 * "This work uses content from the Sansar Knowledge Base. � 2018 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."
 */
using Sansar;
using Sansar.Script;
using Sansar.Simulation;
using System;
//
// MoverExample3B:  aka, predictable scared robot
//   This script makes a robot turn 90 degrees to the left and flee away from anyone who clicks on it.
//
public class FromAngleAxisInteraction : SceneObjectScript
{
    // Public properties
    [DefaultValue("Get away!")]
    public Interaction MyInteraction;
    [DefaultValue(5.0f)]
    public float FleeDistance;
    [DefaultValue(2.0f)]
    public double FleeSeconds;
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

                Log.Write("Object position minus agent position Vector " + fromAgentToObject);
                
                // Remove any vertical difference
                fromAgentToObject.Z = 0.0f;
                //Log.Write("magnitude of the vector, length squared " + fromAgentToObject.LengthSquared());

                if (fromAgentToObject.LengthSquared() > 0.0f)
                {
                    fromAgentToObject = fromAgentToObject.Normalized();
                    Log.Write("fromAgentToObject.Normalized() " + fromAgentToObject);

                    // Disable interaction
                    MyInteraction.SetEnabled(false);
                    
                    // Clear any other queued movement
                    ObjectPrivate.Mover.StopAndClear();
                    
                    // rotation and translation
                    Quaternion rot = ObjectPrivate.Rotation;
                    Quaternion stepRot = Quaternion.FromEulerAngles(new Vector(0,0,(float)Math.PI/2f));
                    rot *= stepRot;
                    ObjectPrivate.Mover.AddRotate(rot);
                    ObjectPrivate.Mover.AddTranslate(ObjectPrivate.Position + Vector.ObjectLeft.Rotate(rot), 2.0, MoveMode.Linear);

                    // Enable interaction
                    MyInteraction.SetEnabled(true);
                }
            }
        });
    }
}