using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Swarm.TaskRunner.Logging;
using Swarm.TaskRunner.Modules;

namespace Swarm.TaskRunner.Tests {
  public class CommandModuleTest {
    [Test]
    public void CommandTest() {
      StringBuilderLogger logger;
      Logger.LogProvider = logger = new StringBuilderLogger();

      using (var module = new CommandModule()) {
        var context = new TaskContext() {
          EnvironmentVariables = new Dictionary<string, string>(),
          SkippedSteps = new HashSet<int>(),
          WorkingDirectory = "../../../"
        };

        var definition = new CommandModuleDefinition() {
          FilePath = "dotnet",
          Arguments = "list reference"
        };

        module.Execute(context, definition);

        Assert.AreEqual("Executing dotnet list reference", logger.Out.ToString(0, 31));

        Logger.ResetProvider();
      }
    }
  }
}
