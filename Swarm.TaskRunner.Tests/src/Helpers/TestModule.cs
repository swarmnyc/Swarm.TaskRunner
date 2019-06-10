using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Swarm.TaskRunner.Modules;
using YamlDotNet.RepresentationModel;

namespace Swarm.TaskRunner.Tests.Helpers {
  public class TestModule : Module<TestModuleDefinition> {
    public int CallCount { get; set; }

    public List<string> CallArgs { get; } = new List<string>();

    public override TestModuleDefinition Parse(string version, YamlMappingNode node) {
      Contract.Requires(version != null);
      Contract.Requires(node != null);

      var def = new TestModuleDefinition(this, node);

      if (node.Children.ContainsKey("arg")) {
        def.Arg = (string)node.Children["arg"];
      }

      return def;
    }

    public override void Execute(TaskContext context, TestModuleDefinition definition) {
      Contract.Requires(context != null);
      Contract.Requires(definition != null);

      CallCount++;

      if (definition.Arg != null) {
        CallArgs.Add(definition.Arg);
      }
    }
  }

  public class TestModuleDefinition : ModuleDefinition {
    public TestModuleDefinition(IModule module, YamlMappingNode node) : base(module, node) {
    }

    public string Arg { get; set; }
  }
}
