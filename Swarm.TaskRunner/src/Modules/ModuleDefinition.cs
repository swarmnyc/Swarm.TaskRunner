using YamlDotNet.RepresentationModel;

namespace Swarm.TaskRunner.Modules {
  public abstract class ModuleDefinition {
    protected ModuleDefinition(Module module, YamlMappingNode node) {
      Module = module;
      if (node.Children.ContainsKey("label")) {
        Label = (string)node.Children["label"];
      }
    }

    public Module Module { get; }
    public string Label { get; set; }
  }
}