using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;
using System.Text.RegularExpressions;

namespace Swarm.TaskRunner.Modules {
  public class RequestModuleDefinition : ModuleDefinition {
    public RequestModuleDefinition(IModule module) : base(module) {
    }

    public RequestModuleDefinition(IModule module, YamlMappingNode node) : base(module, node) {
    }

    public string Url { get; set; }
    public string Method { get; set; }
    public string Body { get; set; }
    public string OutputToFile { get; set; }
    public string OutputToEnv { get; set; }
    public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();
  }

  public class RequestModule : Module<RequestModuleDefinition> {
    private static readonly Regex TextRegex = new Regex("text|json|xml");
    public override RequestModuleDefinition Parse(string version, YamlMappingNode node) {
      var def = new RequestModuleDefinition(this, node) {
        Url = (string)node.Children["url"],
        Method = (string)node.Children["method"]
      };

      if (node.Children.ContainsKey("headers")) {
        var headers = node.Children["headers"] as YamlMappingNode;
        foreach (var header in headers.Children) {
          def.Headers[(string)header.Key] = (string)header.Value;
        }
      }

      if (node.Children.ContainsKey("body")) {
        def.Body = (string)node.Children["body"];
      }

      if (node.Children.ContainsKey("outputToEnv")) {
        def.OutputToEnv = (string)node.Children["outputToEnv"];
      }

      return def;
    }

    public override void Execute(TaskContext context, RequestModuleDefinition definition) {
      try {
        ExecuteAsync(context, definition).Wait();
      } catch (Exception ex) {
        throw ex.InnerException ?? ex;
      }
    }

    public static async Task ExecuteAsync(TaskContext context, RequestModuleDefinition definition) {
      var client = new HttpClient();
      var request = new HttpRequestMessage {
        RequestUri = new Uri(context.GetValue(definition.Url)),
        Method = new HttpMethod(definition.Method)
      };

      foreach (var (key, value) in definition.Headers) {
        request.Headers.Add(key, context.GetValue(value));
      }

      Logger.LogInfo($"Request {request.Method} {request.RequestUri}");

      if (definition.Body != null) {
        request.Content = new StringContent(context.GetValue(definition.Body));
      }

      var response = await client.SendAsync(request);

      string contentType = response.Content.Headers.ContentType.MediaType;

      if (response.IsSuccessStatusCode) {
        if (Logger.IsVerbose || definition.OutputToEnv != null || definition.OutputToFile != null) {
          if (TextRegex.IsMatch(contentType)) {
            // text content
            var content = await response.Content.ReadAsStringAsync();

            if (Logger.IsVerbose) {
              Logger.LogInfo(content);
            }

            if (definition.OutputToEnv != null) {
              context.EnvironmentVariables[definition.OutputToEnv] = content;
            }

            if (definition.OutputToFile != null) {
              File.WriteAllText(definition.OutputToFile, content);
            }
          } else {
            // other context
            if (definition.OutputToFile != null) {
              var content = await response.Content.ReadAsByteArrayAsync();
              File.WriteAllBytes(definition.OutputToFile, content);
            }
          }
        }
      } else {
        if (TextRegex.IsMatch(contentType)) {
          var content = await response.Content.ReadAsStringAsync();
          throw new Exception($"Request Failed by {response.StatusCode} " + content);
        } else {
          throw new Exception($"Request Failed by {response.StatusCode}");
        }
      }
    }
  }
}