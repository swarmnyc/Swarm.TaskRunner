using System;
using System.Collections.Generic;
using Swarm.TaskRunner.Modules;
using YamlDotNet.RepresentationModel;

namespace Swarm.TaskRunner.Tests {
  public class TestModuleDefinition : ModuleDefinition {
    public string Arg { get; set; }

    public TestModuleDefinition(IModule module, YamlMappingNode node) : base(module, node) {
    }
  }

  public class TestModule : Module<TestModuleDefinition> {
    public int CallCount { get; set; }

    public List<string> CallArgs { get; } = new List<string>();

    public override TestModuleDefinition Parse(string version, YamlMappingNode node) {
      var def = new TestModuleDefinition(this, node);

      if (node.Children.ContainsKey("arg")) {
        def.Arg = (string)node.Children["arg"];
      }

      return def;
    }

    public override void Execute(TaskContext context, TestModuleDefinition definition) {
      CallCount++;

      if (definition.Arg != null) {
        CallArgs.Add(definition.Arg);
      }
    }
  }
}
