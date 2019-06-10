using YamlDotNet.RepresentationModel;

namespace Swarm.TaskRunner.Modules.Dockers {
  public class RunModule : Module<RunModuleDefinition> {
    public override RunModuleDefinition Parse(string version, YamlMappingNode node) {
      return new RunModuleDefinition(this, node);
    }

    public override void Execute(TaskContext context, RunModuleDefinition definition) {
    }
  }

  public class RunModuleDefinition : DockerModuleDefinition {
    public RunModuleDefinition(IModule module, YamlMappingNode node) : base(module, node) {
    }
  }
}