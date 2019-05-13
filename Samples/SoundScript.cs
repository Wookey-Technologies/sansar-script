// © 2019 Linden Research, Inc.

using Sansar.Script;
using Sansar.Simulation;

public class SoundScript : SceneObjectScript
{
    public SoundResource Sound;

    [DefaultValue(50.0f)]
    [Range(0.0f, 100.0f)]
    public float Loudness;

    public override void Init()
    {
        // Check to make sure a sound has been configured in the editor
        if (Sound == null)
        {
            Log.Write("SoundScript has no configured sound to play!");
            return;
        }

        ObjectPrivate.AddInteractionData addData = (ObjectPrivate.AddInteractionData)WaitFor(ObjectPrivate.AddInteraction, "Play sound", true);

        addData.Interaction.Subscribe((InteractionData data) =>
        {
            PlaySettings playSettings = PlaySettings.PlayOnce;
            playSettings.Loudness = (60.0f * (Loudness / 100.0f)) - 48.0f;  // Convert percentage to decibels (dB)

            ScenePrivate.PlaySound(Sound, playSettings);
        });
    }
}
