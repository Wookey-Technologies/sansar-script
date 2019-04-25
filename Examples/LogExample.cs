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
using System.Collections.Generic;
using System.Text;

// Put this script in 1 object in the scene. Use it via chat:
//   /console show
//         Show a dialog of the script logs
//   /console clear
//         Clear the script log
//   /console help
//         Show a list of console commands this script supports.
public class LogExample : SceneObjectScript
{
    // The trigger word can be set in the editor
    [DefaultValue("/console")]
    [DisplayName("Chat Command")]
    public readonly string Trigger = "/console";

    // Init will be called by the script loader after the constructor and after any public fields have been initialized.
    public override void Init()
    {
        Log.Write("Console Example started.");
        chatHandlers["show"] = Show;
        chatHandlers["help"] = Help;
        chatHandlers["clear"] = Clear;

        // No command defaults to show
        chatHandlers[""] = Show;

        ScenePrivate.Chat.Subscribe(0, null, OnChat);
    }
    
    private Dictionary<string, Action<AgentPrivate>> chatHandlers = new Dictionary<string, Action<AgentPrivate>>();

    // send the log messages
    private void Show(AgentPrivate agent)
    {
        StringBuilder list = new StringBuilder();
        foreach (var message in Log.Messages)
        {
            list.AppendLine(message.Text);
        }
        list.AppendLine("End of messages");
        agent.SendChat(list.ToString());
    }

    // send the help text
    private void Help(AgentPrivate agent)
    {
        StringBuilder commands = new StringBuilder();
        commands.Append("Recognized commands are ");

        foreach (string command in chatHandlers.Keys)
        {
            commands.AppendFormat("{0} ", command);
        }
        agent.SendChat(commands.ToString());
    }

    // clear the log
    private void Clear(AgentPrivate agent)
    {
        Log.Clear();
        agent.SendChat("Console log cleared");
    }

    private void OnChat(ChatData data)
    {
        // ignore any messages that are not from an agent
        if (data.SourceId != SessionId.Invalid)
        {
            AgentPrivate agent= ScenePrivate.FindAgent(data.SourceId);
            if (agent == null)
            {
                Log.Write(LogLevel.Warning, "Unable to find the agent who was talking.");
                return;
            }
            if (data.Message.StartsWith(Trigger))
            {
                string command = data.Message.Substring(Trigger.Length).Trim();

                if(chatHandlers.ContainsKey(command))
                {
                    chatHandlers[command](agent);
                }
                else
                {
                    chatHandlers["help"](agent);
                }
            }
        }
    }
}