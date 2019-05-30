using System;
using System.IO;
using Swarm.TaskRunner.Definitions.Parser;
using YamlDotNet.RepresentationModel;

namespace Swarm.TaskRunner.Definitions {
  public static class TaskDefinitionParser {
    public static TaskDefinition Parse(string filepath) {
      using (var file = File.OpenText(filepath)) {
        var yaml = new YamlStream();
        yaml.Load(file);

        var rootNode = yaml.Documents[0].RootNode as YamlMappingNode;
        var versionNode = rootNode.Children["version"];
        if (versionNode is YamlScalarNode v && v.Value is string version) {
          switch (version) {
            case "1.0.0":
              return new TaskDefinitionParserV100().Parse(rootNode);
            default:
              throw new Exception($"{version} is not supported version");
          }
        } else {
          throw new Exception("version is not defined");
        }
      }
    }
  }
}