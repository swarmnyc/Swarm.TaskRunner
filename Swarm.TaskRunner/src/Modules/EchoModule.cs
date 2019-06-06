using YamlDotNet.RepresentationModel;

namespace Swarm.TaskRunner.Modules {
  public class EchoModule : Module<EchoModuleDefinition> {
    public override EchoModuleDefinition Parse(string version, YamlMappingNode node) {
      return new EchoModuleDefinition(this, node) {
        Message = (string)node.Children["message"]
      };
    }

    public override void Execute(TaskContext context, EchoModuleDefinition definition) {
      Logger.LogInfo(context.GetValue(definition.Message));
    }
  }

  public class EchoModuleDefinition : ModuleDefinition {
    public EchoModuleDefinition(IModule module, YamlMappingNode node) : base(module, node) {
    }

    public string Message { get; set; }
  }
}