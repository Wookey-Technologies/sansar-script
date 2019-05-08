using Sansar;
using Sansar.Script;
using Sansar.Simulation;

public class HeldObjectCommandScript : SceneObjectScript
{
    RigidBodyComponent _rb = null;
    IEventSubscription _commandSubscription = null;

    public override void Init()
    {
        if (!ObjectPrivate.TryGetFirstComponent(out _rb))
        {
            Log.Write(LogLevel.Error, "HeldObjectCommandScript can't find a RigidBodyComponent!");
            return;
        }

        if (!_rb.GetCanGrab())
        {
            Log.Write(LogLevel.Error, "HeldObjectCommandScript isn't on a grabbable object!");
            return;
        }

        // Subscribe to the "PrimaryAction" action when the object is picked up
        _rb.SubscribeToHeldObject(HeldObjectEventType.Grab, (HeldObjectData holdData) =>
        {
            AgentPrivate agent = ScenePrivate.FindAgent(holdData.HeldObjectInfo.SessionId);

            if (agent != null && agent.IsValid)
            {
                _commandSubscription = agent.Client.SubscribeToCommand("PrimaryAction", CommandAction.Pressed, (CommandData command) =>
                {
                    Log.Write(agent.AgentInfo.Name + " pressed the button while holding the object!");
                },
                (canceledData) => { });
            }
        });

        // Unsubscribe from the "PrimaryAction" action when the object is dropped
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
