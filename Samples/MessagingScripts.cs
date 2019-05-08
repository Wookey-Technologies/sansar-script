// © 2019 Linden Research, Inc.

using Sansar;
using Sansar.Script;
using Sansar.Simulation;

namespace MessagingScripts
{
    public class SendMessageScript : SceneObjectScript
    {
        public override void Init()
        {
            ObjectPrivate.AddInteractionData addData = (ObjectPrivate.AddInteractionData)WaitFor(ObjectPrivate.AddInteraction, "Turn off", true);

            addData.Interaction.Subscribe((InteractionData data) =>
            {
                // Send the "button_pressed" message
                PostScriptEvent("button_pressed");
            });
        }
    }

    public class ReceiveMessageScript : SceneObjectScript
    {
        private LightComponent _light;

        public override void Init()
        {
            if (!ObjectPrivate.TryGetFirstComponent(out _light))
                Log.Write("ReceiveMessageScript couldn't find light!");
            else if (!_light.IsScriptable)
                Log.Write("ReceiveMessageScript couldn't find scriptable light!'");
            else
            {
                // Listen for the "button_pressed" message
                SubscribeToScriptEvent("button_pressed", (ScriptEventData data) =>
                {
                    // Turn off the light
                    _light.SetColorAndIntensity(Color.Black, 0.0f);
                });
            }
        }
    }
}
