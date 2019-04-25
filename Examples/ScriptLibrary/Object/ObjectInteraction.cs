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
    public class ObjectInteraction : ObjectBase
    {
        #region EditorProperties
        [Tooltip(@"The interaction for this object, with a default prompt of ""Click Me!""")]
        [DefaultValue("Click Me!")]
        [DisplayName("Interaction prompt")]
        public readonly Sansar.Simulation.Interaction MyInteraction;

        [Tooltip(@"The events to send on click of this object. Can be a comma separated list of event names.
Use >> to create chains of events that send on subsequent clicks. 'one>>two,three>>four' will send 'one' on the first click, 'two' and 'three' on the second click, 'four' on the third, then 'one' on the fifth etc.")]
        [DefaultValue("on>>off")]
        [DisplayName("On Click ->")]
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
        [DisplayName("-> Reset Count")]
        public readonly string ResetEvent;

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

        protected override void SimpleInit()
        {
            // Any \n put in the parameter will not get converted to newlines, so convert them here.
            string prompt = MyInteraction.GetPrompt();
            if (prompt.Contains("\\n"))
            {
                MyInteraction.SetPrompt(prompt.Replace("\\n", "\n"));
            }

            if (!StartEnabled) MyInteraction.SetEnabled(false);

            SubscribeToAll(DisableEvent, (data) => { MyInteraction.SetEnabled(false); });
            SubscribeToAll(EnableEvent, (data) => { MyInteraction.SetEnabled(true); });
            SubscribeToAll(ResetEvent, (data) => { Reset(OnClick); });

            if (!string.IsNullOrWhiteSpace(OnClick))
            {
                if (MaxEventsPerSecond >= 100 || MaxEventsPerSecond <= 0)
                {
                    MyInteraction.Subscribe((InteractionData data) =>
                    {
                        if (DisableOnClick) MyInteraction.SetEnabled(data.AgentId, false);
                        SimpleData sd = new SimpleData(this);
                        sd.SourceObjectId = ObjectPrivate.ObjectId;
                        sd.AgentInfo = ScenePublic.FindAgent(data.AgentId)?.AgentInfo;
                        sd.ObjectId = sd.AgentInfo != null ? sd.AgentInfo.ObjectId : ObjectId.Invalid;
                        SendToAll(OnClick, sd);
                    });
                }
                else
                {
                    TimeSpan waitTime = TimeSpan.FromSeconds(1.0 / MaxEventsPerSecond);
                    while (true)
                    {
                        InteractionData data = (InteractionData)WaitFor(MyInteraction.Subscribe);
                        if (DisableOnClick) MyInteraction.SetEnabled(data.AgentId, false);
                        SimpleData sd = new SimpleData(this);
                        sd.SourceObjectId = ObjectPrivate.ObjectId;
                        sd.AgentInfo = ScenePublic.FindAgent(data.AgentId)?.AgentInfo;
                        sd.ObjectId = sd.AgentInfo != null ? sd.AgentInfo.ObjectId : ObjectId.Invalid;
                        SendToAll(OnClick, sd);
                        Wait(waitTime);
                    }
                }
            }
        }
    }
}