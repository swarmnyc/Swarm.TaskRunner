using System;
using Swarm.TaskRunner.Modules;
using YamlDotNet.RepresentationModel;

namespace Swarm.TaskRunner.Tests {
  public class TestModuleDefinition : ModuleDefinition {
    public TestModuleDefinition(Module module, YamlMappingNode node) : base(module, node) {
    }
  }

  public class TestModule : Module {
    public int CallCount { get; set; }
    public override ModuleDefinition Parse(string version, YamlMappingNode node) {
      return new TestModuleDefinition(this, node);
    }

    public override void Execute(TaskContext context, ModuleDefinition ModuleDefinition) {
      CallCount++;
    }
  }
}
