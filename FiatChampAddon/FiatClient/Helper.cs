using System.Diagnostics;
using System.Text;
using Amazon;
using Amazon.Runtime;
using AwsSignatureVersion4.Private;
using Flurl.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FiatChamp;

public static class Helper
{
  public static Dictionary<string, string> Compact(this JToken container, string key = "root", Dictionary<string, string>? result = null)
  {
    if (result == null)
    {
      result = new Dictionary<string, string>();
    }
  
    if (container is JValue value)
    {
      result.Add(key, value.Value?.ToString() ?? "null");
    }
    else if(container is JArray array)
    {
      for (int i = 0; i < array.Count(); i++)
      {
        var token = array[i];
        Compact(token, $"{key}_array_{i}", result);
      }
    }
    else if (container is JObject obj)
    {
      foreach (var kv in obj)
      {
        Compact(kv.Value, $"{key}_{kv.Key}", result);
      }
    }

    return result;
  }

  public static IFlurlRequest AwsSign(this IFlurlRequest request, ImmutableCredentials credentials,
    RegionEndpoint regionEndpoint, object? data = null)
  {
    request.BeforeCall(call =>
    {
      var json = data == null ? "" : JsonConvert.SerializeObject(data);
      call.HttpRequestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

      Signer.Sign(call.HttpRequestMessage,
        null, new List<KeyValuePair<string, IEnumerable<string>>>(),
        DateTime.Now, regionEndpoint.SystemName, "execute-api", credentials);
    });

    return request;
  }

  public static string Dump(this object? o)
  {
    try
    {
      var result = o;
      if (o is Task task)
      {
        task.Wait();
        result = (object)((dynamic)task).Result;
      }

      if (result is string str)
      {
        try
        {
          var json = JObject.Parse(str);
          return json.ToString(Formatting.Indented);
        }
        catch (Exception e)
        {
          return str;
        }
      }

      return JsonConvert.SerializeObject(result, Formatting.Indented);

    }
    catch (Exception)
    {
      return o?.GetType().ToString() ?? "null";
    }
  }
}