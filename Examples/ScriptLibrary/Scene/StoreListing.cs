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
using Sansar;
using System;
using System.Linq;

namespace ScriptLibrary
{
    [Tooltip("Shows a store listing for an object.")]
    [DisplayName("Store Listing")]
    public class StoreListing : LibraryBase // ShowStoreListing requires Client requires AgentPrivate requires ScenePrivate
    {
        #region EditorProperties
        [Tooltip("Show the store listing for the Product. Can be a comma separated list of event names.")]
        [DefaultValue("show")]
        [DisplayName("-> Show Listing")]
        public readonly string ShowListingEvent;

        [Tooltip("The Product Id or store listing URL of the product to show the listing for.")]
        [DisplayName("Product Id or URL")]
        [DefaultValue("8c664587-a520-4d7a-978d-f9d757d1f790")]
        public string ProductId;

        [Tooltip("Show the user store listing.")]
        [DefaultValue("showUserStore")]
        [DisplayName("-> Show UserStore")]
        public readonly string ShowUserStoreEvent;

        [Tooltip("The creator handle for store listing.")]
        [DisplayName("creatorHandle")]
        [DefaultValue("sansar-studios")]
        public string CreatorHandle;

        [Tooltip("Enable responding to events for this script. Can be a comma separated list of event names.")]
        [DefaultValue("listing_enable")]
        [DisplayName("-> Enable")]
        public readonly string EnableEvent;

        [Tooltip("Disable responding to events for this script. Can be a comma separated list of event names.")]
        [DefaultValue("listing_disable")]
        [DisplayName("-> Disable")]
        public readonly string DisableEvent;

        // If StartEnabled is true then the script will respond to interactions when the scene is loaded
        // If StartEnabled is false then the script will not respond to interactions until an (-> Enable) event is received.
        [DefaultValue(true)]
        [DisplayName("Start Enabled")]
        public readonly bool StartEnabled = true;
        #endregion

        Action unsubscribes = null;
        Guid ProductGuid;

        protected override void SimpleInit()
        {
            if (!Guid.TryParse(ProductId, out ProductGuid))
            {
                bool foundId = false;
                // Find the ID from the store listing url. Very generic, will just find the first url segment or query arg it can convert to a UUID.
                // https://store.sansar.com/listings/9eb72eb2-38c1-4cd3-a9eb-360e2f19e403/female-pirate-hat-r3d
                string[] segments = ProductId.Split(new string[] { "/", "?", "&", "=" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string segment in segments)
                {
                    if (segment.Length >= 32
                        && Guid.TryParse(segment, out ProductGuid))
                    {
                        foundId = true;
                        break;
                    }
                }

                if (!foundId)
                {
                    SimpleLog(LogLevel.Error, "Product Id must be a valid Guid.");
                    return;
                }
            }

            if (StartEnabled) Subscribe(null);

            SubscribeToAll(EnableEvent, Subscribe);
            SubscribeToAll(DisableEvent, (ScriptEventData data) =>
            {
                if (unsubscribes != null)
                {
                    unsubscribes();
                    unsubscribes = null;
                }
            });
        }

        private void Subscribe(ScriptEventData sed)
        {
            if (unsubscribes == null)
            {
                unsubscribes = SubscribeToAll(ShowListingEvent, (ScriptEventData subdata) =>
                {
                    try
                    {
                        ISimpleData simpledata = subdata.Data?.AsInterface<ISimpleData>();
                        if (simpledata != null)
                        {
                            AgentPrivate agent = ScenePrivate.FindAgent(simpledata.AgentInfo.SessionId);
                            agent.Client.OpenStoreListing(ProductGuid);
                        }
                    }

                    catch (NullReferenceException nre) { SimpleLog(LogLevel.Info, "NullReferenceException showing store listing (maybe the user left): " + nre.Message); }
                    catch (Exception e) { SimpleLog(LogLevel.Error, "Exception showing store listing: " + e.Message); }
                });

                unsubscribes += SubscribeToAll(ShowUserStoreEvent, (ScriptEventData subdata) =>
                {
                    try
                    {
                        ISimpleData simpledata = subdata.Data?.AsInterface<ISimpleData>();
                        if (simpledata != null)
                        {
                            AgentPrivate agent = ScenePrivate.FindAgent(simpledata.AgentInfo.SessionId);
                            if(agent != null)
                            {
                                agent.Client.OpenUserStore(CreatorHandle);
                            }
                        }
                    }

                    catch (NullReferenceException nre) { SimpleLog(LogLevel.Info, "NullReferenceException showing user store (maybe the user left): " + nre.Message); }
                    catch (Exception e) { SimpleLog(LogLevel.Error, "Exception showing user store: " + e.Message); }
                });
            }
        }
    }
}