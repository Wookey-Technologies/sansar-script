/* This content is licensed under the terms of the Creative Commons Attribution 4.0 International License.
 * When using this content, you must:
 * •    Acknowledge that the content is from the Sansar Knowledge Base.
 * •    Include our copyright notice: "© 2018 Linden Research, Inc."
 * •    Indicate that the content is licensed under the Creative Commons Attribution-Share Alike 4.0 International License.
 * •    Include the URL for, or link to, the license summary at https://creativecommons.org/licenses/by-sa/4.0/deed.hi (and, if possible, to the complete license terms at https://creativecommons.org/licenses/by-sa/4.0/legalcode.
 * For example:
 * "This work uses content from the Sansar Knowledge Base. © 2018 Linden Research, Inc. Licensed under the Creative Commons Attribution 4.0 International License (license summary available at https://creativecommons.org/licenses/by/4.0/ and complete license terms available at https://creativecommons.org/licenses/by/4.0/legalcode)."
 */

/* Register for the event that occurs when chat messages happen.
 * Listens for a "/gravity" command that will either print the scene's current gravity or attempts
 * to set the gravity to the specified value, such as "/gravity 9.81" for normal earth gravity.
 */
using Sansar.Simulation;
using Sansar.Script;
using System;

// To get full access to the Object API a script must extend from ObjectScript
public class GravityExample : SceneObjectScript
{
    public override void Init()
    {
        // Set up a callback to listen to the default chat channel for all events
        ScenePrivate.Chat.Subscribe(Chat.DefaultChannel, OnChat, true);
    }

    private void OnChat(ChatData data)
    {
        // Parse the chat message into an array of strings and look for a "/gravity" command
        var cmds = data.Message.Split(new Char[] { ' ' });
        if (cmds[0] == "/gravity")
        {
            if (cmds.Length > 1)
            {
                // Attempt to interpret the text after the command as a number
                float value;
                if (float.TryParse(cmds[1], out value))
                {
                    ScenePrivate.Chat.MessageAllUsers("Attempting to set gravity to: " + value);
                    ScenePrivate.SetGravity(value);  // Assign the scene's gravity to the number
                }
                // Otherwise reset the scene to default gravity if specified
                else if (cmds[1] == "default")
                {
                    ScenePrivate.Chat.MessageAllUsers("Setting gravity back to default.");
                    ScenePrivate.SetGravity(ScenePrivate.DefaultGravity);
                }
            }
            // If no additional parameter was specified, print out the scene's current gravity
            else
            {
                ScenePrivate.Chat.MessageAllUsers("Gravity is set to: " + ScenePrivate.GetGravity());
            }
        }
    }
}
