/* This content is licensed under the terms of the Creative Commons Attribution 4.0 International License.
 * When using this content, you must:
 * •    Acknowledge that the content is from the Sansar Knowledge Base.
 * •    Include our copyright notice: "© 2017 Linden Research, Inc."
 * •    Indicate that the content is licensed under the Creative Commons Attribution-Share Alike 4.0 International License.
 * •    Include the URL for, or link to, the license summary at https://creativecommons.org/licenses/by-sa/4.0/deed.hi (and, if possible, to the complete license terms at https://creativecommons.org/licenses/by-sa/4.0/legalcode.
 * For example:
 * "This work uses content from the Sansar Knowledge Base. © 2017 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."
 */

using Sansar;
using Sansar.Script;
using Sansar.Simulation;

using System;

public class AutoBroadcastAll : SceneObjectScript
{
    public override void Init()
    {
        ScenePrivate.User.Subscribe(User.AddUser, OnAddUser);

        foreach (var visitor in ScenePrivate.GetAgents())
        {
            ScenePrivate.SetMegaphone(visitor, true);
        }
    }

    void OnAddUser(UserData ud)
    {
        AgentPrivate agent = ScenePrivate.FindAgent(ud.User);
        ScenePrivate.SetMegaphone(agent, true);
    }
}
