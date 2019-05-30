using YamlDotNet.RepresentationModel;

namespace Swarm.TaskRunner.Definitions.Parser {
  public interface ITaskDefinitionParser {
    TaskDefinition Parse(YamlMappingNode rootNode);
  }
}