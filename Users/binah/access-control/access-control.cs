using System;
using System.Collections.Generic;
using System.Linq;
using Sansar.Script;
using Sansar.Simulation;
using System.Diagnostics;

// register agents on entry

public class AccessControl : SceneObjectScript
{
    #region EditorProperties
    [Tooltip("Comma separated list of user handles for users that are allowed to ban other users.\nThe scene owner is always an admin and does not need to be on this list.")]
    [DisplayName("Admin Handles")]
    public string AdminHandles;

    [DefaultValue(true)]
    [Tooltip("Allow anyone to enter without an access check")]
    [DisplayName("DoorsOpen")]
    public bool DoorsOpen;

    [DisplayName("Banned Destination")]
    [Tooltip("The destination to send banned users.")]
    [DefaultValue("https://atlas.sansar.com/experiences/katylina/you-have-been-expelled")]
    public string BannedDestination;

    [DefaultValue(true)]
    public bool DebugLogging;
    #endregion

    private RigidBodyComponent _rb;

    private Dictionary<string, string> _commandsUsage;

    // TODO: change these to HashSets
    private List<string> Admins = new List<string>(); // handle
    private bool hasAdmin = false;

    private List<string> Banned = new List<string>();

    public override void Init()
    {
        // get the holding container rigid body
        if (!ObjectPrivate.TryGetFirstComponent(out _rb))
        {
            if (DebugLogging) Log.Write("Script not running on an object with a physics volume!");
            return;
        }
        else
        {
            if (DebugLogging) Log.Write("RigidBody found!");
        }

        // Subscribe to Add User events
        // ---------------------------------------------->start coroutine TrackUser send it UserData
        ScenePrivate.User.Subscribe(User.AddUser, SessionId.Invalid, (UserData data) => StartCoroutine(TrackUser, data.User), true);
        if (DebugLogging) Log.Write("Subscribed to TrackUser");

        //Add admins, scene owner is automatically added when entering
        Admins.AddRange(AdminHandles.Trim().ToLower().Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries));
       
        _commandsUsage = new Dictionary<string, string>
        {
            { "/help", "" },
            { "/log", "" },
            { "/door", "[open or close]" },
            { "/ban", " [user-handle]" },
            { "/unban", "[user-handle]" },
        };

        ScenePrivate.Chat.Subscribe(0, null, onChat);
    }

    public class Visitor
    {
        public string Name { get; internal set; }
        public string Handle { get; internal set; }
        public AgentPrivate Agent { get; internal set;  }
    }

    private Dictionary<string, Visitor> Visitors = new Dictionary<string, Visitor>();

    bool IsAdmin(AgentPrivate agent)
    {
        return agent != null && Admins.Contains(agent.AgentInfo.Handle.ToLower());
    }

    bool IsBanned(AgentPrivate agent)
    {
        return Banned.Contains(agent.AgentInfo.Handle.ToLower());
    }

    private void TrackUser(SessionId userId)
    {
        AgentPrivate agent = ScenePrivate.FindAgent(userId);
        string handle = agent.AgentInfo.Handle.ToLower();
        Visitor visitor;

        if (DebugLogging) Log.Write(handle, " has entered.");

        bool isAdmin;
        //if scene owner add to admin
        if (handle == ScenePrivate.SceneInfo.AvatarId.ToLower())
        {
            Admins.Add(ScenePrivate.SceneInfo.AvatarId.ToLower());
            isAdmin = true;
        } else
        {
            isAdmin = IsAdmin(agent);
        }

        if(!hasAdmin)
        {
            hasAdmin = isAdmin;
        }

        //if visitor is not found in list add them to the list
        if (Visitors.TryGetValue(handle, out visitor))
        {
            if (DebugLogging) Log.Write(visitor.Name + " has been found in the visitor log.");
        } else
        {
            LogEntry(agent);
        }

        if (!IsBanned(agent))
        {
            //if the doors are open or the agent is an admin let them in
            if((DoorsOpen || isAdmin) || !hasAdmin)
            {
                SetAccess(agent);
            } else
            {
                StartCoroutine(AskForEntry, agent);
            }
        } else
        {
            //if already on banlist bannish them
            IEventSubscription timerEvent = Timer.Create(TimeSpan.FromSeconds(1), () => { Bannish(agent); });
        }

    }

    private void LogEntry(AgentPrivate agent)
    {
        Visitor visitor = new Visitor();
        visitor.Agent = agent;
        visitor.Name = agent.AgentInfo.Name; //used for pretty print 
        Visitors[agent.AgentInfo.Handle.ToLower()] = visitor;

        if (DebugLogging) Log.Write("Visitor entry has been logged");
    }

    private void SetAccess(AgentPrivate agent)
    {
        bool isBanned = IsBanned(agent);
        if (DebugLogging) Log.Write("Setting Access: " + !isBanned);
        agent.IgnoreCollisionWith(_rb, !isBanned);
    }

    private void Bannish(AgentPrivate agent)
    {
       if(!IsAdmin(agent))
        {
            try
            {
                agent.Client.TeleportToUri(BannedDestination);
                if (DebugLogging) Log.Write("Say goodbye to " + agent.AgentInfo.Name);
                return;
            }
            catch (NullReferenceException nre) { if (DebugLogging) Log.Write("Bannish", nre.Message); } // User Gone.
            catch (System.Exception e) { if (DebugLogging) Log.Write("Bannish", e.ToString()); }
        } else
        {
            if (DebugLogging) Log.Write("You can't ban "+ agent.AgentInfo.Name +"they're an admin!");
        }
    }

    private void GrantEntry(AgentPrivate agent)
    {
        //if on ban list remove them
        if(IsBanned(agent))
        {
            RemoveUserBan(agent);
        }
        //setAccess
        SetAccess(agent);
    }

    private void AskForEntry(AgentPrivate agent)
    {
        if (DebugLogging) Log.Write("Checking Access");
        string message = "Allow " + agent.AgentInfo.Name + " to enter?";
        Visitor admin = Visitors[Admins[0]];
        ModalDialog modalDialog = admin.Agent.Client.UI.ModalDialog;
        modalDialog.Show(message, "Yes", "No", (opc) =>
        {
            if(modalDialog.Response == "Yes")
            {
                if (DebugLogging) Log.Write("Entry was granted.");
                GrantEntry(agent);
            } else
            {
                if (DebugLogging) Log.Write("Entry was denied.");
                BanUser(agent);
            }
        });
    }

    private void BanUser(AgentPrivate agent)
    {
        if(!IsBanned(agent))
        {
            Banned.Add(agent.AgentInfo.Handle.ToLower());
            if (DebugLogging) Log.Write(agent.AgentInfo.Name + " has been added to banlist");
        }

        IEventSubscription timerEvent = Timer.Create(TimeSpan.FromSeconds(1), () => { Bannish(agent); });
    }

    private void RemoveUserBan(AgentPrivate agent)
    {
        Banned.RemoveAll(a => a == agent.AgentInfo.Handle.ToLower());
        if (DebugLogging) Log.Write(agent.AgentInfo.Name + " has been removed from the banlist");
    }

    //Chat Commands

    private void onShowHelp(AgentPrivate agent)
    {
        string helpMessage = "MediaChatCommand usage:";
        foreach (var kvp in _commandsUsage)
        {
            helpMessage += "\n" + kvp.Key + " " + kvp.Value;
        }

        try
        {
            agent.SendChat(helpMessage);
        }
        catch
        {
            // Agent left
            if (DebugLogging) Log.Write("Possible race condition and they already logged off.");
        }
    }

    private void onShowLog(AgentPrivate agent)
    {

        string message = "There have been " + Visitors.Count + " visitors";
        message += "\nThe doors are open: " + DoorsOpen;

        message += "\nVisitor list: ";

        foreach (var visitor in Visitors)
        {
            bool isAdmin = IsAdmin(visitor.Value.Agent);
            bool isBanned = IsBanned(visitor.Value.Agent);

            message += "\nName: " + visitor.Value.Name;

            if (isAdmin)
            {
                message += "\n - isAdmin: " + isAdmin;
            } else
            {
                message += "\n - isBanned " + isBanned;
            }
        }

        message += "\nBanned list: ";
        foreach (var b in Banned)
        {
            message += "\n" + b;
        }

        agent.SendChat(message);
    }

    private void onDoorCommand(AgentPrivate agent, string param)
    {
        if(String.IsNullOrEmpty(param))
        {
            if (DebugLogging) Log.Write("Door command did not pass a paramter. Doors are open remains = " + DoorsOpen);
            return;
        }

        DoorsOpen = param == "open" ? true : false;
        if (DebugLogging) Log.Write("Doors are open = " + DoorsOpen);
    }

    private void onBanCommand(AgentPrivate agent, string param)
    {
        if (DebugLogging) Log.Write("Ban Command triggered. " + param);

        if (String.IsNullOrEmpty(param))
        {
            if (DebugLogging) Log.Write("Ban command did not pass a user handle parameter");
            return;
        }

        try
        {
            Visitor revoked = Visitors[param.ToLower()];
            BanUser(revoked.Agent);
        } catch (Exception e)
        {
            if (DebugLogging) Log.Write("Ban Exception: " + e);
        }
    }

    private void onUnBanCommand(AgentPrivate agent, string param)
    {
        if (DebugLogging) Log.Write("UnBan Command triggered. " + param);

        if (String.IsNullOrEmpty(param))
        {
            if (DebugLogging) Log.Write("UnBan Command for did not pass a user handle parameter");
            return;
        }

        try
        {
            Visitor granted = Visitors[param.ToLower()];
            RemoveUserBan(granted.Agent);
        }
        catch (Exception e)
        {
            if (DebugLogging) Log.Write("UnBan Exception: " + e);
        }
    }
        

    private void onChat(ChatData data)
    {
        AgentPrivate agent = ScenePrivate.FindAgent(data.SourceId);
        if(IsAdmin(agent))
        {
            string[] chatWords = data.Message.Split(' ');

            if (chatWords.Length < 1)
            {
                if (DebugLogging) Log.Write("Chat message not long enough.");
                return;
            }

            string command = chatWords[0];

            if (!_commandsUsage.ContainsKey(command))
            {
                if (DebugLogging) Log.Write("Command not found in dictionary." + command);
                return;
            }

            if (DebugLogging) Log.Write("Command = " + command);

            switch(command)
            {
                case "/help":
                    onShowHelp(agent);
                    break;
                case "/log":
                    onShowLog(agent);
                    break;
                case "/door":
                    onDoorCommand(agent, chatWords[1]);
                    break;
                case "/ban":
                    onBanCommand(agent, chatWords[1]);
                    break;
                case "/unban":
                    onUnBanCommand(agent, chatWords[1]);
                    break;
                //case "/vote":
                default: break;
            }
        }
    }
}
