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
    [Tooltip("Updates media surfaces in response to simple script events.")]
    [DisplayName(nameof(Media))]
    public class Media : LibraryBase // Media APIs require ScenePrivate
    {
        #region EditorProperties
        [Tooltip("Events to trigger media A")]
        [DefaultValue("on")]
        [DisplayName("-> Start Media A")]
        public readonly string StartMessageA;

        [Tooltip("Media URL A")]
        [DefaultValue("https://www.youtube.com/embed/Cor2ruiy_n8?autoplay=1&loop=1&playlist=Cor2ruiy_n8&allowfullscreen=1&controls=0&vq=hd2160")]
        [DisplayName("Media A")]
        public readonly string MediaAUrl;

        [Tooltip("Events to trigger media B")]
        [DefaultValue("off")]
        [DisplayName("-> Start Media B")]
        public readonly string StartMessageB;

        [Tooltip("Media URL B")]
        [DefaultValue("")]
        [DisplayName("Media B")]
        public readonly string MediaBUrl;

        [DefaultValue(1920)]
        [Range(256,2048)]
        [DisplayName("Media Source Width")]
        public readonly int MediaWidth;

        [DefaultValue(1080)]
        [Range(256, 2048)]
        [DisplayName("Media Source Height")]
        public readonly int MediaHeight;

        [Tooltip("Private media for one agent only")]
        [DefaultValue(false)]
        [DisplayName("Private Media")]
        public readonly bool PrivateMedia = false;

        [Tooltip("Enable responding to events for this script")]
        [DefaultValue("media_enable")]
        [DisplayName("-> Enable")]
        public readonly string EnableEvent;

        [Tooltip("Disable responding to events for this script")]
        [DefaultValue("media_disable")]
        [DisplayName("-> Disable")]
        public readonly string DisableEvent;

        [Tooltip(@"If StartEnabled is true then the script will respond to interactions when the scene is loaded
If StartEnabled is false then the script will not respond to interactions until an (-> Enable) event is received.")]
        [DefaultValue(true)]
        [DisplayName("Start Enabled")]
        public readonly bool StartEnabled = true;
        #endregion

        System.Collections.Generic.Dictionary<string, string> mediaUrls = null;

        Action unsubscribes = null;

        protected override void SimpleInit()
        {
            mediaUrls = new System.Collections.Generic.Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(StartMessageA))
            {
                mediaUrls[StartMessageA] = MediaAUrl;
            }

            if (!string.IsNullOrWhiteSpace(StartMessageB))
            {
                mediaUrls[StartMessageB] = MediaBUrl;
            }

            if (mediaUrls.Keys.Count > 0)
            {
                if (StartEnabled) SubscribeToMessages();

                SubscribeToAll(EnableEvent, (ScriptEventData data) => { SubscribeToMessages(); });
                SubscribeToAll(DisableEvent, (ScriptEventData data) => { UnsubscribeFromMessages(); });
            }
        }

        void SubscribeToMessages()
        {
            // Already subscribed
            if (unsubscribes != null)
            {
                return;
            }

            foreach (var kvp in mediaUrls)
            {
                unsubscribes += SubscribeToAll(kvp.Key, (ScriptEventData subData) =>
                {
                    if (PrivateMedia)
                    {
                        ISimpleData simpleData = subData.Data?.AsInterface<ISimpleData>();

                        if (simpleData != null && simpleData.AgentInfo != null)
                        {
                            AgentPrivate agent = ScenePrivate.FindAgent(simpleData.AgentInfo.SessionId);

                            if (agent != null && agent.IsValid)
                            {
                                try
                                {
                                    agent.OverrideMediaSource(kvp.Value, MediaWidth, MediaHeight);
                                }
                                catch (ThrottleException)
                                {
                                // Throttled
                                Log.Write(LogLevel.Warning, "OverrideMediaSource request throttle hit. Media not set.");
                                }
                                catch (NullReferenceException)
                                {
                                // User Left, ignore.
                            }
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            ScenePrivate.OverrideMediaSource(kvp.Value, MediaWidth, MediaHeight);
                        }
                        catch (ThrottleException)
                        {
                        // Throttled
                        Log.Write(LogLevel.Warning, "OverrideMediaSource request throttle hit. Media not set.");
                        }
                    }
                });
            }
        }

        void UnsubscribeFromMessages()
        {
            if (unsubscribes != null)
            {
                unsubscribes();
                unsubscribes = null;
            }
        }
    }
}