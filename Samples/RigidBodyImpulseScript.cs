using Sansar;
using Sansar.Script;
using Sansar.Simulation;

public class RigidBodyImpulseScript : SceneObjectScript
{
    public Interaction MyInteraction;

    private RigidBodyComponent _rb;

    public override void Init()
    {
        if (ObjectPrivate.TryGetFirstComponent(out _rb))
        {
            _rb.SetCanGrab(false);  // Disable grabbing for this object

            if (_rb.GetMotionType() == RigidBodyMotionType.MotionTypeDynamic)
            {
                MyInteraction.Subscribe((InteractionData data) =>
                {
                    _rb.AddLinearImpulse(Vector.Up * 100.0f);
                });
            }
            else
                Log.Write(LogLevel.Warning, "RigidBodyImpulseScript not running on a dynamic object!");
        }
        else
            Log.Write(LogLevel.Warning, "RigidBodyImpulseScript not running on an object with a physics volume!");
    }
}
