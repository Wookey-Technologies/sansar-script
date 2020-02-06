using System;
using System.Collections.Generic;
using System.Linq;
using Sansar.Script;
using Sansar.Simulation;
using System.Diagnostics;

public class ResetScene : SceneObjectScript
{
    #region EditorProperties
    [DisplayName("Reset Event")]
    [Tooltip("Event name to reset the scene")]
    [DefaultValue("reset-scene")]
    public string ResetEvent;

    [DefaultValue(true)]
    public bool EventEnabled;

    [DisplayName("Chat Command to reset scene")]
    [Tooltip("Chat command to reset the scene")]
    [DefaultValue("reset-scene")]
    public string ResetChatCommand;

    [DefaultValue(true)]
    public bool ChatEnabled;

    [DefaultValue("Reset Scene!")]
    public Interaction MyInteraction;

    [DefaultValue(true)]
    public bool InteractionEnabled;

    [DefaultValue(true)]
    public bool DebugLogging;
    #endregion

    private string _resetChatCommand;

  public override void Init()
  {
        if(EventEnabled)
        {
            SubscribeToScriptEvent(ResetEvent, (ScriptEventData data) =>
            {
                onReset();
            });

            if (DebugLogging) Log.Write("ResetScene script subscribed to", ResetEvent);
        }

        if(ChatEnabled)
        {
            _resetChatCommand = "/" + ResetChatCommand;
            ScenePrivate.Chat.Subscribe(0, null, onChat);

            if (DebugLogging) Log.Write("ResetScene script subscribed to chat command ", _resetChatCommand);
        }

        if(InteractionEnabled)
        {
            string basePrompt = MyInteraction.GetPrompt();
            MyInteraction.Subscribe((InteractionData idata) =>
            {
                onReset();
            });

            if (DebugLogging) Log.Write("ResetScene script subscribed to interaction.");
        } else
        {
            MyInteraction.SetPrompt("");
        }

    }

    private void onReset()
    {
        if (DebugLogging) Log.Write("Resetting world.");
        ScenePrivate.ResetScene();
    }

    private void onChat(ChatData data)
    {
        var cmds = data.Message.Split(new Char[] { ' ' });
        
        if (DebugLogging) Log.Write("Chat command " + cmds[0]);

        AgentPrivate agent = ScenePrivate.FindAgent(data.SourceId);
        string handle = agent.AgentInfo.Handle.ToLower();

        if (handle == ScenePrivate.SceneInfo.AvatarId.ToLower())
        {
            if (cmds[0] == _resetChatCommand)
            {
                onReset();
            }
        } else
        {
            if (DebugLogging) Log.Write("You must be the owner of the scene");
        }
    }
}
