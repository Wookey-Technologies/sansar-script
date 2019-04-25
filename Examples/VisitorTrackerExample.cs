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

/* Use oroutines as an event callback to easily track visitors to a scene.
 * Every visitor triggers an AddUser event which will start a coroutine for that visitor.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using Sansar.Script;
using Sansar.Simulation;
using System.Diagnostics;

// This example shows how to use coroutines and events to track agents entering and leaving the scene
public class VisitorTrackerExample : SceneObjectScript
{
    #region EditorProperties
    [DefaultValue(false)]
    [DisplayName("Allow Anyone To Use")]
    public readonly bool PublicAccess;

    [DefaultValue("/visitorlist")]
    [DisplayName("Chat Command")]
    public readonly string VisitorListCommand = "/visitorlist";
    #endregion

    public override void Init()
    {
        // Subscribe to Add User events
        // Events can be handled by anonymous methods
        ScenePrivate.User.Subscribe(User.AddUser, SessionId.Invalid, (UserData data) => StartCoroutine(TrackUser, data.User), true);

        // listen for commands
        ScenePrivate.Chat.Subscribe(0, null, OwnerCommand);
    }

    public  class Visitor 
    {
        public TimeSpan TotalTime { get; internal set; }
        public long VisitStarted { get; internal set; }
        public bool Here { get; internal set; } = false;

        public TimeSpan ThisVisitSoFar
        {
            get { return TimeSpan.FromTicks(Stopwatch.GetTimestamp() - VisitStarted); }
        }
    }

    private Dictionary<string, Visitor> Visitors = new Dictionary<string, Visitor>();

    // There will be one instance of this coroutine per active user in the scene
    private void TrackUser(SessionId userId)
    {
        // Lookup the name of the agent. This is looked up now since the agent cannot be retrieved after they
        // leave the scene.
        string name = ScenePrivate.FindAgent(userId).AgentInfo.Name;
        Visitor visitor;
        if (Visitors.TryGetValue(name, out visitor))
        {
            visitor.VisitStarted = Stopwatch.GetTimestamp();
            visitor.Here = true;
        }
        else
        {
            visitor = new Visitor();
            visitor.TotalTime = TimeSpan.Zero;
            visitor.VisitStarted = Stopwatch.GetTimestamp();
            visitor.Here = true;
            Visitors[name] = visitor;
        }

        // Block until the agent leaves the scene
        WaitFor(ScenePrivate.User.Subscribe, User.RemoveUser, userId);

        // This should succeed unless the data has been reset.
        // Even then it _should_ succeed as we re-build it with anyone still in the region.
        if (Visitors.TryGetValue(name, out visitor))
        {
            visitor.TotalTime += visitor.ThisVisitSoFar;
            visitor.Here = false;
        }
    }

    private string getVisitorMessage()
    {
        string message = "There have been " + Visitors.Count + " visitors:\n";
        foreach (var visitor in Visitors)
        {
            message += "   " + visitor.Key + " visited for " + (visitor.Value.TotalTime + visitor.Value.ThisVisitSoFar).TotalMinutes + " minutes. [here now]\n";
        }
        return message;
    }

    private void OwnerCommand(ChatData data)
    {
        // Checking the message is actually the fastest thing we could do here. Discard anything that isn't the command we are looking for.
        if (data.Message != VisitorListCommand)
        {
            return;
        }

        AgentPrivate agent = ScenePrivate.FindAgent(data.SourceId);
        if (agent == null)
        {   // Possible race condition and they already logged off.
            return;
        }

        // If no OwnerName is set, let anyone get the visitor list, otherwise only if the owner name matches.
        if (PublicAccess
            || agent.AgentInfo.AvatarUuid == ScenePrivate.SceneInfo.AvatarUuid)
        {
            // Dialogs are much easier in a coroutine with WaitFor
            StartCoroutine(ShowStats, agent);
        }
    }

    private void ShowStats(AgentPrivate agent)
    {
        ModalDialog modalDialog = agent.Client.UI.ModalDialog;
        OperationCompleteEvent result = (OperationCompleteEvent)WaitFor(modalDialog.Show, getVisitorMessage(), "OK", "Reset");
        if (result.Success && modalDialog.Response == "Reset")
        {
            WaitFor(modalDialog.Show, "Are you sure you want to reset visitor counts?", "Yes!", "Cancel");
            if (modalDialog.Response == "Yes!")
            {
                // Make a new dictionary of everyone still here to replace the current tracking info.
                Dictionary<string, Visitor> stillHere = Visitors;
                Visitors = new Dictionary<string, Visitor>();

                foreach (var visitor in stillHere)
                {
                    Visitor v = new Visitor();
                    v.TotalTime = TimeSpan.Zero;
                    v.VisitStarted = Stopwatch.GetTimestamp();
                    v.Here = true;
                    Visitors[visitor.Key] = v;
                }

                WaitFor(modalDialog.Show, "Visitor times reset.", "", "Ok");
            }
        }
    }
}