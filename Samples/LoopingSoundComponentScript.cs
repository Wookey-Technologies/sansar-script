// © 2019 Linden Research, Inc.

using Sansar.Script;
using Sansar.Simulation;

public class LoopingSoundComponentScript : SceneObjectScript
{
    public SoundResource LoopingSound;

    [DefaultValue(80.0f)]
    [Range(0.0f, 100.0f)]
    public float Loudness;

    private AudioComponent _audio = null;
    private PlayHandle _playHandle = null;

    public override void Init()
    {
        // Check to make sure a sound has been configured in the editor
        if (LoopingSound == null)
        {
            Log.Write("LoopingSoundComponentScript has no configured sound to play!");
            return;
        }

        if (!ObjectPrivate.TryGetFirstComponent(out _audio))
        {
            Log.Write("LoopingSoundComponentScript is on an object that does not have an audio emitter.");
            return;
        }

        ObjectPrivate.AddInteractionData addData = (ObjectPrivate.AddInteractionData)WaitFor(ObjectPrivate.AddInteraction, "Play sound", true);

        addData.Interaction.Subscribe((InteractionData data) =>
        {
            // If not sound is playing, start one up
            if (_playHandle == null)
            {
                PlaySettings playSettings = PlaySettings.Looped;
                playSettings.Loudness = (60.0f * (Loudness / 100.0f)) - 48.0f;  // Convert percentage to decibels (dB)

                _playHandle = _audio.PlaySoundOnComponent(LoopingSound, playSettings);
            }
            // Else if a sound is playing, stop it
            else
            {
                if (_playHandle.IsPlaying())
                    _playHandle.Stop();

                _playHandle = null;
            }
        });
    }
}
