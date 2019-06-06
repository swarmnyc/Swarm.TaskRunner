using YamlDotNet.RepresentationModel;

namespace Swarm.TaskRunner.Modules {
  public abstract class Module<T> : IModule where T : ModuleDefinition {
    public abstract T Parse(string version, YamlMappingNode node);

    public abstract void Execute(TaskContext context, T definition);

    public virtual void Abort() {
    }

    ModuleDefinition IModule.Parse(string version, YamlMappingNode node) {
      return Parse(version, node);
    }

    void IModule.Execute(TaskContext context, ModuleDefinition definition) {
      Execute(context, (T)definition);
    }
  }
}