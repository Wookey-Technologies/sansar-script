/* This content is licensed under the terms of the Creative Commons Attribution 4.0 International License.
 * When using this content, you must:
 * •    Acknowledge that the content is from the Sansar Knowledge Base.
 * •    Include our copyright notice: "© 2017 Linden Research, Inc."
 * •    Indicate that the content is licensed under the Creative Commons Attribution-Share Alike 4.0 International License.
 * •    Include the URL for, or link to, the license summary at https://creativecommons.org/licenses/by-sa/4.0/deed.hi (and, if possible, to the complete license terms at https://creativecommons.org/licenses/by-sa/4.0/legalcode.
 * For example:
 * "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."
 */

using Sansar;
using Sansar.Script;
using Sansar.Simulation;
using System;
using System.Linq;

namespace ScriptLibrary
{
    [Tooltip("Teleports a player in response to simple script events.")]
    [DisplayName(nameof(Teleport))]
    public class Teleport : LibraryBase // Teleport requires Client requires AgentPrivate requires ScenePrivate
    {
        #region EditorProperties
        [Tooltip("Teleport the person who interacted to somewhere within the scene. Can be a comma separated list of event names.")]
        [DefaultValue("on")]
        [DisplayName("-> Local Teleport")]
        public readonly string LocalEvent;

        [Tooltip("Teleport the person who interacted to somewhere within the scene when the object or trigger volume this script is on is collided with by a user.")]
        [DefaultValue(false)]
        [DisplayName("LocalTP on Collide")]
        public readonly bool UseObjectForLocal;

        [Tooltip("The teleport destination within the scene.")]
        [DisplayName("Destination Position")]
        [DefaultValue("<0,0,0>")]
        public readonly Vector Destination;

        [Tooltip("The teleport destination forward direction within the scene. Note that avatars always remain upright so the Z component will be ignored.\nSet to <0,0,0> to maintain the user's orientation through the teleport.")]
        [DisplayName("Destination Forward Direction")]
        [DefaultValue("<0,1,0>")]
        public readonly Vector DestinationForward;

        [Tooltip(@"If true the destination position is relative to the object this script is on
If false the destination is in scene coordinates regardless of this script's object's position.")]
        [DefaultValue(true)]
        [DisplayName("Relative Position")]
        public readonly bool RelativePosition;

        [Tooltip(@"If true the destination forward direction is relative to the object this script is on
If false the destination is in scene coordinates regardless of this script's object's rotation.")]
        [DefaultValue(true)]
        [DisplayName("Relative Rotation")]
        public readonly bool RelativeRotation;

        [Tooltip("Teleport the person that interacted to a remote scene. Can be a comma separated list of event names.")]
        [DefaultValue("")]
        [DisplayName("-> Remote Teleport")]
        public readonly string RemoteEvent;

        [Tooltip("Teleport the person that interacted to a remote scene when the object or trigger volume this script is on is collided with by a user.")]
        [DefaultValue(false)]
        [DisplayName("RemoteTP on Collide")]
        public readonly bool UseObjectForRemote;

        [Tooltip("The destination scene owner (from the scene url).")]
        [DisplayName("Dest. Owner")]
        [DefaultValue("")]
        public readonly String DestOwner;

        [Tooltip("The destination scene handle (from the scene url).")]
        [DefaultValue("")]
        [DisplayName("Dest. Scene")]
        public readonly String DestScene;

        [Tooltip("An optional target spawn point name in the destination scene.")]
        [DefaultValue("")]
        [DisplayName("Dest. Spawn")]
        public readonly String DestSpawn;

        [Tooltip("Enable responding to events for this script. Can be a comma separated list of event names.")]
        [DefaultValue("tp_enable")]
        [DisplayName("-> Enable")]
        public readonly string EnableEvent;

        [Tooltip("Disable responding to events for this script. Can be a comma separated list of event names.")]
        [DefaultValue("tp_disable")]
        [DisplayName("-> Disable")]
        public readonly string DisableEvent;

        [Tooltip(@"If StartEnabled is true then the script will respond to events when the scene is loaded
If StartEnabled is false then the script will not respond to events until an (-> Enable) event is received.")]
        [DefaultValue(true)]
        [DisplayName("Start Enabled")]
        public readonly bool StartEnabled = true;
        #endregion

        Action Unsubscribes = null;
        Vector NormalizedForward;
        bool UseAvatarRotation = false;

        protected override void SimpleInit()
        {
            UseAvatarRotation = DestinationForward.LengthSquared() < 0.01f;
            NormalizedForward = (DestinationForward.LengthSquared() > 0.0f ? DestinationForward.Normalized() : Vector.ObjectForward);

            if (StartEnabled) Subscribe(null);

            SubscribeToAll(EnableEvent, Subscribe);
            SubscribeToAll(DisableEvent, Unsubscribe);
        }

        private void Subscribe(ScriptEventData sed)
        {
            if (Unsubscribes == null)
            {
                Unsubscribes = SubscribeToAll(LocalEvent, LocalTeleport);
                Unsubscribes += SubscribeToAll(RemoteEvent, RemoteTeleport);

                if (UseObjectForLocal || UseObjectForRemote)
                {
                    try
                    {
                        RigidBodyComponent rigidBody = null;
                        if (ObjectPrivate.TryGetFirstComponent(out rigidBody))
                        {
                            CollisionEventType collisionEvent = rigidBody.IsTriggerVolume() ? CollisionEventType.Trigger : CollisionEventType.CharacterContact;

                            if (UseObjectForRemote)
                            {
                                Unsubscribes += rigidBody.Subscribe(collisionEvent, (CollisionData data) =>
                                {
                                    AgentPrivate agent = ScenePrivate.FindAgent(data.HitComponentId.ObjectId);
                                    if (agent != null) RemoteTeleport(agent.AgentInfo.SessionId);
                                }).Unsubscribe;
                            }
                            if (UseObjectForLocal)
                            {
                                Unsubscribes += rigidBody.Subscribe(collisionEvent, (CollisionData data) =>
                                {
                                    AgentPrivate agent = ScenePrivate.FindAgent(data.HitComponentId.ObjectId);
                                    if (agent != null) LocalTeleport(agent.AgentInfo.SessionId);
                                }).Unsubscribe;
                            }
                        }
                    }
                    catch (NullReferenceException nre) { SimpleLog(LogLevel.Info, "NullReferenceException setting up collision events: " + nre.Message); }
                    catch (Exception e) { SimpleLog(LogLevel.Error, "Exception setting up collision event user: " + e.Message); }
                }
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

        private void LocalTeleport(ScriptEventData data)
        {
            ISimpleData idata = data.Data.AsInterface<ISimpleData>();
            if (idata != null && idata.AgentInfo != null)
                LocalTeleport(idata.AgentInfo.SessionId);
            else
                SimpleLog(LogLevel.Info, "No agent attached to LocalTeleport request.");
        }

        private void LocalTeleport(SessionId sessionId)
        {
            try
            {
                Vector teleportPosition = Destination;
                Vector teleportForward = NormalizedForward;

                AgentPrivate agent = ScenePrivate.FindAgent(sessionId);
                if (UseAvatarRotation)
                {
                    if (agent != null && agent.IsValid)
                    {
                        ObjectPrivate agentObject = ScenePrivate.FindObject(agent.AgentInfo.ObjectId);
                        if (agentObject != null)
                        {
                            teleportForward = agentObject.ForwardVector;
                        }
                    }
                }
                else
                {
                    if (RelativePosition)
                    {
                        if (RelativeRotation)
                            teleportPosition = ObjectPrivate.Position + Destination.Rotate(ObjectPrivate.Rotation);
                        else
                            teleportPosition = ObjectPrivate.Position + Destination;
                    }

                    if (RelativeRotation)
                    {
                        teleportForward = NormalizedForward.Rotate(ObjectPrivate.Rotation);
                    }
                }

                if (agent != null)
                {
                    agent.Client.TeleportTo(teleportPosition, teleportForward);
                }
            }
            catch (NullReferenceException nre) { SimpleLog(LogLevel.Info, "NullReferenceException local teleporting user (maybe they just left): " + nre.Message); }
            catch (Exception e) { SimpleLog(LogLevel.Error, "Exception local teleporting user: " + e.Message); }
        }

        private void RemoteTeleport(ScriptEventData data)
        {
            ISimpleData idata = data.Data.AsInterface<ISimpleData>();
            if (idata != null && idata.AgentInfo != null)
                RemoteTeleport(idata.AgentInfo.SessionId);
            else
                SimpleLog(LogLevel.Info, "No agent attached to RemoteTeleport request.");
        }

        private void RemoteTeleport(SessionId sessionId)
        {
            try
            {
                AgentPrivate agent = ScenePrivate.FindAgent(sessionId);
                if (agent != null)
                {
                    agent.Client.TeleportToLocation(DestOwner, DestScene, DestSpawn);
                }
            }
            catch (NullReferenceException nre) { SimpleLog(LogLevel.Info, "NullReferenceException remote teleporting user (maybe they just left): " + nre.Message); }
            catch (Exception e) { SimpleLog(LogLevel.Error, "Exception remote teleporting user: " + e.Message); }
        }
    }
}