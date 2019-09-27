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

class AgentGravityExample : SceneObjectScript
{
    [DisplayName("Gravity Factor")]
    [DefaultValue(0.1f)]
    public readonly float gravityFactor = 0.1f;

    private RigidBodyComponent RigidBody = null;

    public override void Init()
    {
        if (ObjectPrivate.TryGetFirstComponent(out RigidBody) && RigidBody.IsTriggerVolume())
        {
            RigidBody.Subscribe(CollisionEventType.Trigger, OnTrigger);
        }
        else
        {
            Log.Write(LogLevel.Error, "Could not start " + GetType().Name + " because no trigger volume was found.");
        }
    }

    private void OnTrigger(CollisionData data)
    {
        try
        {
            AgentPrivate agent = ScenePrivate.FindAgent(data.HitComponentId.ObjectId);
            if (data.Phase == CollisionEventPhase.TriggerExit)
            {
                agent.SetGravityFactor(1.0f);
            }
            else if (data.Phase == CollisionEventPhase.TriggerEnter)
            {
                agent.SetGravityFactor(gravityFactor);
            }
        }
        catch (NullReferenceException nre) { Log.Write(LogLevel.Info, "NullReferenceException setting agent gravity factor (maybe the user left): " + nre.Message); }
        catch (Exception e) { Log.Write(LogLevel.Error, "Exception setting agent gravity factor: " + e.Message); }
    }
}


