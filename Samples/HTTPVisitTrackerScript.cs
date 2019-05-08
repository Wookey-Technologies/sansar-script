using Sansar.Script;
using Sansar.Simulation;
using Sansar.Utility;
using System;
using System.Collections.Generic;

public class HTTPVisitTrackerScript : SceneObjectScript
{
    [DefaultValue("https://api.my.com/api/v1/sansar_visitor_tracking")]
    public readonly string Endpoint;

    public override void Init()
    {
        if (!ScenePrivate.HttpClient.IsValid)
        {
            // In practice this should never happen but just in case...
            Log.Write(LogLevel.Error, "HTTP client invalid!  Visitor tracking disabled.");
            return;
        }

        // Track a visit when a user joins the scene
        ScenePrivate.User.Subscribe(User.AddUser, (UserData ud) => { TrackVisit(); });

        // Add a chat listener to retrieve visit count when "/visits" is written in chat
        ScenePrivate.Chat.Subscribe(Chat.DefaultChannel, (ChatData data) =>
        {
            if (data.Message == "/visits")
                GetVisits();
        });
    }

    void TrackVisit()
    {
        HttpRequestOptions options = new HttpRequestOptions();
        options.Method = HttpRequestMethod.POST;

        options.Parameters = new Dictionary<string, string>()
        {
            { "sansar_uri" , ScenePrivate.SceneInfo.SansarUri },
        };

        ScenePrivate.HttpClient.Request(Endpoint, options, (HttpClient.RequestData data) =>
        {
            if (!data.Success || data.Response.Status != 200)
            {
                ScenePrivate.Chat.MessageAllUsers("VisitTrackerScript TrackVisit: Error");
            }
        });
    }

    void GetVisits()
    {
        HttpRequestOptions options = new HttpRequestOptions();
        options.Method = HttpRequestMethod.GET;

        options.Parameters = new Dictionary<string, string>()
        {
            { "sansar_uri" , ScenePrivate.SceneInfo.SansarUri },
        };

        ScenePrivate.HttpClient.Request(Endpoint, options, (HttpClient.RequestData data) =>
        {
            if (data.Success && data.Response.Status == 200)
            {
                Dictionary<string, int> jsonData = ((JsonSerializationData<Dictionary<string, int>>)(WaitFor(JsonSerializer.Deserialize<Dictionary<string, int>>, data.Response.Body))).Object;

                // print returned "visits" count to chat
                ScenePrivate.Chat.MessageAllUsers("Total visits: " + jsonData["visits"]);
            }
            else
            {
                ScenePrivate.Chat.MessageAllUsers("VisitTrackerScript GetVisits: Error");
            }
        });
    }
}
