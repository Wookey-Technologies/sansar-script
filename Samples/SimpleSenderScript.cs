// © 2019 Linden Research, Inc.

using Sansar.Script;
using Sansar.Simulation;

public class SimpleSenderScript : SceneObjectScript
{
    public interface ISimpleData
    {
        AgentInfo AgentInfo { get; }
        ObjectId ObjectId { get; }
        ObjectId SourceObjectId { get; }

        // Extra data
        Reflective ExtraData { get; }
    }

    public class SimpleData : Reflective, ISimpleData
    {
        public SimpleData(ScriptBase script) { ExtraData = script; }
        public AgentInfo AgentInfo { get; set; }
        public ObjectId ObjectId { get; set; }
        public ObjectId SourceObjectId { get; set; }

        public Reflective ExtraData { get; }
    }

    public override void Init()
    {
        ObjectPrivate.AddInteractionData addData = (ObjectPrivate.AddInteractionData)WaitFor(ObjectPrivate.AddInteraction, "Turn on", true);

        addData.Interaction.Subscribe((InteractionData data) =>
        {
            // Create the simple script message data payload
            SimpleData sd = new SimpleData(this);
            sd.ObjectId = ObjectPrivate.ObjectId;
            sd.SourceObjectId = ObjectPrivate.ObjectId;

            // Include the agent info for the avatar that triggered this event
            AgentPrivate agent = ScenePrivate.FindAgent(data.AgentId);
            if (agent != null)
            {
                sd.AgentInfo = agent.AgentInfo;
                sd.ObjectId = agent.AgentInfo.ObjectId;
            }

            // Send the "on" message with the SimpleData payload
            PostScriptEvent("on", sd);
        });
    }
}
