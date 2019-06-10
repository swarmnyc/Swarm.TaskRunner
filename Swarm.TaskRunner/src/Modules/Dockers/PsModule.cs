using Docker.DotNet;
using Docker.DotNet.Models;
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
        Limit = 10,
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
    }
  }
}