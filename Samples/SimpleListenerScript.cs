// © 2019 Linden Research, Inc.

using Sansar.Script;
using Sansar.Simulation;

public class SimpleListenerScript : SceneObjectScript
{
    public interface ISimpleData
    {
        AgentInfo AgentInfo { get; }
        ObjectId ObjectId { get; }
        ObjectId SourceObjectId { get; }

        // Extra data
        Reflective ExtraData { get; }
    }

    public override void Init()
    {
        // Listen for the 'on' message
        SubscribeToScriptEvent("on", (ScriptEventData data) =>
        {
            ISimpleData idata = data.Data.AsInterface<ISimpleData>();
            if (idata == null)
            {
                ScenePrivate.Chat.MessageAllUsers("The 'on' message does not have a simple script payload!");
            }
            else
            {
                ObjectPrivate obj = ScenePrivate.FindObject(idata.ObjectId);
                ScenePrivate.Chat.MessageAllUsers("The 'on' message simple script payload came from " + obj.Name);
            }
        });
    }
}
