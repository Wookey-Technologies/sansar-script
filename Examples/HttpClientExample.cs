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


public class HttpClientExample : SceneObjectScript
{
    public override void Init()
    {
        HttpRequestOptions options = new HttpRequestOptions();

        options.Parameters = new Dictionary<string, string>()
        {
            { "num" , "1" },
            { "min" , "100000000" },
            { "max" , "999999999" },
            { "base" , "10" },
            { "format" , "plain" },
            { "col" , "1" },
        };

        ScenePrivate.User.Subscribe(User.AddUser, userData=>NewUser(userData, options));
    }

    void NewUser(UserData userData, HttpRequestOptions options)
    {
        AgentPrivate agent = ScenePrivate.FindAgent(userData.User);

        if (agent != null)
        {
            var result = WaitFor(ScenePrivate.HttpClient.Request, "https://www.random.org/integers/", options) as HttpClient.RequestData;
            if (result.Success)
            {
                agent.SendChat("Your lucky number is " + result.Response.Body);
            }
        }
    }
}