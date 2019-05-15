using Sansar.Simulation;
using Sansar.Script;
using EvoAv.Promises.Http;
using System;

namespace EvoExamples 
{
  class PromiseHttpExample : SceneObjectScript 
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
}