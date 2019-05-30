using System.Collections.Generic;
using Swarm.TaskRunner.Definitions;
using YamlDotNet.RepresentationModel;

namespace Swarm.TaskRunner.Modules {
  public abstract class Module {
    public abstract ModuleDefinition Parse(string version, YamlMappingNode node);

    public abstract void Execute(TaskContext context, ModuleDefinition ModuleDefinition);
    public virtual void Abort() {}
  }
}