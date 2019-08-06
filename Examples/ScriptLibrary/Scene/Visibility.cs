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
    [DisplayName("Visibility")]
    public class Visibility : LibraryBase
    {
        #region EditorProperties
        [Tooltip("Event name to show the object. Can be a comma separated list of event names.")]
        [DefaultValue("on")]
        [DisplayName("-> Show")]
        public readonly string ShowEvent;

        [Tooltip("Event name to hide the object. Can be a comma separated list of event names.")]
        [DefaultValue("off")]
        [DisplayName("-> Hide")]
        public readonly string HideEvent;

        [Tooltip("If set, control visiblity for all users. Otherwise, just for the user that sent the event")]
        [DefaultValue(false)]
        [DisplayName("Affect Everyone")]
        public readonly bool AffectEveryone;

        [Tooltip("Enable responding to events for this script")]
        [DefaultValue("visibility_enable")]
        [DisplayName("-> Enable")]
        public readonly string EnableEvent;

        [Tooltip("Disable responding to events for this script")]
        [DefaultValue("visibility_disable")]
        [DisplayName("-> Disable")]
        public readonly string DisableEvent;

        [Tooltip(@"If StartEnabled is false then this script will not respond to event to show or hide the object until an -> Enable event is received.")]
        [DefaultValue(true)]
        [DisplayName("Start Enabled")]
        public readonly bool StartEnabled = true;

        #endregion

        private MeshComponent meshComponent;
        Action Unsubscribes = null;

        protected override void SimpleInit()
        {
            if (!ObjectPrivate.TryGetFirstComponent<MeshComponent>(out meshComponent) || !meshComponent.IsScriptable)
            {
                Log.Write(LogLevel.Error, "Visibility", "Must have a scriptable mesh component to set visibility");
                return;
            }
            if (StartEnabled) Subscribe(null);

            SubscribeToAll(EnableEvent, Subscribe);
            SubscribeToAll(DisableEvent, Unsubscribe);
        }

        private void Subscribe(ScriptEventData sed)
        {
            if (Unsubscribes == null)
            {
                Unsubscribes = SubscribeToAll(ShowEvent, (ScriptEventData e) =>SetVisible(e, true));
                Unsubscribes += SubscribeToAll(HideEvent, (ScriptEventData e) =>SetVisible(e, false));
            }
        }

        private void Unsubscribe(ScriptEventData sed)
        {
            if (Unsubscribes != null)
            {
                Unsubscribes();
                Unsubscribes = null;
            }
        }

        void SetVisible(ScriptEventData sed, bool visible)
        {
            if (AffectEveryone)
            {
                meshComponent.SetIsVisible(visible);
                return;
            }

            ISimpleData idata = sed.Data.AsInterface<ISimpleData>();
            if (idata != null && idata.AgentInfo != null)
            {
                meshComponent.SetIsVisible(idata.AgentInfo.SessionId, visible);
            }
            else
            {
                Log.Write(LogLevel.Error, "Visibility", "Event data did not contain agent info.");
            }
        }
    }
}
