using YamlDotNet.RepresentationModel;

namespace Swarm.TaskRunner.Modules {
  public class EchoModuleDefinition : ModuleDefinition {

    public EchoModuleDefinition(Module module, YamlMappingNode node) : base(module, node) {
    }

    public string Message { get; set; }
  }

  public class EchoModule : Module {

    public override ModuleDefinition Parse(string version, YamlMappingNode node) {
      return new EchoModuleDefinition(this, node) {
        Message = (string)node.Children["message"]
      };
    }

    public override void Execute(TaskContext context, ModuleDefinition definition) {
      var def = definition as EchoModuleDefinition;

      Logger.LogInfo(context.GetValue(def.Message));
    }
  }
}