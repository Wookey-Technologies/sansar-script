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
    [DisplayName("Admin Handles CSV")]
    public string AdminHandles;

    [DefaultValue(true)]
    [Tooltip("Allow anyone to enter without an access check")]
    [DisplayName("DoorsOpen")]
    public readonly bool DoorsOpen;

    [DefaultValue("/showlog")]
    [DisplayName("Chat Command")]
    public readonly string VisitorListCommand = "/showlog";

    [DefaultValue(true)]
    public bool DebugLogging;
    #endregion

    private RigidBodyComponent _rb;

    private List<string> Admins = new List<string>(); // handle

    public override void Init()
    {
        // get the holding vestibule rigid body
        if (!ObjectPrivate.TryGetFirstComponent(out _rb))
        {
            if (DebugLogging) Log.Write("Script not running on an object with a physics volume!");
        } else
        {
            if (DebugLogging) Log.Write("RigidBody found!");
        }

        // Subscribe to Add User events
        // ---------------------------------------------->start coroutine TrackUser send it UserData
        ScenePrivate.User.Subscribe(User.AddUser, SessionId.Invalid, (UserData data) => StartCoroutine(TrackUser, data.User), true);
        if (DebugLogging) Log.Write("Subscribed to TrackUser");

        //Add admins, scene owner is automatically added
        Admins.AddRange(AdminHandles.Trim().ToLower().Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries));
        Admins.Add(ScenePrivate.SceneInfo.AvatarId);

        // listen for commands
        ScenePrivate.Chat.Subscribe(0, null, ShowLogCommand);
    }

    bool IsAdmin(AgentPrivate agent)
    {
        if (DebugLogging) Log.Write(agent.AgentInfo.Handle + " IsAdmin: " + Admins.Contains(agent.AgentInfo.Handle.ToLower()));
        return agent != null && Admins.Contains(agent.AgentInfo.Handle.ToLower());
    }

    bool IsBanned(AgentPrivate agent)
    {
        //isAdmin ? false : 

        //if (DebugLogging) Log.Write(agent.AgentInfo.Handle + " IsBanned: " + );
        return agent != null && Admins.Contains(agent.AgentInfo.Handle.ToLower());
    }

    public class Visitor
    {
        public string Name { get; internal set; }
        public AgentPrivate Agent { get; internal set;  }
        public bool isAdmin { get; internal set; }
        public bool isBanned { get; internal set; }
    }

    private Dictionary<string, Visitor> Visitors = new Dictionary<string, Visitor>();

    private void TrackUser(SessionId userId)
    {
        AgentPrivate agent = ScenePrivate.FindAgent(userId);
        string name = agent.AgentInfo.Name;
        Visitor visitor;

        if (DebugLogging) Log.Write(name, " has entered.");

        //if visitor is not found in list add them to the list
        if (!Visitors.TryGetValue(name, out visitor))
        {
            //log user entry
            LogEntry(agent);
        }

        SetAccess(agent);

        if(IsBanned(agent))
        {
            //ask for permission to enter

        }

    }

    private void LogEntry(AgentPrivate agent)
    {
        int key = Visitors.Count;
        key++;
        Visitor visitor = new Visitor();
        visitor.Agent = agent;
        visitor.Name = agent.AgentInfo.Name;
        visitor.isAdmin = IsAdmin(agent);
        visitor.isBanned = visitor.isAdmin ? false : !DoorsOpen;
        Visitors[key.ToString()] = visitor;

        if (DebugLogging) Log.Write("Visitor entry has been logged");
    }

    private void ShowLogCommand(ChatData data)
    {
        // Checking the message is the fastest thing we could do here. Discard anything that isn't the command we are looking for.
        if (data.Message != VisitorListCommand)
        {
            return;
        }

        AgentPrivate agent = ScenePrivate.FindAgent(data.SourceId);
        if (agent == null)
        { 
            if (DebugLogging) Log.Write("Possible race condition and they already logged off.");
            return;
        }

        if (IsAdmin(agent) || agent.AgentInfo.AvatarUuid == ScenePrivate.SceneInfo.AvatarUuid)
        {
            //StartCoroutine(ShowStats, agent);
            ShowStats(agent);
        }
    }

    private string getLogMessage()
    {
        string message = "There have been " + Visitors.Count + " visitors:\n";
        foreach (var visitor in Visitors)
        {
            message += "Name: " + visitor.Value.Name;
            message += " - isAdmin: " + visitor.Value.isAdmin + ", isBanned " + visitor.Value.isBanned;
            message += " - isBanned " + visitor.Value.isBanned;
        }
        return message;
    }

    private void ShowStats(AgentPrivate agent)
    {
        agent.SendChat(getLogMessage());
    }

    private void SetAccess(AgentPrivate agent)
    {
        if (DebugLogging) Log.Write("Setting Access: " + IsBanned(agent));
        agent.IgnoreCollisionWith(_rb, IsBanned(agent));
    }

    private void CheckAccess()
    {
        if (DebugLogging) Log.Write("Checking Access");

    }

}
