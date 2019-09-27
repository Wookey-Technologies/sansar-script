/* This content is licensed under the terms of the Creative Commons Attribution 4.0 International License.
 * When using this content, you must:
 * •    Acknowledge that the content is from the Sansar Knowledge Base.
 * •    Include our copyright notice: "© 2017 Linden Research, Inc."
 * •    Indicate that the content is licensed under the Creative Commons Attribution-Share Alike 4.0 International License.
 * •    Include the URL for, or link to, the license summary at https://creativecommons.org/licenses/by-sa/4.0/deed.hi (and, if possible, to the complete license terms at https://creativecommons.org/licenses/by-sa/4.0/legalcode.
 * For example:
 * "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."
 */

using Sansar.Script;
using Sansar.Simulation;
using System;
using System.Linq;

namespace ScriptLibrary
{
    [Tooltip("Sends simple script events in response to interactions.")]
    [DisplayName("Interaction")]
    [DefaultScript]
    public class InteractionManager : LibraryBase
    {
        #region EditorProperties
        [Tooltip(@"The interaction for this object, with a default prompt of ""Click Me!""")]
        [DefaultValue("Click Me!")]
        [DisplayName("Interaction prompt")]
        public readonly Sansar.Simulation.Interaction MyInteraction;

        [Tooltip(@"The events to send when the object is clicked. Can be a comma separated list of event names.
To send different events on subsequent clicks use the >> sequence operator like: on>>off.")]
        [DefaultValue("on>>off")]
        [DisplayName("This Click ->")]
        public readonly string OnClick;

        [Tooltip("Maximum number of events per second that will trigger events. Set to 0 for no limit.")]
        [DefaultValue(10)]
        [DisplayName("Max Events Per Second")]
        [Range(0,100)]
        public readonly float MaxEventsPerSecond = 10;

        [Tooltip("If true this interaction will disable itself for the user that clicked it after they click it once.\nOther users will still be able to click it and leaving then returning to the scene will reset the state of the interaction. Disabling then enabling the interaction will reset the state for everyone in the scene.")]
        [DefaultValue(false)]
        [DisplayName("Single Use")]
        public readonly bool DisableOnClick;

        [Tooltip("Reset this script so the next click will be 1st click.")]
        [DefaultValue("interaction_reset")]
        [DisplayName("-> Reset")]
        public readonly string ResetEvent;

        [Tooltip("Advance to the next state, if sequences '>>' are used, as if the button was clicked but without sending the events.")]
        [DefaultValue("interaction_next")]
        [DisplayName("-> Next")]
        public readonly string NextEvent;

        [Tooltip(@"Set to a volume to add an Interaction to the object the volume is on.
Only one interaction can work on a single object.
All Interactions set on this script will follow the same configuration.")]
        [DisplayName("Volume1")]
        public readonly RigidBodyComponent Volume1;

        [Tooltip(@"The interaction prompt for Volume 1.")]
        [DefaultValue("Click 1!")]
        [DisplayName("Volume 1 prompt")]
        public readonly string Volume1Prompt;

        [Tooltip(@"The events to send when Volume1 is clicked. Can be a comma separated list of event names.
To send different events on subsequent clicks use the >> sequence operator like: on>>off.")]
        [DefaultValue("on>>off")]
        [DisplayName("Volume 1 ->")]
        public readonly string Volume1OnClick;

        [Tooltip("Reset Volume 1 so the next click will be 1st click.")]
        [DefaultValue("interaction_reset")]
        [DisplayName("-> Reset 1")]
        public readonly string Reset1;

        [Tooltip("Advance Volume 1 to the next state, if sequences '>>' are used, as if the button was clicked but without sending the events.")]
        [DefaultValue("interaction_next")]
        [DisplayName("-> Next 1")]
        public readonly string Next1;

        [Tooltip(@"Set to a volume to add an Interaction to the object the volume is on.
Only one interaction can work on a single object.
All Interactions set on this script will follow the same configuration.")]
        [DisplayName("Volume 2")]
        public readonly RigidBodyComponent Volume2;

        [Tooltip(@"The interaction prompt for Volume 2.")]
        [DefaultValue("Click 2!")]
        [DisplayName("Volume 2 prompt")]
        public readonly string Volume2Prompt;

        [Tooltip(@"The events to send when Volume 2 is clicked. Can be a comma separated list of event names.
To send different events on subsequent clicks use the >> sequence operator like: on>>off.")]
        [DefaultValue("on>>off")]
        [DisplayName("Volume 2 ->")]
        public readonly string Volume2OnClick;

        [Tooltip("Reset Volume 2 so the next click will be 1st click.")]
        [DefaultValue("interaction_reset")]
        [DisplayName("-> Reset 2")]
        public readonly string Reset2;

        [Tooltip("Advance Volume 2 to the next state, if sequences '>>' are used, as if the button was clicked but without sending the events.")]
        [DefaultValue("interaction_next")]
        [DisplayName("-> Next 2")]
        public readonly string Next2;

        [Tooltip(@"Set to a volume to add an Interaction to the object the volume is on.
Only one interaction can work on a single object.
All Interactions set on this script will follow the same configuration.")]
        [DisplayName("Volume 3")]
        public readonly RigidBodyComponent Volume3;

        [Tooltip(@"The interaction prompt for Volume 3.")]
        [DefaultValue("Click 3!")]
        [DisplayName("Volume 3 prompt")]
        public readonly string Volume3Prompt;

        [Tooltip(@"The events to send when Volume 3 is clicked. Can be a comma separated list of event names.
To send different events on subsequent clicks use the >> sequence operator like: on>>off.")]
        [DefaultValue("on>>off")]
        [DisplayName("Volume 3 ->")]
        public readonly string Volume3OnClick;

        [Tooltip("Reset Volume 3 so the next click will be 1st click.")]
        [DefaultValue("interaction_reset")]
        [DisplayName("-> Reset 3")]
        public readonly string Reset3;

        [Tooltip("Advance Volume 3 to the next state, if sequences '>>' are used, as if the button was clicked but without sending the events.")]
        [DefaultValue("interaction_next")]
        [DisplayName("-> Next 3")]
        public readonly string Next3;

        [Tooltip("Enable responding to events for this script")]
        [DefaultValue("interaction_enable")]
        [DisplayName("-> Enable")]
        public readonly string EnableEvent;

        [Tooltip("Disable responding to events for this script")]
        [DefaultValue("interaction_disable")]
        [DisplayName("-> Disable")]
        public readonly string DisableEvent;

        [Tooltip(@"If StartEnabled is true then the script will respond to interactions when the scene is loaded
If StartEnabled is false then the script will not respond to interactions until an (-> Enable) event is received.")]
        [DefaultValue(true)]
        [DisplayName("Start Enabled")]
        public readonly bool StartEnabled = true;
        #endregion

        Action<bool> EnableAll = null;
        protected override void SimpleInit()
        {
            // Any \n put in the parameter will not get converted to newlines, so convert them here.
            string prompt = MyInteraction.GetPrompt();
            if (prompt.Contains("\\n"))
            {
                MyInteraction.SetPrompt(prompt.Replace("\\n", "\n"));
            }

            SubscribeToAll(DisableEvent, (data) => { EnableAll(false);  });
            SubscribeToAll(EnableEvent, (data) => { EnableAll(true); });
            SubscribeToAll(ResetEvent, (data) => { ResetSendState(OnClick); });
            SubscribeToAll(NextEvent, (data) => { NextSendState(OnClick); });

            StartCoroutine(SetupInteraction, MyInteraction, OnClick);

            StartCoroutine(AddInteraction, Volume1, Volume1Prompt, Volume1OnClick);
            SubscribeToAll(Reset1, (data) => { ResetSendState(Volume1OnClick); });
            SubscribeToAll(Next1, (data) => { NextSendState(Volume1OnClick); });

            StartCoroutine(AddInteraction, Volume2, Volume2Prompt, Volume2OnClick);
            SubscribeToAll(Reset2, (data) => { ResetSendState(Volume2OnClick); });
            SubscribeToAll(Next2, (data) => { NextSendState(Volume2OnClick); });

            StartCoroutine(AddInteraction, Volume3, Volume3Prompt, Volume3OnClick);
            SubscribeToAll(Reset3, (data) => { ResetSendState(Volume3OnClick); });
            SubscribeToAll(Next3, (data) => { NextSendState(Volume3OnClick); });
        }

        private void AddInteraction(RigidBodyComponent volume, string prompt, string events)
        {
            if (volume != null && volume.IsValid)
            {
                ObjectPrivate op = ScenePrivate.FindObject(volume.ComponentId.ObjectId);
                if (op != null)
                {
                    var result = WaitFor(op.AddInteraction, prompt.Replace("\\n", "\n"), StartEnabled) as ObjectPrivate.AddInteractionData;
                    if (result.Success)
                    {
                        SetupInteraction(result.Interaction, events);
                    }
                    else
                    {
                        SimpleLog(LogLevel.Error, "Unable to add Interaction to object " + op.Name + ": " + result.Message);
                    }
                }
            }
        }

        private void SetupInteraction(Interaction interaction, string events)
        {
            EnableAll += interaction.SetEnabled;
            interaction.SetEnabled(StartEnabled);

            if (MaxEventsPerSecond >= 100 || MaxEventsPerSecond <= 0)
            {
                interaction.Subscribe((InteractionData data) =>
                {
                    if (DisableOnClick) interaction.SetEnabled(data.AgentId, false);

                    SimpleData sd = new SimpleData(this);
                    sd.SourceObjectId = ObjectPrivate.ObjectId;
                    sd.AgentInfo = ScenePrivate.FindAgent(data.AgentId)?.AgentInfo;
                    sd.ObjectId = sd.AgentInfo != null ? sd.AgentInfo.ObjectId : ObjectId.Invalid;
                    SendToAll(events, sd);
                });
            }
            else
            {
                TimeSpan waitTime = TimeSpan.FromSeconds(1.0 / MaxEventsPerSecond);
                while (true)
                {
                    InteractionData data = (InteractionData)WaitFor(interaction.Subscribe);
                    if (DisableOnClick) interaction.SetEnabled(data.AgentId, false);

                    SimpleData sd = new SimpleData(this);
                    sd.SourceObjectId = ObjectPrivate.ObjectId;
                    sd.AgentInfo = ScenePrivate.FindAgent(data.AgentId)?.AgentInfo;
                    sd.ObjectId = sd.AgentInfo != null ? sd.AgentInfo.ObjectId : ObjectId.Invalid;
                    SendToAll(events, sd);
                    Wait(waitTime);
                }
            }
        }
    }
}