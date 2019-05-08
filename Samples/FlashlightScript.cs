using Sansar;
using Sansar.Script;
using Sansar.Simulation;

public class FlashlightScript : SceneObjectScript
{
    LightComponent _light = null;
    RigidBodyComponent _rb = null;
    IEventSubscription _commandSubscription = null;

    public override void Init()
    {
        if (!ObjectPrivate.TryGetFirstComponent(out _rb))
        {
            Log.Write(LogLevel.Error, "FlashlightScript can't find a RigidBodyComponent!");
            return;
        }

        if (!ObjectPrivate.TryGetFirstComponent(out _light) || _light.IsScriptable)
        {

        }

        _rb.SubscribeToHeldObject(HeldObjectEventType.Grab, (HeldObjectData holdData) =>
        {
            AgentPrivate agent = ScenePrivate.FindAgent(holdData.HeldObjectInfo.SessionId);

            if (agent != null && agent.IsValid)
            {
                _commandSubscription = agent.Client.SubscribeToCommand("PrimaryAction", CommandAction.Pressed, (CommandData command) =>
                {
                    // Do 
                },
                (canceledData) => { });
            }
        }

        _rb.SubscribeToHeldObject(HeldObjectEventType.Release, (HeldObjectData holdData) =>
        {
            if (_commandSubscription != null)
            {
                _commandSubscription.Unsubscribe();
                _commandSubscription = null;
            }
        });
    }
}
