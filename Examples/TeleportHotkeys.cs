/* (C) 2017 - 2018 Linden Research, Inc. All Rights Reserved.
 * 
 * Linden Research, Inc. ("Linden Lab") owns (or has the necessary rights to) all information contained herein, all of which is CONFIDENTIAL and PROPRIETARY to Linden Lab. Use of any such information is governed by the Employee Proprietary Information & Inventions Agreement you entered into upon commencement of your employment with Linden Lab. All other use, dissemination or reproduction of this information is strictly prohibited.
 * 
 * NOTWITHSTANDING THE FOREGOING, ALL LINDEN LAB SOURCE CODE IS PROVIDED STRICTLY ON AN "AS IS" BASIS. LINDEN LAB MAKES NO WARRANTIES, EXPRESS, IMPLIED, STATUTORY OR OTHERWISE, INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OF TITLE, NONINFRINGEMENT, MERCHANTABILITY, ACCURACY, SATISFACTORY QUALITY OR FITNESS FOR A PARTICULAR PURPOSE. */

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
using System.ComponentModel;

public class TeleportHotkeys : SceneObjectScript
{
    [Description("Hotkey to press to initiate a teleport")]
    public string TeleportHotkey = "Key_F12";

    [Description("Comma separated list of experiences to visit.")]
    public string Experiences = "mars-outpost-alpha, toppleton-toy-town, egyptian-tomb, colossus-rising, origin-cinema";

    [Description("PersonHandle which owns the experiences.")]
    public string PersonaHandle = "sansar-studios";

    public bool TP_Everyone = false;

    public float TP_Delay = 0.0f;

    // Parsed list of experience names
    private string[] experiences;

    public override void Init()
    {
        if (TP_Delay < 0.1f) TP_Delay = 0.1f;

        // Split the list into specific entries
        experiences = Experiences.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

        if (experiences.Length == 0)
        {
            Log.Write($"No experiences found in {Experiences}");
            return;
        }
        // Listen for new agents to join the scene
        ScenePrivate.User.Subscribe(User.AddUser, NewUser);
    }

    private void SubscribeToHotkey(SessionId userId)
    {
        AgentPrivate agent = ScenePrivate.FindAgent(userId);

        // Lookup the scene object for this agent
        ObjectPrivate agentObejct = ScenePrivate.FindObject(agent.AgentInfo.ObjectId);
        if (agentObejct == null)
        {
            Log.Write($"Unable to find a ObjectPrivate component for user {userId}");
            return;
        }

        // Lookup the animation component. There should be just one, so grab the first
        AnimationComponent animationComponent = null;
        if (!agentObejct.TryGetFirstComponent(out animationComponent))
        {
            Log.Write($"Unable to find an animation component on user {userId}");
            return;
        }

        // Listen for a key press. Since the agent will be teleporting away, do no request a persistent subscription 
        animationComponent.Subscribe(TeleportHotkey, TeleportToNext, false);
    }

    void NewUser(UserData data)
    {
        // Set up the the hotkey listener.
        SubscribeToHotkey(data.User);
    }

    private void TeleportToNext(AnimationData data)
    {
        // Find the index of the current experience in the list
        int index = Array.IndexOf(experiences, ScenePrivate.SceneInfo.LocationHandle);

        // Get the next index, wrapping around. If the current location is not in the list
        // IndexOf returns -1, so the destination will be the first item in the list.
        index = (index + 1) % experiences.Length;

        if (TP_Everyone == false)
        {
            // Lookup the agent
            AgentPrivate agent = ScenePrivate.FindAgent(data.ComponentId.ObjectId);
            if (agent == null)
            {
                Log.Write($"Unable to find an agent for the component {data.ComponentId}");
                return;
            }

            // Actually do the teleport
            agent.Client.TeleportToLocation(PersonaHandle, experiences[index]);
        }
        else
        {
            AgentPrivate agent = ScenePrivate.FindAgent(data.ComponentId.ObjectId);
            StartCoroutine(TeleportAll, PersonaHandle, experiences[index], agent);
        }
    }

    private void TeleportAll(string persona, string experience, AgentPrivate initiator)
    {
        System.Collections.Generic.List<AgentPrivate> agents = new System.Collections.Generic.List<AgentPrivate>();

        foreach (AgentPrivate agent in ScenePrivate.GetAgents())
        {
            if (agent != initiator) agents.Add(agent);
        }
        agents.Add(initiator);

        foreach (AgentPrivate agent in agents)
        {
            try
            {
                if (agent.IsValid) // Agent left a few seconds ago, skip it.
                {
                    agent.Client.TeleportToLocation(persona, experience);
                    Wait(TimeSpan.FromSeconds(TP_Delay));
                }
                else
                {
                    Log.Write(LogLevel.Warning, "TeleportAll", $"Race 1, agent {agent.AgentInfo.Name} broken handle.");
                }
            }
            catch (NullReferenceException)
            {
                // Agent _just_ left, just skip it.
                Log.Write(LogLevel.Warning, "TeleportAll", $"Race 2, agent {agent.AgentInfo.Name} broken handle exception.");
            }
        }
    }
}