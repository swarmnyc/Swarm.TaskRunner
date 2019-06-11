using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Docker.DotNet.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using Swarm.TaskRunner.Logging;
using Swarm.TaskRunner.Modules;
using Swarm.TaskRunner.Modules.Dockers;

namespace Swarm.TaskRunner.Tests.Dockers {
  // Needs a docker engine to run this Test
  public class RunModuleTest {
    [Test]
    public void RunTest() {
      Logger.IsVerbose = true;
      var module = new RunModule();
      var context = new TaskContext() {
        EnvironmentVariables = new Dictionary<string, string>(),
        SkippedSteps = new HashSet<int>(),
        WorkingDirectory = Environment.CurrentDirectory
      };

      var definition = new RunModuleDefinition() {
        Image = "ubuntu:latest",
        Name = "Test-" + DateTime.UtcNow.Ticks.ToString(),
        Cmd = new List<string>() {
          "echo",
          "\"hello world\""
        },
        Remove = true
      };

      module.Execute(context, definition);

      Assert.IsTrue(context.EnvironmentVariables.ContainsKey("DOCKER_CONTAINER_ID"));
    }
  }
}
