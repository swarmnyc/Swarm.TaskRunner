using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace Swarm.TaskRunner.Modules {
  public class RequestModule : Module<RequestModuleDefinition> {
    private static readonly Regex TextRegex = new Regex("text|json|xml");

    public override RequestModuleDefinition Parse(string version, YamlMappingNode node) {
      return new RequestModuleDefinition(this, node);
    }

    public override void Execute(TaskContext context, RequestModuleDefinition definition) {
      try {
        ExecuteAsync(context, definition).Wait();
      } catch (Exception ex) {
        throw ex.InnerException ?? ex;
      }
    }

    protected virtual HttpClient CreateHttpClient() {
      return new HttpClient();
    }

    protected async Task ExecuteAsync(TaskContext context, RequestModuleDefinition definition) {
      Contract.Requires(context != null);
      Contract.Requires(definition != null);

      using (var client = CreateHttpClient())
      using (var request = new HttpRequestMessage()) {
        request.RequestUri = new Uri(context.GetValue(definition.Url));
        request.Method = new HttpMethod(definition.Method);

        foreach (var (key, value) in definition.Headers) {
          request.Headers.Add(key, context.GetValue(value));
        }

        Logger.LogInfo($"Request {request.Method} {request.RequestUri}");

        if (definition.Body != null) {
          request.Content = new StringContent(context.GetValue(definition.Body));
        }

        using (var response = await client.SendAsync(request)) {
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
  }

  public class RequestModuleDefinition : ModuleDefinition {
    public RequestModuleDefinition() {
    }

    public RequestModuleDefinition(IModule module, YamlMappingNode node) : base(module, node) {
      Url = (string)node.Children["url"];
      Method = (string)node.Children["method"];

      if (node.Children.ContainsKey("headers")) {
        var headers = node.Children["headers"] as YamlMappingNode;
        foreach (var header in headers.Children) {
          Headers[(string)header.Key] = (string)header.Value;
        }
      }

      if (node.Children.ContainsKey("body")) {
        Body = (string)node.Children["body"];
      }

      if (node.Children.ContainsKey("outputToEnv")) {
        OutputToEnv = (string)node.Children["outputToEnv"];
      }
    }

    public string Url { get; set; }

    public string Method { get; set; }

    public string Body { get; set; }

    public string OutputToFile { get; set; }

    public string OutputToEnv { get; set; }

    public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();
  }
}