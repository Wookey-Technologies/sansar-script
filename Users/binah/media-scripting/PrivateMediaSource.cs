using Sansar.Script;
using Sansar.Simulation;

public class PrivateMediaSourceScript : SceneObjectScript
{
    [DefaultValue("https://www.sansar.com/")]
    public string PublicMedia;

    [DefaultValue("https://atlas.sansar.com/experiences/sansar-studios/")]
    public string PrivateMedia;

    public override void Init()
    {
        // Set the public media source for the scene
        ScenePrivate.OverrideMediaSource(PublicMedia);

        // Override the media source to the private media source for each user that clicks on this object

        ObjectPrivate.AddInteractionData addData = (ObjectPrivate.AddInteractionData) WaitFor(ObjectPrivate.AddInteraction, "Show media", true);

        addData.Interaction.Subscribe((InteractionData data) =>
        {
            AgentPrivate agent = ScenePrivate.FindAgent(data.AgentId);

            if (agent != null)
            {
                agent.OverrideMediaSource(PrivateMedia);
            }
        });
    }
}