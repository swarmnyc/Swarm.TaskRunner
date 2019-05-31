using Swarm.TaskRunner.Modules;
using System;
using System.Collections.Generic;
using YamlDotNet.RepresentationModel;

namespace Swarm.TaskRunner.Definitions.Parser {
  public class TaskDefinitionParserV100 : ITaskDefinitionParser {
    private TaskDefinition definition;

    public TaskDefinition Parse(YamlMappingNode rootNode) {
      definition = new TaskDefinition();
      definition.Version = "1.0.0";

      foreach (var node in rootNode.Children) {
        var name = (string)node.Key;
        try {
          switch (name) {
            case "label":
              definition.Label = (string)node.Value;
              break;

            case "modules":
              ParseModules(node.Value);
              break;

            case "required-envs":
              ParseRequiredEnvs(node.Value);
              break;

            case "optional-envs":
              ParseOptionalEnv(node.Value);
              break;

            case "steps":
              ParseModuleDefinitions(definition.Steps, node.Value);
              break;

            case "onError":
              ParseModuleDefinitions(definition.OnError, node.Value);
              break;
          }
        } catch (Exception ex) {
          throw new Exception($"Parse {name} failed", ex);
        }
      }

      return definition;
    }

    private void ParseModules(YamlNode node) {
      foreach (var item in (node as YamlMappingNode).Children) {
        var name = (string)item.Key;
        var typeName = (string)item.Value;

        var type = System.Type.GetType(typeName);

        if (type == null) {
          throw new Exception($"Cannot find {typeName} module");
        }

        if (!typeof(Module).IsAssignableFrom(type)) {
          throw new Exception($"{typeName} is not inherited from Module");
        }

        var instance = Activator.CreateInstance(type) as Module;

        definition.Modules[name] = instance;
      }
    }

    private void ParseRequiredEnvs(YamlNode node) {
      foreach (string item in node as YamlSequenceNode) {
        if (definition.EnvironmentDefinitions.ContainsKey(item)) {
          definition.EnvironmentDefinitions[item].IsRequired = true;
        } else {
          definition.EnvironmentDefinitions.Add(item, new EnvDefinition() {
            Name = item,
            IsRequired = true
          });
        }
      }
    }

    private void ParseOptionalEnv(YamlNode node) {
      foreach (YamlMappingNode item in node as YamlSequenceNode) {
        var name = (string)item.Children["name"];
        var defaultValue = (string)item.Children["defaultValue"];

        if (definition.EnvironmentDefinitions.ContainsKey(name)) {
          definition.EnvironmentDefinitions[name].Value = defaultValue;
        } else {
          definition.EnvironmentDefinitions.Add(name, new EnvDefinition() {
            Name = name,
            Value = defaultValue
          });
        }
      }
    }

    private void ParseModuleDefinitions(ICollection<ModuleDefinition> list, YamlNode node) {
      if (node is YamlSequenceNode items) {
        foreach (YamlMappingNode item in items) {
          var moduleName = (string)item.Children["module"];
          Module module;
          if (definition.Modules.TryGetValue(moduleName, out module)) {
            var def = module.Parse(definition.Version, item);
            list.Add(def);
          } else {
            throw new Exception($"Cannot find {moduleName} in modules");
          }
        }
      }
    }
  }
}