# Sansar Api, with Promises 😉

The promise pattern is a an async pattern that simplifies, and adds abilities to, async programming. The nature of async progamming can be summed up as "I call a function now, and can wait for the response, without having to stall the currently running code. 

The word `Promise` refers to the "promise" of a response, while allowing to break it when there is an error. When the promise is fullfilled, this is called `Resolved`, and when there is an error, it is called `Rejected`. These two possible outcomes dismiss the need for try/catch blocks, because when an exception is thrown during the call, the promise simply rejects, and the code can happily continue running.

When a promise is made, the result can be subscribed to with `Then`, or `Catch` for listening only to rejects.

By having a promise object as a return value to functions, it is possible to collect many of them and then wait for all of them at once, without havinge to break up your code into different "done" callbacks, without freezing the code, such as when using `WaitFor`, and generally allows for much cleaner and more readable code.


# Http Wrapper

Included next to this doc, is a wrapper library to Sansar's `HttpClient` and `JsonSerializer`, that also provides a `Url` struct, a `DeThrottler`, and uses the promise api.

Example usage:

```csharp
using Sansar.Simulation;
using Sansar.Script;
using EvoAv.Promises.Http;
using System;

namespace EvoExamples 
{
  class HttpExample : SceneObjectScript 
  {

    EvoAv.Promises.Http.HttpClient Client;

    Url ExperienceSearchUrl = new Url("https://atlas.sansar.com/proxies/web/atlas-api/v3/experiences?perPage=1&q=2077");

    public override void Init() 
    {
      Client = new EvoAv.Promises.Http.HttpClient(ScenePrivate.HttpClient);

      Client.GetJson<SansarWebApi<Experience>>(ExperienceSearchUrl).Then(Response, Reject);
      Log.Write("This log appears first because the http call is async and does not freeze the code.");
    }

    void Response(SansarWebApi<Experience> res) 
    {
      if (res.data.Length == 0) throw new Exception("Not found");
      Log.Write("Response: " + res.data[0].name + ", by: " + res.data[0].personaName);
    }

    void Reject(Exception e) 
    {
      Log.Write(LogLevel.Error, e.Message);
    }
  }

  class Experience 
  {
    public string id;
    public string description;
    public string name;
    public string personaName;
    public string uri;
  }
  
  public class SansarWebApi<T> {
    public class Meta {
      public int page;
      public int pages;
      public int perPage;
      public int total;
    }
    public T[] data;
    public Meta meta;
  }
}
```