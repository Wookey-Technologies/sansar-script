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
using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace PewPewExample
{
    [Tooltip("Add this script to a grabbable tool to make a 'ban hammer' - the bans are temporary and local only, but easy and fun!")]
    public class BanHammer : SceneObjectScript
    {
        [DisplayName("Ban Command")]
        [Tooltip("Input Command to ban the selected user. Default: 'Trigger'")]
        [DefaultValue("Trigger")]
        public string BanCommand;

        [DisplayName("Info Command")]
        [Tooltip("Input Command to review bans and manually unban. Default: 'SecondaryAction'")]
        [DefaultValue("SecondaryAction")]
        public string InfoCommand;

        [DisplayName("Banned Destination")]
        [Tooltip("The destination to send banned users.")]
        [DefaultValue("https://atlas.sansar.com/experiences/sansar-studios/mars-outpost-alpha")]
        public string BannedDestination;

        [Tooltip("Played at the location of a user that is ejected and banned.")]
        [DisplayName("Banned User Sound")]
        public SoundResource UserEjectedSound;

        [Tooltip("Played at the location of a user that tries to pick up the tool but is not authorized to use it.")]
        [DisplayName("Unauthorized User Sound")]
        public SoundResource UnauthorizedSound;
        
        [Tooltip("Played at the mods location if trying to ban something that isn't a user.")]
        [DisplayName("Failed Selection Sound")]
        public SoundResource FailedSelectionSound;

        [DefaultValue(50)]
        [Range(0, 100)]
        [DisplayName("Sounds Volume")]
        [Tooltip("The sound volume used for all sounds.")]
        public float LoudnessPct;

        [Tooltip("Comma separated list of user handles for users that are allowed to ban other users.\nThe scene owner is always an admin and does not need to be on this list.")]
        [DisplayName("Admin Handles CSV")]
        public string AdminHandles;

        [DisplayName("Ban Duration")]
        [Tooltip("The length a banned user stays banned for, in minutes. Set to 0 for indefinite. Ban lists are reset if the server restarts.")]
        [DefaultValue(10)]
        [Range(0, 120)]
        public int BanDuration;

        [Tooltip("If true the object will return to its original position and orientation when dropped.")]
        [DisplayName("Reset Position On Drop")]
        [DefaultValue(true)]
        public bool ResetPositionOnDrop;

        [Tooltip("Cheese.\nEnable to shout a cheesy parting message in text chat when a user is banned.")]
        [DefaultValue(false)]
        [DisplayName("Cheese")]
        public bool EnableCheese;

        [DisplayName("Log Errors")]
        [Tooltip("Enables printing non-fatal errors to script debug console.\nThese are errors that will not kill the hammer entirely, and some are expected under normal operation.")]
        [DefaultValue(false)]
        public bool DebugSpam;

        private float LoudnessPercentToDb(float loudnessPercent)
        {
            loudnessPercent = Math.Min(Math.Max(loudnessPercent, 0.0f), 100.0f);
            return 60.0f * (loudnessPercent / 100.0f) - 48.0f;
        }

        private ControlPointType heldHand = ControlPointType.Invalid;
        private AgentPrivate holdingAgent;
        private Dictionary<string,long> Banned = new Dictionary<string, long>(); // handle, expires
        private List<string> Admins = new List<string>(); // handle
        private PlaySettings SoundSettings = new PlaySettings();
        private Sansar.Vector OriginalPosition;
        private Sansar.Quaternion OriginalOrientation;
        private RigidBodyComponent RigidBody;
        private Action unsubscribe;
        private Random rnd = new Random();
        private List<string> cheese = new List<string>()
        {
            "BANNED!",
            "Welcome to the BAN ZONE!",
            "BAN BAN!",
            "Don't let the door hit ya on the way out!",
            "Hasta la vista baby",
            "Later alligator!",
            "You won't be missed!",
            "In a 32-bit world, you're a 2-bit user.",
            "Bye.",
            "You're sufferin' from delusions of adequacy.",
            "Don't call us, we'll call you.",
            "You should leave.",
            "Hate to cut this short, but . . .",
            "Show's over.",
            "This might sting a little...",
            "You don't have to go home but you can't stay here.",
            "You are the weakest link, goodbye!"
        };

        public override void Init()
        {
            Script.UnhandledException += (object o, Exception e) => { if (DebugSpam) Log.Write("UnhandledException", "[" + (o?.ToString() ?? "NULL") +"] " + e.ToString()); };

            SoundSettings.Loudness = LoudnessPercentToDb(LoudnessPct);
            Admins.AddRange(AdminHandles.Trim().ToLower().Split(new char[]{ ',',' '}, StringSplitOptions.RemoveEmptyEntries));
            Admins.Add(ScenePrivate.SceneInfo.AvatarId);
            OriginalOrientation = ObjectPrivate.InitialRotation;
            OriginalPosition = ObjectPrivate.InitialPosition;

            if (ObjectPrivate.TryGetFirstComponent(out RigidBody))
            {
                if (!RigidBody.GetCanGrab()) RigidBody.SetCanGrab(true, (data) => { if (!data.Success) Log.Write("Could not set ModHammer to grabbable - won't be able to pick up the gun."); });
                RigidBody.SubscribeToHeldObject(HeldObjectEventType.Grab, OnPickup);
                RigidBody.SubscribeToHeldObject(HeldObjectEventType.Release, OnDrop);
            }

            ScenePrivate.User.Subscribe("AddUser", AddUser);

            if (BanDuration > 0) Timer.Create(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1), CleanupBans);
        }

        bool IsBanned(AgentPrivate user) { return user != null && Banned.ContainsKey(user.AgentInfo.Handle.ToLower());  }
        bool IsAdmin(AgentPrivate user) { return user != null && Admins.Contains(user.AgentInfo.Handle.ToLower()); }

        void AddUser(UserData data)
        {
            try
            {
                AgentPrivate newUser = ScenePrivate.FindAgent(data.User);
                if (IsBanned(newUser))
                {
                    Wait(TimeSpan.FromSeconds(2));
                    bool admin = IsAdmin(newUser);
                    IEventSubscription timerEvent = Timer.Create(TimeSpan.FromSeconds(admin? 300 : 10), () => { Bannish(newUser); });
                    WaitFor(newUser.Client.UI.ModalDialog.Show, "You are banned." + (admin ? "\n( ;) )" : ""), admin ? "You can't ban me!" : "Okay", "Bye");

                    if (timerEvent.Active) timerEvent.Unsubscribe();
                    if (newUser.Client.UI.ModalDialog.Response == "You can't ban me!")
                    {
                        Banned.Remove(newUser.AgentInfo.Handle.ToLower());
                        return;
                    }
                    Bannish(newUser);
                }
            }
            catch (NullReferenceException nre) { if (DebugSpam) Log.Write("AddUser", nre.Message); } // ignore exceptions for not found agents.
            catch (Exception e) { if (DebugSpam) Log.Write("AddUser", e.ToString()); }
        }

        void CleanupBans()
        {
            try
            {
                List<string> toRemove = new List<string>();
                foreach (var ban in Banned) if (ban.Value < Stopwatch.GetTimestamp()) toRemove.Add(ban.Key);
                foreach (var banToRemove in toRemove) Banned.Remove(banToRemove);
            }
            catch (NullReferenceException nre) { if (DebugSpam) Log.Write("CleanupBans", nre.Message); } // User Gone.
            catch (System.Exception e) { if (DebugSpam) Log.Write("CleanupBans", e.ToString()); }
        }

        void Bannish(AgentPrivate user, string uri = null)
        {
            try { user.Client.TeleportToUri(uri ?? BannedDestination); }
            catch (NullReferenceException nre ) { if (DebugSpam) Log.Write("Bannish", nre.Message); } // User Gone.
            catch (System.Exception e) { if (DebugSpam) Log.Write("Bannish", e.ToString()); }
        }

        void OnPickup(HeldObjectData data)
        {
            try {
                unsubscribe?.Invoke();
                unsubscribe = null;
                holdingAgent = ScenePrivate.FindAgent(data.HeldObjectInfo.SessionId);
                if (IsAdmin(holdingAgent))
                {
                    heldHand = data.HeldObjectInfo.ControlPoint;
                    unsubscribe = holdingAgent.Client.SubscribeToCommand(BanCommand, OnBan, null).Unsubscribe;
                    unsubscribe += holdingAgent.Client.SubscribeToCommand(InfoCommand, OnInfo, null).Unsubscribe;
                }
                else
                {
                    if (UnauthorizedSound != null) ScenePrivate.PlaySoundAtPosition(UnauthorizedSound, ScenePrivate.FindObject(holdingAgent.AgentInfo.ObjectId).Position, SoundSettings);
                    holdingAgent.Client.UI.ModalDialog.Show("This tool is only usable by designated users. Not you.\nPlease drop the tool or you will be forced to drop it.", "Ok", "");
                    AgentPrivate invalidUser = holdingAgent;
                    Timer.Create(TimeSpan.FromSeconds(60), () => { if (holdingAgent == invalidUser) Bannish(holdingAgent, ScenePrivate.SceneInfo.SansarUri); });
                }
                unsubscribe += () => { unsubscribe = null; holdingAgent = null; };
            }
            catch (NullReferenceException nre) { if (DebugSpam) Log.Write("OnPickup", nre.Message); } // ignore exceptions for not found agents.
            catch (Exception e) { if (DebugSpam) Log.Write("OnPickup", e.ToString()); holdingAgent = null; }
        }

        void OnDrop(HeldObjectData data)
        {
            try {
                unsubscribe?.Invoke();
                if (ResetPositionOnDrop)
                {
                    RigidBody.SetPosition(OriginalPosition);
                    RigidBody.SetOrientation(OriginalOrientation);
                    RigidBody.SetLinearVelocity(Sansar.Vector.Zero);
                    RigidBody.SetAngularVelocity(Sansar.Vector.Zero);
                }
            }
            catch (NullReferenceException nre) { if (DebugSpam) Log.Write("OnDrop", nre.Message); } // ignore exceptions for not found agents.
            catch (Exception e) { if (DebugSpam) Log.Write("OnDrop", e.ToString()); }
        }

        void OnBan(CommandData command)
        {
            try {
                if (command.ControlPoint != heldHand && command.ControlPoint != ControlPointType.DesktopGrab && heldHand != ControlPointType.DesktopGrab) return;

                var targetAgent = ScenePrivate.FindAgent(command.TargetingComponent.ObjectId);
                if (targetAgent != null)
                {
                    string banButton = EnableCheese ? "Ban Ban!" : "Ban";
                    WaitFor(holdingAgent.Client.UI.ModalDialog.Show, "Would you like to ban " + targetAgent.AgentInfo.Name + " (" + targetAgent.AgentInfo.Handle.ToLower() + ") from this scene for " + BanDuration + " minutes?", banButton, EnableCheese ? "Never mind..." : "Cancel");
                    if (holdingAgent.Client.UI.ModalDialog.Response == banButton)
                    {
                        Bannish(targetAgent);
                        Banned[targetAgent.AgentInfo.Handle.ToLower()] = Stopwatch.GetTimestamp() + TimeSpan.FromMinutes(BanDuration).Ticks;
                        if (EnableCheese) ScenePrivate.Chat.MessageAllUsers("HEY " + targetAgent.AgentInfo.Name + "! " + cheese[rnd.Next(cheese.Count)]);
                        if (UserEjectedSound != null) ScenePrivate.PlaySoundAtPosition(UserEjectedSound, command.TargetingPosition, SoundSettings);
                    }
                }
                else if (FailedSelectionSound != null) ScenePrivate.PlaySoundAtPosition(FailedSelectionSound, command.TargetingOrigin, SoundSettings);
            }
            catch (NullReferenceException nre) { if (DebugSpam) Log.Write("OnBan", nre.Message); }
            catch (Exception e) { if (DebugSpam) Log.Write("OnBan",e.ToString()); } // ignore exceptions for not found agents.
        }

        void OnInfo(CommandData command)
        {
            try {
                if (command.ControlPoint != heldHand && command.ControlPoint != ControlPointType.DesktopGrab && heldHand != ControlPointType.DesktopGrab) return;

                if (Banned.Count == 0)
                {
                    holdingAgent.Client.UI.ModalDialog.Show("There are no banned users right now.", "Ok", "");
                    return;
                }

                string msg = "The following users are banned:";
                foreach (var ban in Banned) msg += "\n" + ban.Key + " for " + TimeSpan.FromTicks(ban.Value - Stopwatch.GetTimestamp()).Minutes + " more minutes.";

                WaitFor(holdingAgent.Client.UI.ModalDialog.Show, msg, "Manage Bans", "Ok");
                if (holdingAgent.Client.UI.ModalDialog.Response == "Manage Bans")
                {
                    List<string> toRemove = new List<string>();
                    foreach (var ban in Banned)
                    {
                        WaitFor(holdingAgent.Client.UI.ModalDialog.Show, "Remove ban for " + ban.Key + "?", "Remove", "Keep");
                        if (holdingAgent.Client.UI.ModalDialog.Response == "Remove") toRemove.Add(ban.Key);
                    }

                    foreach (var b in toRemove) Banned.Remove(b);
                    holdingAgent.Client.UI.ModalDialog.Show("Ban review complete.", "Ok", "");
                }
            }
            catch (NullReferenceException nre) { if (DebugSpam) Log.Write("OnInfo", nre.Message); }
            catch (Exception e) { if (DebugSpam) Log.Write("OnInfo", e.ToString()); } // ignore exceptions for not found agents.
        }
    }
}