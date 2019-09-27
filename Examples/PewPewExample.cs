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

namespace PewPewExample
{
    public class Gun : SceneObjectScript
    {
        [Tooltip("Played at the location they were hit.")]
        public SoundResource PlayerHitSound;

        [Tooltip("Played at the location the shot originated from when a player or target is hit.")]
        public SoundResource ShotHitSound;

        [Tooltip("Played at the location the shot originated from when no player or target is hit.")]
        public SoundResource ShotMissSound;

        [Tooltip("Played at the location the shot originated from when firing and out of ammo.")]
        public SoundResource OutOfAmmoSound;

        [Tooltip("Played at the location the shot originated from when reloading.")]
        public SoundResource ReloadedSound;

        [DefaultValue(0)]
        [Range(-48, 12)]
        [DisplayName("Target Loudness")]
        [Tooltip("The loudness for the sound played when a player gets hit, played at the location they were hit.")]
        public float TargetLoudness;

        [DefaultValue(0)]
        [Range(-48, 12)]
        [DisplayName("Shot Loudness")]
        [Tooltip("The loudness for all sounds played at the location the shot originated from. Including empty clicks, reloads and successful hits")]
        public float ShotLoudness;

        [DefaultValue(100)]
        [DisplayName("Player Points")]
        [Tooltip("Points earned any time a player is hit.")]
        public int PointsPerPlayer;

        [DefaultValue(6)]
        [Range(0, 24)]
        public int ClipSize;

        [DefaultValue(false)]
        public bool DebugLogging;

        [DefaultValue("Trigger")]
        [Tooltip("Trigger does not work well as the firing mechanism in desktop as you will drop the gun every time you fire. Suggest PrimaryAction instead.")]
        public string ShootCommand;

        [DefaultValue("SecondaryAction")]
        public string ReloadCommand;

        [DefaultValue(true)]
        [Tooltip("If true it is possible to reload by 'shooting' yourself or your other hand etc.")]
        [DisplayName("Target Self To Reload")]
        public bool TargetSelfReload;

        private int ammo;
        private int shotsFired;
        private int shotsHit;
        private int score;
        private ControlPointType heldHand = ControlPointType.Invalid;

        private AgentInfo holdingAgent;

        PlaySettings ShotSettings;
        PlaySettings TargetSettings;
        public override void Init()
        {
            ShotSettings.Loudness = ShotLoudness;
            TargetSettings.Loudness = TargetLoudness;

            RigidBodyComponent RigidBody;
            if (ObjectPrivate.TryGetFirstComponent(out RigidBody))
            {
                if (!RigidBody.GetCanGrab())
                {
                    RigidBody.SetCanGrab(true, (data) =>
                    {
                        if (data.Success == false)
                        {
                            Log.Write("Could not set gun to grabbable - won't be able to pick up the gun.");
                        }
                    });
                }
                RigidBody.SubscribeToHeldObject(HeldObjectEventType.Grab, OnPickup);
                RigidBody.SubscribeToHeldObject(HeldObjectEventType.Release, OnDrop);
                Log.Write("Init complete");
            }
        }

        System.Action unsubscribe = null;

        void OnPickup(HeldObjectData data)
        {
            try
            {
                AgentPrivate agent = ScenePrivate.FindAgent(data.HeldObjectInfo.SessionId);
                holdingAgent = agent.AgentInfo;
                heldHand = data.HeldObjectInfo.ControlPoint;

                ammo = 10;
                shotsFired = 0;
                shotsHit = 0;
                score = 0;
                unsubscribe += agent.Client.SubscribeToCommand(ShootCommand, OnTrigger, null).Unsubscribe;
                unsubscribe += agent.Client.SubscribeToCommand(ReloadCommand, Reload, null).Unsubscribe;
                Log.Write(GetType().Name, "Gun Picked up");
            }
            catch (System.Exception)
            {
                holdingAgent = null;
                Log.Write(GetType().Name, "Exception picking up gun");
            }
        }

        void OnDrop(HeldObjectData data)
        {
            try
            {
                unsubscribe();
                unsubscribe = null;
                AgentPrivate user = ScenePrivate.FindAgent(holdingAgent.SessionId);
                float accuracy = 0;
                if (shotsHit > 0)
                {
                    accuracy = 100.0f * (float)shotsHit / (float)shotsFired;
                }
                user.SendChat($"Final score: {score}. You hit {shotsHit} out of {shotsFired}, a hit accuracy of {accuracy.ToString("00.0")}%");
                Log.Write(GetType().Name, "Gun dropped");
            }
            catch (System.Exception) { }
        }

        void Reload(CommandData command)
        {
            try
            {
                if (ReloadedSound != null) ScenePrivate.PlaySoundAtPosition(ReloadedSound, command.TargetingOrigin, ShotSettings);
                if (DebugLogging) Log.Write(GetType().Name, "Reloaded");
                ammo = ClipSize;
            }
            catch (System.Exception) { }
        }

        bool CheckReload(CommandData command)
        {
            if (!TargetSelfReload) return false;

            try
            {
                var reloadAgent = ScenePrivate.FindAgent(command.TargetingComponent.ObjectId);
                if (reloadAgent.AgentInfo.SessionId == holdingAgent.SessionId)
                {
                    // Reload
                    Reload(command);
                    return true;
                }
            }
            catch (System.Exception) { }
            return false;
        }


        void OnTrigger(CommandData command)
        {
            try
            {
                if (CheckReload(command))
                {
                    return;
                }

                if (command.ControlPoint != heldHand)
                {
                    // Let them use the off hand to reload the gun, but otherwise early exit.
                    if (DebugLogging) Log.Write(GetType().Name, $"Dropping Non-reload from device {command.ControlPoint.ToString()} while being held by {command.ControlPoint.ToString()}");
                    return;
                }

                if (ammo <= 0)
                {
                    // Play 'empty' sound
                    if (OutOfAmmoSound != null) ScenePrivate.PlaySoundAtPosition(OutOfAmmoSound, command.TargetingOrigin, ShotSettings);
                    if (DebugLogging) Log.Write(GetType().Name, "Out of ammo");
                    return;
                }

                
                shotsFired++;
                ammo--;

                var targetAgent = ScenePrivate.FindAgent(command.TargetingComponent.ObjectId);
                if (targetAgent != null)
                {

                    shotsHit++;
                    score += PointsPerPlayer;
                    if (PlayerHitSound != null) ScenePrivate.PlaySoundAtPosition(PlayerHitSound, command.TargetingPosition, TargetSettings);
                    if (ShotHitSound != null) ScenePrivate.PlaySoundAtPosition(ShotHitSound, command.TargetingOrigin, ShotSettings);
                    if (DebugLogging) Log.Write(GetType().Name, "Player Hit");
                }
                else
                {
                    ObjectPrivate targetObject = ScenePrivate.FindObject(command.TargetingComponent.ObjectId);
                    if (targetObject != null)
                    {
                        Target target = targetObject.FindScripts<Target>("PewPewExample.Target").FirstOrDefault();
                        if (target != null)
                        {
                            if (ShotHitSound != null) ScenePrivate.PlaySoundAtPosition(ShotHitSound, command.TargetingOrigin, ShotSettings);
                            if (DebugLogging) Log.Write(GetType().Name, "Target hit");
                            score += target.Hit(holdingAgent, ScenePrivate);
                            shotsHit++;
                            return;
                        }
                    }
                    if (ShotMissSound != null) ScenePrivate.PlaySoundAtPosition(ShotMissSound, command.TargetingOrigin, ShotSettings);
                    if (DebugLogging) Log.Write(GetType().Name, "Miss:: " + command.ToString());
                }
            }
            catch (System.Exception) { } // ignore exceptions for not found agents.
        }
    }

    [RegisterReflective]
    public class Target : ObjectScript
    {
        [Tooltip("Sound to play at this targets location when the target is hit.")]
        public SoundResource HitSound;

        [DefaultValue(0)]
        [Range(-48, 12)]
        [DisplayName("Hit Sound Loudness")]
        [Tooltip("The loudness for the sound played when this target gets hit, played at the location of this target.")]
        public float Loudness;

        [DefaultValue(10)]
        [Tooltip("Number of points earned for hitting this target.")]
        [DisplayName("Point Value")]
        public int PointValue;

        PlaySettings SoundSettings;
        public override void Init()
        {
            SoundSettings.Loudness = Loudness;
        }

        public int Hit(AgentInfo agent, ScenePrivate scene)
        {
            if (HitSound != null)
            {
                scene.PlaySoundAtPosition(HitSound, ObjectPrivate.Position, SoundSettings);
            }
            return PointValue;
        }
    }
}