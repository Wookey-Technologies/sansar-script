using Sansar.Script;
using Sansar.Simulation;

[RegisterReflective]
public class ReflectiveReceiverScript : SceneObjectScript
{
    public Interaction Button;

    public override void Init() { }

    public void SetButtonEnabled(bool enabled)
    {
        Button.SetEnabled(enabled);
    }
}
