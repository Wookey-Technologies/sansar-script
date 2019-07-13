
using System;
using System.Collections.Generic;
using Sansar.Simulation;
using Sansar.Utility;
using EvoAv.Promises;
using EvoAv.Promises.Utility;

namespace EvoAv.Promises.Http {

  public struct Url {
    public string Protocol;
    public string Host;
    public string Path;
    public Dictionary<string, string> Query;
    public string Fragment;

    public string Username;
    public string Password;

    static List<Char> AllowedPlain = new List<Char>("0123456789abcdefghijklmnopqrstuvwxyz.-".ToCharArray());
    static List<string> Endcoded = new List<string> {"%25", "%3D", "%3F", "%26", "%23", "%40", "%20", "%3A", "%0A", "%09"};
    static List<string> Decoded = new List<string> {"%", "=", "?", "&", "#", "@", " ", ":", "\n", "\t"};
    public Url (string url) {
      Protocol = null;
      Host = null;
      Path = null;
      Query = null;
      Fragment = null;
      Username = null;
      Password = null;

      if (url == null) return;

      int i;
      string part;
      string next = url;

      i = next.IndexOf("://");
      if (i > -1) {
        part = next.Substring(0, i);
        if (Validate(AllowedPlain, part)) {
          Protocol = Decode(part);
          next = next.Substring(part.Length + 3);
        } else {
          throw new Exception("Bad protocol: \"" + part + "\"");
        }
      } else {
        throw new Exception("Url must have protocol like http/https: " + url);
      }

      if (Protocol != null) {
        i = next.IndexOf("/");
        if(i > -1) {
          part = next.Substring(0, i);
        } else {
          part = next;
        }
        i = part.IndexOf("@");
        if (i > -1) {
          string[] parts = part.Substring(0, i).Split(":".ToCharArray());
          if (parts.Length != 2) {
            throw new Exception("Bad auth params: \"" + part + "\"");
          }
          Username = Decode(parts[0]);
          Password = Decode(parts[1]);
          part = part.Substring(i + 1);
        }
        if (Validate(AllowedPlain, part)) {
          Host = Decode(part);
          next = next.Substring(i + 2 + part.Length);
        } else {
          throw new Exception("Bad host: \"" + part + "\"");
        }
      }

      i = next.IndexOf("#");
      if(i > -1) {
        Fragment = Decode(next.Substring(i + 1));
        next = next.Substring(0, i);
      }
      
      i = next.IndexOf("?");
      if(i > -1) {
        Path = next.Substring(0, i);
        Query = ParseQueryString(next.Substring(i + 1));
      } else {
        Path = Decode(next);
      }
    }

    public static string Encode(string value) {
      for (int i = 0; i < Endcoded.Count; i++) {
        value = value.Replace(Decoded[i], Endcoded[i]);
      }
      return value;
    }
    public static string Decode(string value) {
      string res = "";
      string next = value;
      while(next != "") {
        int i = next.IndexOf("%");
        if (i > -1) {
          res += next.Substring(0, i);
          try {
            byte[] b = HexToBytes(next.Substring(i+1, 2));
            res += new string(System.Text.ASCIIEncoding.ASCII.GetChars(b));
			      next = next.Substring(i+3);
          } catch (Exception) {
            if (next.Length > 3) {
              res += next.Substring(0, 3);
				      next = next.Substring(3);
            } else {
              res += next;
				      next = "";
            }
          }
        } else {
          res += next;
          next = "";
        }
      }
      return res;
    }
    static byte[] HexToBytes(string str)
    {
        if (str == null || str.Length == 0 || str.Length % 2 != 0)
            return new byte[0];

        byte[] buffer = new byte[str.Length / 2];
        char c;
        for (int bx = 0, sx = 0; bx < buffer.Length; ++bx, ++sx)
        {
            // Convert first half of byte
            c = str[sx];
            buffer[bx] = (byte)((c > '9' ? (c > 'Z' ? (c - 'a' + 10) : (c - 'A' + 10)) : (c - '0')) << 4);

            // Convert second half of byte
            c = str[++sx];
            buffer[bx] |= (byte)(c > '9' ? (c > 'Z' ? (c - 'a' + 10) : (c - 'A' + 10)) : (c - '0'));
        }

        return buffer;
    }

    public override string ToString() {
      return Get(Protocol, true, "://") + Get(GetAuth(Username, Password), false, "@") + Get(Host, true, "/") + Get(Path, true) + Get(QueryToString(Query), false, "", "?") + Get(Fragment, true, "", "#");
    }
	  
    static string GetAuth(string u, string p) {
      return u == null ? null : (Encode(u) + ":" + (p == null ? "" : Encode(p)));
    }

    public static Dictionary<string, string> ParseQueryString(string s) {
      if (s == null) return null;
      Dictionary<string,string> query = new Dictionary<string,string>();
      foreach (string v in s.Split("&".ToCharArray())) {
        string[] parts = v.Split("=".ToCharArray());
        if (parts.Length > 2) {
          throw new Exception("Bad query params: \"" + v + "\"");
        } else if (parts.Length > 0 && parts[0] != "") { 
          if (parts.Length == 1) {
            query[Decode(parts[0])] = null;
          } else if (parts.Length == 2) {
            query[Decode(parts[0])] = Decode(parts[1]);
          }
        }
      }
      return query;
    }

    public static string QueryToString(Dictionary<string, string> query) {
      if (query == null) return null;
      string s = "";
      foreach (KeyValuePair<string, string> kv in query) {
        s += Encode(kv.Key) + (kv.Value == null ? "" :  "=" + (Encode(kv.Value))) + "&";
      }
      return s == "" ? "" : s.Substring(0, s.Length-1);
    }

    static string Get(string s, bool encode = true, string leading = "", string preceding = "") {
      return (s == null ? "" : preceding + (encode ? Encode(s) : s) + leading);
    }

    static bool Validate(List<Char> allowed, string value) {
      foreach(Char ch in value) {
        if (!allowed.Contains(ch)) {
          return false;
        }
      }
      return true;
    }
  }

  public class HttpClient {
    public Sansar.Simulation.HttpClient Client {get; protected set;}
    DeThrottler<HttpResponse> DeThrottler = new DeThrottler<HttpResponse>(5, 10);

    public HttpClient(Sansar.Simulation.HttpClient client) {
      Client = client;
    }

    public IPromise<HttpResponse> Request(Url url, HttpRequestOptions options) {
      return DeThrottler.Enqueue(() => {
        Promise<HttpResponse> promise = new Promise<HttpResponse>();
        Client.Request(url.ToString(), options, (Sansar.Simulation.HttpClient.RequestData data) => {
          if (data.Success) {
            promise.Resolve(data.Response);
          } else {
            promise.Reject(data.Exception == null ? new Exception(data.Message) : data.Exception);
          }
        });
        return promise;
      });
    }

    public IPromise<string> Get(Url url, Dictionary<string, string> headers = null) {
      Promise<string> promise = new Promise<string>();
      HttpRequestOptions options = new HttpRequestOptions();
      options.Headers = headers;
      options.Method = HttpRequestMethod.GET;
      options.DisableRedirect = false;
      Request(url, options).Then((HttpResponse res) => {
        if(res.Status >= 200  && res.Status < 300) {
          promise.Resolve(res.Body);
        } else {
          promise.Reject(new Exception(res.Status.ToString() + ": " + res.Body));
        }
      }, promise.Reject);
      return promise;
    }

    public IPromise<T> GetJson<T>(Url url) {
      Promise<T> promise = new Promise<T>();
      Get(url).Then((string body) => {
        JsonSerializer.Deserialize(body, (JsonSerializationData<T> json) => {
          if (json.Success) {
            promise.Resolve(json.Object);
          } else {
            promise.Reject(json.Exception);
          }
        });
      }, promise.Reject);
      return promise;
    }

    public IPromise<string> Post(Url url, string body = null, Dictionary<string, string> headers = null) {
      Promise<string> promise = new Promise<string>();
      HttpRequestOptions options = new HttpRequestOptions();
      options.Body = body;
      options.Headers = headers;
      options.Method = HttpRequestMethod.POST;
      options.DisableRedirect = false;
      Request(url, options).Then((HttpResponse res) => {
        if(res.Status >= 200  && res.Status < 300) {
          promise.Resolve(res.Body);
        } else {
          promise.Reject(new Exception(res.Body));
        }
      }, promise.Reject);
      return promise;
    }

    public IPromise<T> PostJson<T, B>(Url url, B jsonBody, Dictionary<string, string> headers = null) {
      Promise<T> promise = new Promise<T>();
      if (jsonBody == null) {
        return PostJson<T>(url, headers);
      } else {
        JsonSerializer.Serialize(jsonBody, (JsonSerializationData<B> json) => {
            if (json.Success) {
              Post(url, json.JsonString, headers).Then((string body) => {
                JsonSerializer.Deserialize(body, (JsonSerializationData<T> jsonRes) => {
                  if (json.Success) {
                    promise.Resolve(jsonRes.Object);
                  } else {
                    promise.Reject(jsonRes.Exception);
                  }
                });
              }, promise.Reject);
            } else {
              promise.Reject(json.Exception);
            }
        });
      }
      return promise;
    }
    
    public IPromise<T> PostJson<T>(Url url, Dictionary<string, string> headers = null) {
      Promise<T> promise = new Promise<T>();
      Post(url, null, headers).Then((string body) => {
        JsonSerializer.Deserialize(body, (JsonSerializationData<T> json) => {
          if (json.Success) {
            promise.Resolve(json.Object);
          } else {
            promise.Reject(json.Exception);
          }
        });
      }, promise.Reject);
      return promise;
    }

  }

}
