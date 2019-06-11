using Docker.DotNet;
using Docker.DotNet.Models;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using YamlDotNet.RepresentationModel;

namespace Swarm.TaskRunner.Modules.Dockers {
  public class PsModule : DockerModule<PsModuleDefinition> {
    public override PsModuleDefinition Parse(string version, YamlMappingNode node) {
      return new PsModuleDefinition(this, node);
    }

    public override void Execute(TaskContext context, PsModuleDefinition definition) {
      Contract.Requires(context != null);
      Contract.Requires(definition != null);

      var client = CreateClient(definition);

      var task = client.Containers.ListContainersAsync(new ContainersListParameters() {
        All = definition.All,
        Size = definition.Size,
        Limit = definition.Limit,
        Since = definition.Since,
        Before = definition.Before,
        Filters = definition.Filters,
      });

      task.Wait();

      var result = task.Result;

      var index = 0;
      context.EnvironmentVariables[$"DOCKER_CONTAINER_COUNT"] = result.Count.ToString();
      foreach (var container in result) {
        context.EnvironmentVariables[$"DOCKER_CONTAINER[{index}].ID"] = container.ID;
        context.EnvironmentVariables[$"DOCKER_CONTAINER[{index}].NAME"] = container.Names.FirstOrDefault();

        index++;
      }
    }
  }

  public class PsModuleDefinition : DockerModuleDefinition {
    public PsModuleDefinition() {
    }

    public PsModuleDefinition(IModule module, YamlMappingNode node) : base(module, node) {
      if (node.Children.ContainsKey("size")) {
        Size = bool.Parse((string)node.Children["size"]);
      }

      if (node.Children.ContainsKey("all")) {
        All = bool.Parse((string)node.Children["all"]);
      }

      if (node.Children.ContainsKey("since")) {
        Since = (string)node.Children["since"];
      }

      if (node.Children.ContainsKey("before")) {
        Before = (string)node.Children["before"];
      }

      if (node.Children.ContainsKey("limit")) {
        Limit = long.Parse((string)node.Children["limit"]);
      }

      if (node.Children.ContainsKey("filters")) {
        Filters = new Dictionary<string, IDictionary<string, bool>>();
        var filters = node.Children["filters"] as YamlSequenceNode;
        foreach (YamlScalarNode filter in filters) {
          var arr = filter.Value.Split("=");
          if (arr.Length == 2) {
            var prop = arr[0];
            var value = arr[1];

            if (!Filters.ContainsKey(prop)) {
              Filters[prop] = new Dictionary<string, bool>();
            }

            Filters[prop][value] = true;
          }
        }
      }
    }

    public bool? Size { get; set; }

    public bool? All { get; set; }

    public string Since { get; set; }

    public string Before { get; set; }

    public long? Limit { get; set; }

    public IDictionary<string, IDictionary<string, bool>> Filters { get; set; }
  }
}