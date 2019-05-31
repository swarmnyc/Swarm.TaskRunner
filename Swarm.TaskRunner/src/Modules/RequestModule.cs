using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace Swarm.TaskRunner.Modules {
  public class RequestModuleDefinition : ModuleDefinition {
    public RequestModuleDefinition(Module module, YamlMappingNode node) : base(module, node) {
    }

    public string Url { get; set; }
    public string Method { get; set; }
    public string Body { get; set; }
    public IDictionary<string, string> Headers { get; } = new Dictionary<string, string>();
  }

  public class RequestModule : Module {
    public override ModuleDefinition Parse(string version, YamlMappingNode node) {
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

      return def;
    }

    public override void Execute(TaskContext context, ModuleDefinition definition) {
      try {
        ExecuteAsync(context, definition).Wait();
      } catch (Exception ex) {
        throw ex.InnerException == null ? ex : ex.InnerException;
      }
    }

    public async Task ExecuteAsync(TaskContext context, ModuleDefinition definition) {
      var def = definition as RequestModuleDefinition;
      var client = new HttpClient();
      var request = new HttpRequestMessage();
      request.RequestUri = new Uri(context.GetValue(def.Url));
      request.Method = new HttpMethod(def.Method);

      foreach (var (key, value) in def.Headers) {
        request.Headers.Add(key, context.GetValue(value));
      }

      Logger.LogInfo($"Request {request.Method} {request.RequestUri}");

      if (def.Body != null) {
        request.Content = new StringContent(context.GetValue(def.Body));
      }

      var response = await client.SendAsync(request);

      if (response.IsSuccessStatusCode) {
        if (Logger.IsVerbose) {
          var body = await response.Content.ReadAsStringAsync();
          Logger.LogInfo(body);
        }
      } else {
        var body = await response.Content.ReadAsStringAsync();
        throw new Exception($"Request Failed by {response.StatusCode} " + body);
      }
    }
  }
}