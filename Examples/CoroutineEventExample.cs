/* This content is licensed under the terms of the Creative Commons Attribution 4.0 International License.
 * When using this content, you must:
 * •    Acknowledge that the content is from the Sansar Knowledge Base.
 * •    Include our copyright notice: "© 2017 Linden Research, Inc."
 * •    Indicate that the content is licensed under the Creative Commons Attribution-Share Alike 4.0 International License.
 * •    Include the URL for, or link to, the license summary at https://creativecommons.org/licenses/by-sa/4.0/deed.hi (and, if possible, to the complete license terms at https://creativecommons.org/licenses/by-sa/4.0/legalcode.
 * For example:
 * "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."
 */

/* Use a coroutine as an event callback to easily track visitors to a scene.
 * Every visitor triggers an AddUser event which will start a coroutine for that visitor.
 */
using System;
using Sansar.Script;
using Sansar.Simulation;
using System.Diagnostics;


// This example shows how to use coroutines and events to track agents entering and leaving the scene
public class CoroutineEventExample : SceneObjectScript
{
    public override void Init()
    {
        // Subscribe to Add User events
        // Events can be handled by anonymous methods
        ScenePrivate.User.Subscribe(User.AddUser, (UserData data) => StartCoroutine(TrackUser, data.User), true);
    }


    // There will be one instance of this coroutine per active user in the scene
    private void TrackUser(SessionId userId)
    {
        // Track joined time
        long joined = Stopwatch.GetTimestamp();

        // Lookup the name of the agent. This is looked up now since the agent cannot be retrieved after they
        // leave the scene.
        string name = ScenePrivate.FindAgent(userId).AgentInfo.Name;

        // Block until the agent leaves the scene
        WaitFor(ScenePrivate.User.Subscribe, User.RemoveUser, userId);

        // Calculate elapsed time and report.
        TimeSpan elapsed = TimeSpan.FromTicks(Stopwatch.GetTimestamp()-joined);
        ScenePrivate.Chat.MessageAllUsers(string.Format("{0} was present for {0} seconds", name, elapsed.TotalSeconds));
    }
}
