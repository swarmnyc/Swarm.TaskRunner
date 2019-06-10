using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using Swarm.TaskRunner.Logging;
using Swarm.TaskRunner.Modules;
using Swarm.TaskRunner.Modules.Dockers;

namespace Swarm.TaskRunner.Tests.Dockers {
  // Needs a docker engine to run this Test
  public class PsModuleTest {
    [Test]
    public void PsTest() {
      var module = new PsModule();
      var context = new TaskContext() {
        EnvironmentVariables = new Dictionary<string, string>(),
        SkippedSteps = new HashSet<int>(),
        WorkingDirectory = Environment.CurrentDirectory
      };

      var definition = new PsModuleDefinition();
      module.Execute(context, definition);

      Assert.IsTrue(context.EnvironmentVariables.ContainsKey("DOCKER_CONTAINER_COUNT"));
    }
  }
}
