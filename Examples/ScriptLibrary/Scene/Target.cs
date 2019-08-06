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
using System.Linq;
using System;

namespace ScriptLibrary
{
    public interface ITarget
    {
        int Hit(AgentInfo agent, CommandData data);
        string GetGroupTag();
    }

    [DisplayName("Game Target")]
    [Tooltip("Use with the Selector or Game.Gun scripts to create targets that will respond to being shot or selected.")]
    public class Target : LibraryBase, ITarget
    {
        [DisplayName("Target Hit ->")]
        [Tooltip("This event will act as though the shooter did the action. For example, a Teleport script listening for this event would teleport the user that selected or shot this target.")]
        public string ShotHitEvent;

        [DefaultValue(10)]
        [Tooltip("Number of points earned for hitting this target.")]
        [DisplayName("Point Value")]
        public int PointValue;

        [Tooltip("Enable this target to be findable by selectors or guns. Can be a comma separated list of event names.")]
        [DefaultValue("target_enable")]
        [DisplayName("-> Enable")]
        public readonly string EnableEvent;

        [Tooltip("Disable this target so it is not findable by selectors or guns. Can be a comma separated list of event names.")]
        [DefaultValue("target_disable")]
        [DisplayName("-> Disable")]
        public readonly string DisableEvent;

        [Tooltip(@"If StartEnabled is true then this target will be findable by selectors or guns when the scene is loaded.
If StartEnabled is false then this target will not be findable until an (-> Enable) event is received.")]
        [DefaultValue(true)]
        [DisplayName("Start Enabled")]
        public readonly bool StartEnabled = true;

        protected override Context ReflectiveContexts => Context.ObjectPrivate;
        protected override string ReflectiveName => "Simple.Target";    // To continue working with older scripts, register as Simple.Target.

        protected override void SimpleInit()
        {
            foreach (Target t in ObjectPrivate.FindScripts<Target>("Simple.Target")) { }

            if (!StartEnabled) Unregister();

            SubscribeToAll(EnableEvent, (data) => { Register(); });
            SubscribeToAll(DisableEvent, (data) => { Unregister(); });
        }

        public int Hit(AgentInfo agent, CommandData data)
        {
            try
            {
                SimpleData simpleData = new SimpleData(this);
                simpleData.AgentInfo = agent;
                simpleData.ObjectId = agent.ObjectId;
                simpleData.SourceObjectId = ObjectPrivate.ObjectId;
                SendToAll(ShotHitEvent, simpleData);
            }
            catch (System.Exception) { }
            return PointValue;
        }

        public string GetGroupTag() { return Group; }
    }
}