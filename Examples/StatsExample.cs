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
using System.Linq;
using System.Text;

// Inherit from AgentScript to get access to the Client to enable sending chat and dialogs to the agent
public class Stats : SceneObjectScript
{
    #region EditorProperties
    [DefaultValue("/help")]
    [DisplayName("Help")]
    public readonly string HelpCmd;

    [DefaultValue("/sceneinfo")]
    [DisplayName("Scene Info")]
    public readonly string ExpInfoCmd;

    [DefaultValue("/listagents")]
    [DisplayName("Agent List")]
    public readonly string ListAgentsCmd;

    [DefaultValue("/mypos")]
    [DisplayName("My Position")]
    public readonly string MyPosCmd;
    #endregion

    public override void Init()
    {
        // Displays all the registered chat commands
        ChatCommands[HelpCmd] = new ChatCommand
        {
            Action = (String s, AgentPrivate AgentPrivate) =>
            {
                StringBuilder builder  = new StringBuilder();
                foreach (KeyValuePair<string, ChatCommand> handler in ChatCommands)
                {
                    builder.AppendFormat($"\n{handler.Key} : {handler.Value.Description}");
                }

                AgentPrivate.SendChat(builder.ToString());
            },
            Description = "Display this message."
        };


        // Displays the ExperienceInfo fields
        ChatCommands[ExpInfoCmd] = new ChatCommand
        {
            Action = (String s, AgentPrivate AgentPrivate) =>
            {
                StringBuilder builder  = new StringBuilder();

                // Small action which only adds values that are not empty
                Action<string,object> append = (string name, object value)=>
                {
                    if(value != null && value.ToString().Length > 0)
                    {
                        builder.Append($"\n{name} {value.ToString()}");
                    }
                };

                // Add all the properties to the chat message
                SceneInfo info = ScenePrivate.SceneInfo;
                append("This experience is running build", info.BuildId);
                append("AvatarId", info.AvatarId);
                append("AccessGroup", info.AccessGroup);
                append("CompatVersion", info.CompatVersion);
                append("ProtoVersion", info.ProtoVersion);
                append("Configuration", info.Configuration);
                append("ExperienceId", info.ExperienceId);
                append("Experience", info.ExperienceName);
                append("LocationHandle", info.LocationHandle);
                append("InstanceId", info.InstanceId);
                append("LocationUri", info.SansarUri);
                append("Owner AvatarUuid", info.AvatarUuid);

                AgentPrivate.SendChat(builder.ToString());
            },
            Description = "Dumps the experience info object."
        };

        // Lists all agents currently in the scene
        ChatCommands[ListAgentsCmd] = new ChatCommand
        {
            Action = (String s, AgentPrivate AgentPrivate) =>
            {
                AgentPrivate.SendChat($"There are {ScenePrivate.AgentCount.ToString()} agents in the region.");

                // Build up a list of agents to send to the dialog.
                // The list is built up outside the coroutine as the dialog may be up for some
                // time and the list may change while it is being show.
                List<string> agents = new List<string>();
                foreach (var agent in ScenePrivate.GetAgents())
                {
                    agents.Add($"[{agent.AgentInfo.SessionId.ToString()}:{agent.AgentInfo.AvatarUuid}] {agent.AgentInfo.Name}");
                }

                StartCoroutine(ListAgents, agents, 10, AgentPrivate);
            },
            Description = "List all agents in the region."
        };

        // Report the current agent position
        ChatCommands[MyPosCmd] = new ChatCommand
        {
            Action = (String message, AgentPrivate AgentPrivate) =>
            {
                ObjectPrivate obj = ScenePrivate.FindObject(AgentPrivate.AgentInfo.ObjectId);
                if (obj != null)
                {
                    AgentPrivate.SendChat($"You are at {obj.Position}");
                }
                else
                {
                    AgentPrivate.SendChat($"Where are you? Can not find your avatar object.");
                }

            },
            Description = "Get your current position in world coordinates."
        };

        ScenePrivate.Chat.Subscribe(0, Chat.User, OnChat);
    }

    private void OnChat(ChatData data)
    {
        // Try to parse the message as a chat command and ignore it if it is not a known command
        string cmd = data.Message.Split(' ').First();
        if (ChatCommands.ContainsKey(cmd))
        {
            AgentPrivate agent=ScenePrivate.FindAgent(data.SourceId);
            if (agent != null)
            {
                ChatCommands[cmd].Action(data.Message, agent);
            }
        }
    }

    // This is run as a coroutine to easily paginate the list.
    private void ListAgents(List<string> lines, int maxLines, AgentPrivate Agent)
    {
        StringBuilder list = new StringBuilder();
        for (int i = 0; i < lines.Count; i += maxLines)
        {
            list.Clear();

            // Take (up to) the next maxLines
            var segment = lines.Skip(i).Take(maxLines);

            // Write out a header line
            list.AppendFormat($"Showing agent {i + 1}-{i + segment.Count()}\n\n\n");

            // Add each line
            foreach (string line in segment)
            {
                list.AppendLine(line);
            }

            // Show "More" for initial pages and "Okay" for the final page
            string right="MORE";
            if (i + maxLines >= lines.Count)
            {
                right = "OK";
            }

            // Show the dialog and wait for a response
            WaitFor(Agent.Client.UI.ModalDialog.Show, list.ToString(), "", right);

            // If "Cancel" or "Okay are clicked, exit the coroutine and stop showing the list.
            if (Agent.Client.UI.ModalDialog.Response != "MORE")
            {
                return;
            }
        }
    }

    // Tiny utility class to keep track of a command and a description for it
    private class ChatCommand
    {
        public Action<String, AgentPrivate> Action;
        public String Description;
    }

    // The chat command table
    private Dictionary<string, ChatCommand> ChatCommands = new Dictionary<string, ChatCommand>();

}