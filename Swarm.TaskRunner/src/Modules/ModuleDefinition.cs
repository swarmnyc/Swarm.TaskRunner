using System.Diagnostics.Contracts;
using YamlDotNet.RepresentationModel;

namespace Swarm.TaskRunner.Modules {
  public abstract class ModuleDefinition {
    public ModuleDefinition() {
    }

    protected ModuleDefinition(IModule module, YamlMappingNode node) {
      Contract.Requires(module != null);
      Contract.Requires(node != null);

      Module = module;
      if (node.Children.ContainsKey("label")) {
        Label = (string)node.Children["label"];
      }
    }

    public IModule Module { get; set; }

    public string Label { get; set; }
  }
}