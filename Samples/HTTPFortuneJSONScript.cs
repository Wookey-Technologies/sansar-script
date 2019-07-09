// © 2019 Linden Research, Inc.

// Special thanks to NyushaZoryAna for making me aware of the rest service, the idea for this sample and help making this functional.

using Sansar.Script;
using Sansar.Simulation;
using Sansar.Utility;
using System;
using System.Collections.Generic;

public class HTTPFortuneJSONScript : SceneObjectScript
{
    private readonly string url = "http://fortunecookieapi.herokuapp.com/v1/cookie";

    // The above url will return a JSON blob like this:
    //
    //[
    // {
    //  "fortune":{"message":"Too many cooks spoil the broth.","id":"5403c81dc2fea4020029abe0"},
    //  "lesson": {"english":"100,000","chinese":"十万","pronunciation":"shí-wàn","id":"5404c5404cad2502004dee54"},
    //  "lotto":  {"id":"000400040023003100080005","numbers":[4,4,23,31,8,5]}
    // }
    //]
    //
    // NOTE: This is a little unusual with the outer array block which makes it slightly tricky to parse.

    // Define public classes with public member variables to mirror the JSON data layout
    public class FortuneData { public string message; public string id; }
    public class LessonData { public string english; public string chinese; public string pronunciation; public string id; }
    public class LottoData { public string id; public List<int> numbers; }
    public class CookieData { public FortuneData fortune; public LessonData lesson; public LottoData lotto; }

    public override void Init()
    {
        if (!ScenePrivate.HttpClient.IsValid)
        {
            // In practice this should never happen but just in case...
            Log.Write(LogLevel.Error, "HTTP client invalid!  Fortune cookie retrieval disabled.");
            return;
        }

        // Add a chat listener to retrieve visit count when "fortune" or "/fortune" is written in chat
        ScenePrivate.Chat.Subscribe(Chat.DefaultChannel, (ChatData data) =>
        {
            if ((data.Message == "fortune") || (data.Message == "/fortune"))
                GetFortune();
        });
    }

    void GetFortune()
    {
        // Set up an HTTP GET request
        HttpRequestOptions options = new HttpRequestOptions();
        options.Method = HttpRequestMethod.GET;
        options.Parameters = new Dictionary<string, string>(){ { "limit", "1" } };  // unnecessary since the default is 1 but here as reference

        // Do the HTTP request
        ScenePrivate.HttpClient.Request(url, options, (HttpClient.RequestData data) =>
        {
            // Check for success
            if (data.Success && data.Response.Status == 200)
            {
                // Display the entire body of unparsed JSON in the debug log for reference
                Log.Write($"Unparsed JSON:\n{data.Response.Body}\n");

                // Parse the data into an array of CookieData
                var cookiesDefinition = WaitFor(JsonSerializer.Deserialize<CookieData[]>, data.Response.Body) as JsonSerializationData<CookieData[]>;
                CookieData[] cookiesData = cookiesDefinition.Object;

                // Print the relevant fields to chat

                var fortune = cookiesData[0].fortune;
                ScenePrivate.Chat.MessageAllUsers($"Your fortune: {fortune.message}");

                var lesson = cookiesData[0].lesson;
                ScenePrivate.Chat.MessageAllUsers($"Chinese lesson: {lesson.chinese} ({lesson.pronunciation}) means {lesson.english} in English");

                var lotto = cookiesData[0].lotto;
                ScenePrivate.Chat.MessageAllUsers($"Lotto numbers: {string.Join(", ", lotto.numbers)}");
            }
            else
            {
                ScenePrivate.Chat.MessageAllUsers("HTTPFortuneJSONScript GetFortune: Error");
            }
        });
    }
}
