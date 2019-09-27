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
    [Tooltip("Put this script on an object that is to be picked up and used like a gun or pointer, to send events to other scripts when specific game-like actions happen. The object should have grab points defined with Stays in Hand set to true. For a target game use the Game.Target script on the objects to be hit.")]
    [DisplayName("Game Gun")]
    public class Gun : LibraryBase // SubscribeToCommand requires Client requires AgentPrivate requires ScenePrivate
    {
        [Tooltip("Event sent whenever the trigger is pulled and there is ammo.")]
        [DisplayName("Shot Fired ->")]
        public string ShotFiredEvent;

        [Tooltip("Event sent when a shot hits another avatar.")]
        [DisplayName("Player Hit ->")]
        public string PlayerHitEvent;

        [Tooltip("Event sent when a shot hits anything recognizable target: an object with the Game.Target script on it.")]
        [DisplayName("Target Hit ->")]
        public string ShotHitEvent;

        [Tooltip("Event sent when a shot does not hit either a player or an object with the Game.Target script on it.")]
        [DisplayName("Shot Miss ->")]
        public string ShotMissEvent;

        [Tooltip("Event sent when the trigger is pulled but there is no ammo.")]
        [DisplayName("Out Of Ammo ->")]
        public string OutOfAmmoEvent;

        [Tooltip("Event sent when the gun is reloaded.")]
        [DisplayName("Reloaded ->")]
        public string ReloadedEvent;

        [DefaultValue("Trigger")]
        [Tooltip("The input method to fire. Default: 'Trigger' corresponds to left mouse button and VR triggers.")]
        public string ShootCommand;

        [DefaultValue("SecondaryAction")]
        [Tooltip("The input method to reload, if reload is enabled. Default: 'SecondaryAction' corresponds to R on keyboard, X and A on Oculus Touch and bottom-of-touchpad click on Vive Wands.")]
        public string ReloadCommand;

        [DisplayName("Reload Time")]
        [Tooltip("The amount of time in seconds that it takes to reload before firing can happen again.")]
        public float ReloadTime;

        [DisplayName("Keep Score")]
        [Tooltip("If true a score and hit accuracy will be said on chat when the object is dropped.")]
        [DefaultValue(true)]
        public bool KeepScore;

        [Tooltip("If true then the 'Target Hit ->' event and the Target's 'Target Hit ->' event will only be sent if the target is in the same group as the selector. If the Target is in a different group then 'Shot Miss ->' events are sent.")]
        [DisplayName("Group Targets Only")]
        [DefaultValue(true)]
        public bool SameGroupRequired = true;

        [DefaultValue(100)]
        [DisplayName("Player Points")]
        [Tooltip("Points earned any time a player is hit. Points per target are set individually in the Target script.")]
        public int PointsPerPlayer;

        [DisplayName("Clip Size")]
        [Tooltip("Number of shots fired before a reload is required. Set to 0 for unlimited.")]
        [DefaultValue(6)]
        [Range(0, 100)]
        public int ClipSize;

        [DisplayName("VR Enabled")]
        [Tooltip("If true the gun will work in VR.")]
        [DefaultValue(true)]
        public bool VREnabled;

        [DisplayName("Mouse Look Enabled")]
        [Tooltip("If true the gun will only work in desktop mouse look mode. Press Escape to enter or exit mouse look mode.")]
        [DefaultValue(true)]
        public bool MouseLookEnabled;

        [DisplayName("Free Click Enabled")]
        [Tooltip("If true the gun will work with normal mouse clicks, outside of mouse look mode.\nNote that this mode can make aiming games trivial.")]
        [DefaultValue(true)]
        public bool FreeClickEnabled;

        [DisplayName("Free Cam Enabled")]
        [Tooltip("If true the gun will work while the user is in 'Free Cam' mode.")]
        [DefaultValue(true)]
        public bool FreeCamEnabled;

        private int ammo;
        private int shotsFired;
        private int shotsHit;
        private int score;
        private ControlPointType heldHand = ControlPointType.Invalid;

        private AgentInfo holdingAgent;

        SimpleData simpleData;

        protected override void SimpleInit()
        {
            simpleData = new SimpleData(this);
            simpleData.SourceObjectId = ObjectPrivate.ObjectId;

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
                simpleData.AgentInfo = holdingAgent;
                simpleData.ObjectId = holdingAgent.ObjectId;
                ammo = ClipSize;
                shotsFired = 0;
                shotsHit = 0;
                score = 0;
                unsubscribe += agent.Client.SubscribeToCommand(ShootCommand, OnTrigger, null).Unsubscribe;
                unsubscribe += agent.Client.SubscribeToCommand(ReloadCommand, Reload, null).Unsubscribe;
            }
            catch (System.Exception)
            {
                holdingAgent = null;
                simpleData.AgentInfo = null;
                simpleData.ObjectId = ObjectId.Invalid;
            }
        }

        void OnDrop(HeldObjectData data)
        {
            try
            {
                unsubscribe();
                unsubscribe = null;
                AgentPrivate user = ScenePrivate.FindAgent(holdingAgent.SessionId);

                // Do this after we get the user but before trying to use the user for chat in case they have left.
                holdingAgent = null;
                simpleData.AgentInfo = null;
                simpleData.ObjectId = ObjectId.Invalid;

                float accuracy = 0;
                if (shotsHit > 0)
                {
                    accuracy = 100.0f * (float)shotsHit / (float)shotsFired;
                }

                if (KeepScore)
                {
                    user.SendChat($"Final score: {score}. You hit {shotsHit} out of {shotsFired}, a hit accuracy of {accuracy.ToString("00.0")}%");
                }
            }
            catch (System.Exception) { }
        }
        bool reloading = false;

        void Reload(CommandData command)
        {
            if (ClipSize == 0) return;

            try
            {
                if (reloading == false)
                {
                    if (ReloadTime > 0.01)
                    {
                        reloading = true;
                        Sansar.Script.Timer.Create(TimeSpan.FromSeconds(ReloadTime), () =>
                        {
                            SendToAll(ReloadedEvent, simpleData);
                            ammo = ClipSize;
                            reloading = false;
                        });
                    }
                    else
                    {
                        SendToAll(ReloadedEvent, simpleData);
                        ammo = ClipSize;
                    }
                }
            }
            catch (System.Exception) { }
        }

        void OnTrigger(CommandData command)
        {
            try
            {
                if (!FreeCamEnabled && command.CameraControlMode == CameraControlMode.FlyCam)
                {
                    AgentPrivate user = ScenePrivate.FindAgent(holdingAgent.SessionId);
                    user.SendChat("This device does not work in Free Cam mode, return to 1st or 3rd person views to use this device.");
                    return;
                }

                switch(command.ControlPoint)
                {
                    case ControlPointType.DesktopGrab:
                        if (!MouseLookEnabled && command.MouseLookMode)
                        {
                            AgentPrivate user = ScenePrivate.FindAgent(holdingAgent.SessionId);
                            user.SendChat("This device does not work in desktop Mouse Look Mode: press Escape to enter or exit Mouse Look.");
                            return;
                        }
                        if (!FreeClickEnabled && !command.MouseLookMode)
                        {
                            AgentPrivate user = ScenePrivate.FindAgent(holdingAgent.SessionId);
                            user.SendChat("This device does not work in desktop Free Click Mode: press Escape to enter or exit Mouse Look.");
                            return;
                        }
                        break;
                    case ControlPointType.LeftTool:
                    case ControlPointType.RightTool:
                        if (!VREnabled)
                        {
                            AgentPrivate user = ScenePrivate.FindAgent(holdingAgent.SessionId);
                            user.SendChat("This device does not work in VR.");
                            return;
                        }

                        // If they grabbed it in desktop let them use it from whichever hand now? I guess?
                        if (heldHand != ControlPointType.DesktopGrab && command.ControlPoint != heldHand)
                        {
                            return;
                        }
                        break;
                    default:
                        break;
                }
                
                if (ClipSize > 0 && ammo <= 0)
                {
                    SendToAll(OutOfAmmoEvent, simpleData);
                    return;
                }

                SendToAll(ShotFiredEvent, simpleData);
                shotsFired++;
                ammo--;

                var targetAgent = ScenePrivate.FindAgent(command.TargetingComponent.ObjectId);
                if (targetAgent != null)
                {

                    shotsHit++;
                    score += PointsPerPlayer;
                    SendToAll(PlayerHitEvent, simpleData);

                    SimpleData targetSimpleData = new SimpleData(this);
                    targetSimpleData.AgentInfo = targetAgent.AgentInfo;
                    targetSimpleData.ObjectId = targetSimpleData.AgentInfo.ObjectId;
                    targetSimpleData.SourceObjectId = simpleData.SourceObjectId;

                    SendToAll(ShotHitEvent, targetSimpleData);
                }
                else
                {
                    ObjectPrivate targetObject = ScenePrivate.FindObject(command.TargetingComponent.ObjectId);
                    if (targetObject != null)
                    {
                        ITarget target = targetObject.FindScripts<ITarget>("Simple.Target").FirstOrDefault();
                        if (target != null 
                            && (!SameGroupRequired || target.GetGroupTag() == Group))
                        {
                            SimpleData targetSimpleData = new SimpleData(this);
                            targetSimpleData.AgentInfo = null;
                            targetSimpleData.ObjectId = targetObject.ObjectId;
                            targetSimpleData.SourceObjectId = simpleData.SourceObjectId;

                            SendToAll(ShotHitEvent, targetSimpleData);
                            score += target.Hit(holdingAgent, command);
                            shotsHit++;
                            return;
                        }
                    }

                    SendToAll(ShotMissEvent, simpleData);
                }
            }
            catch (System.Exception) { } // ignore exceptions for not found agents.
        }

        public interface ITarget
        {
            int Hit(AgentInfo agent, CommandData data);
            string GetGroupTag();
        }
    }
}