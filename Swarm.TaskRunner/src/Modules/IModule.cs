using YamlDotNet.RepresentationModel;

namespace Swarm.TaskRunner.Modules {
  public interface IModule {

    ModuleDefinition Parse(string version, YamlMappingNode node);

    void Execute(TaskContext context, ModuleDefinition definition);

    void Abort();
  }
}