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
// MoverExample1:
//   This script makes an object move away when you click on it.
// 

public class MoverExample1 : SceneObjectScript
{
    // Public properties

    [DefaultValue("Move away!")]
    public Interaction MyInteraction;

    [DefaultValue(5.0f)]
    public float Distance;

    [DefaultValue(2.0f)]
    public double Seconds;

    // Logic!

    public override void Init()
    {
        // Subscribe to the interaction, meaning this next block of code will be executed when the object is clicked on
        MyInteraction.Subscribe((InteractionData data) =>
        {
            // Get the agent that clicked on this object
            var agent = ScenePrivate.FindAgent(data.AgentId);

            if (agent != null)
            {
                if (ObjectPrivate.IsMovable)
                {
                    // Calculate the XY difference from the agent to the object
                    Vector toObject = ObjectPrivate.Position - ScenePrivate.FindObject(agent.AgentInfo.ObjectId).Position;
                    toObject.Z = 0.0f;

                    // If there is an XY difference in position
                    if (toObject.LengthSquared() > 0.0f)
                    {
                        // Normalize the direction vector
                        toObject = toObject.Normalized();

                        // Smoothly move the object away from the agent by the specified distance
                        ObjectPrivate.Mover.AddTranslate(ObjectPrivate.Position + toObject * Distance, Seconds, MoveMode.Smoothstep);
                    }
                }
                else
                {
                    ShowNotMovableReason(agent);
                }
            }
        });
    }

    void ShowNotMovableReason(AgentPrivate agent)
    {
        string message = $"The MoverExample1 script can't move the object with the name \"{ObjectPrivate.Name}\".\n";

        bool notMovableFromScript = (ObjectPrivate.Mover == null);
        if (notMovableFromScript)
        {
            message += "\n";
            message += "The \"Movable From Script\" property is OFF.\n";
        }

        RigidBodyComponent rb;
        bool notKeyframedRigidBody = (ObjectPrivate.TryGetFirstComponent(out rb) && (rb.GetMotionType() != RigidBodyMotionType.MotionTypeKeyframed));
        if (notKeyframedRigidBody)
        {
            message += "\n";
            message += "The object does not have the \"Keyframed\" motion type. Physics objects must be set to \"Keyframed\" to be moved by script.";
        }

        agent.Client.UI.ModalDialog.Show(message, "Ok", "");
    }
}
