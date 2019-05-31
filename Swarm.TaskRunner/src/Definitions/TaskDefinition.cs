using Swarm.TaskRunner.Modules;
using System.Collections.Generic;

namespace Swarm.TaskRunner.Definitions {
  public class TaskDefinition {
    public string Label { get; set; }
    public string Version { get; set; }
    public IDictionary<string, Module> Modules { get; } = new Dictionary<string, Module>();
    public IDictionary<string, EnvDefinition> EnvironmentDefinitions { get; } = new Dictionary<string, EnvDefinition>();
    public ICollection<ModuleDefinition> Steps { get; } = new List<ModuleDefinition>();
    public ICollection<ModuleDefinition> OnError { get; } = new List<ModuleDefinition>();
  }
}