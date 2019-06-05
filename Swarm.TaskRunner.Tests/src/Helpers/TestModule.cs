using System;
using System.Collections.Generic;
using Swarm.TaskRunner.Modules;
using YamlDotNet.RepresentationModel;

namespace Swarm.TaskRunner.Tests {
  public class TestModuleDefinition : ModuleDefinition {
    public string Arg { get; set; }

    public TestModuleDefinition(Module module, YamlMappingNode node) : base(module, node) {
    }
  }

  public class TestModule : Module {
    public int CallCount { get; set; }

    public List<string> CallArgs { get; } = new List<string>();

    public override ModuleDefinition Parse(string version, YamlMappingNode node) {
      var def = new TestModuleDefinition(this, node);

      if (node.Children.ContainsKey("arg")) {
        def.Arg = (string)node.Children["arg"];
      }

      return def;
    }

    public override void Execute(TaskContext context, ModuleDefinition definition) {
      var def = definition as TestModuleDefinition;
      CallCount++;

      if (def.Arg != null) {
        CallArgs.Add(def.Arg);
      }
    }
  }
}
