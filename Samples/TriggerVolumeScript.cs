using Sansar;
using Sansar.Script;
using Sansar.Simulation;

public class TriggerVolumeScript : SceneObjectScript
{
    public override void Init()
    {
        RigidBodyComponent rigidBody;

        if (ObjectPrivate.TryGetFirstComponent(out rigidBody) && rigidBody.IsTriggerVolume())
            rigidBody.Subscribe(CollisionEventType.Trigger, OnTrigger);
        else
            Log.Write(LogLevel.Warning, "TriggerVolumeScript not running on a trigger volume!");
    }

    void OnTrigger(CollisionData data)
    {
        AgentPrivate agent = ScenePrivate.FindAgent(data.HitComponentId.ObjectId);

        if (agent != null)
        {
            if (data.Phase == CollisionEventPhase.TriggerEnter)
                agent.SendChat("Agent entered trigger volume!");
            else if (data.Phase == CollisionEventPhase.TriggerExit)
                agent.SendChat("Agent exited trigger volume!");
        }
    }
}
