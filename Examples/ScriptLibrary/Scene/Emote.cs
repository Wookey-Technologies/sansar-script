/* This content is licensed under the terms of the Creative Commons Attribution 4.0 International License.
 * When using this content, you must:
 *     Acknowledge that the content is from the Sansar Knowledge Base.
 *     Include our copyright notice: "© 2017 Linden Research, Inc."
 *     Indicate that the content is licensed under the Creative Commons Attribution-Share Alike 4.0 International License.
 *     Include the URL for, or link to, the license summary at https://creativecommons.org/licenses/by-sa/4.0/deed.hi (and, if possible, to the complete license terms at https://creativecommons.org/licenses/by-sa/4.0/legalcode.
 * For example:
 * "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."
 */

using Sansar.Script;
using Sansar.Simulation;
using System;
using System.Linq;
using System.Collections.Generic;

namespace ScriptLibrary
{
    [Tooltip("Controls agent animations in response to simple script events.")]
    [DisplayName(nameof(Emote))]
    public class Emote : LibraryBase
    {
        #region EditorProperties
        [DisplayName("Animation")]
        [Tooltip("The animation to play when the play event is received")]
        public CharacterAnimation Animation;

        [Tooltip("The speed of the animation.")]
        [DefaultValue(1.0f)]
        [Range(0.1, 10)]
        public float AnimationSpeed;
        
        [Tooltip("Whether or not to loop the animation.")]
        [DefaultValue(true)]
        public readonly bool Repeat;

        [Tooltip("True to animate upper body only, false for full body.")]
        [DefaultValue(false)]
        public readonly bool UpperBodyOnly;

        [Tooltip("If false, the animation will not play for VR users who have their hands enabled. Useful to avoid upper body animations which would overrule the user's hand positions.")]
        [DisplayName("Allow in VR")]
        [DefaultValue(true)]
        public readonly bool AllowInVR;

        [Tooltip("The event to play the animation.")]
        [DefaultValue("on")]
        [DisplayName("-> Play")]
        public readonly string PlayEvent;

        [Tooltip("The event to stop the animation.")]
        [DefaultValue("off")]
        [DisplayName("-> Stop")]
        public readonly string StopEvent;

        [Tooltip("Enable responding to events for this script")]
        [DefaultValue("enable_emote")]
        public readonly string EnableEvent;

        [Tooltip("Disable responding to events for this script")]
        [DefaultValue("disable_emote")]
        public readonly string DisableEvent;

        [Tooltip(@"If StartEnabled is true then the script will respond to commands when the scene is loaded
If StartEnabled is false then the script will not respond to commands until an (-> Enable) event is received.")]
        [DefaultValue(true)]
        public readonly bool StartEnabled;

        #endregion

        private Action subscription = null;

        private void Subscribe(ScriptEventData sed)
        {
            if (subscription == null)
            {
                subscription = SubscribeToAll(PlayEvent, (ScriptEventData data) =>
                {
                    try
                    {
                        if(AllowInVR || !ScenePrivate.FindAgent(data.Data.AsInterface<ISimpleData>().AgentInfo.SessionId).Client.InVrAndHandsActive)
                        {
                            CharacterComponent cc;
                            ScenePrivate.FindObject(data.Data.AsInterface<ISimpleData>().AgentInfo.ObjectId).TryGetFirstComponent(out cc);
                            if(cc != null)
                            {
                                cc.PlayAnimation(Animation, (Repeat ? AnimationPlaybackMode.Loop : AnimationPlaybackMode.PlayOnce), (UpperBodyOnly ? AnimationBoneSubset.Subset1 : AnimationBoneSubset.Full), AnimationSpeed);
                            }
                        }
                    }
                    catch{}
                });

                subscription += SubscribeToAll(StopEvent, (ScriptEventData data) =>
                {
                    try
                    {
                        CharacterComponent cc;
                        ScenePrivate.FindObject(data.Data.AsInterface<ISimpleData>().AgentInfo.ObjectId).TryGetFirstComponent(out cc);
                        if(cc != null)
                        {
                            cc.StopAnimations();
                        }
                    }
                    catch{}
                });

            }
        }
        private void Unsubscribe(ScriptEventData sed)
        {

            if (subscription != null)
            {
                subscription();
                subscription = null;
            }
        }

        protected override void SimpleInit()
        {
            if (Animation == null)
            {
                Log.Write(LogLevel.Error, "Animation not set!");
                return;
            }

            if (StartEnabled) Subscribe(null);

            SubscribeToAll(EnableEvent, Subscribe);
            SubscribeToAll(DisableEvent, Unsubscribe);

        }
    }
}
