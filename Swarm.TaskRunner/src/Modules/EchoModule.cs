using System.Diagnostics.Contracts;
using YamlDotNet.RepresentationModel;

namespace Swarm.TaskRunner.Modules {
  public class EchoModule : Module<EchoModuleDefinition> {
    public override EchoModuleDefinition Parse(string version, YamlMappingNode node) {
      return new EchoModuleDefinition(this, node);
    }

    public override void Execute(TaskContext context, EchoModuleDefinition definition) {
      Contract.Requires(context != null);
      Contract.Requires(definition != null);

      Logger.LogInfo(context.GetValue(definition.Message));
    }
  }

  public class EchoModuleDefinition : ModuleDefinition {
    public EchoModuleDefinition(IModule module, YamlMappingNode node) : base(module, node) {
      Contract.Requires(module != null);
      Contract.Requires(node != null);

      Message = (string)node.Children["message"];
    }

    public string Message { get; set; }
  }
}