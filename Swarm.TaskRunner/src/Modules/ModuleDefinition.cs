using YamlDotNet.RepresentationModel;

namespace Swarm.TaskRunner.Modules {
  public abstract class ModuleDefinition {
    public ModuleDefinition(IModule module) {
      Module = module;
    }

    protected ModuleDefinition(IModule module, YamlMappingNode node) {
      Module = module;
      if (node.Children.ContainsKey("label")) {
        Label = (string)node.Children["label"];
      }
    }

    public IModule Module { get; }
    public string Label { get; set; }
  }
}