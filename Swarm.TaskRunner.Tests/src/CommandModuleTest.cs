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

      var module = new CommandModule();
      var context = new TaskContext() {
        EnvironmentVariables = new Dictionary<string, string>(),
        SkippedSteps = new HashSet<int>(),
        WorkingDirectory = "../../../../"
      };

      var definition = new CommandModuleDefinition(module) {
        FilePath = "dotnet",
        Arguments = "list package"
      };

      module.Execute(context, definition);

      Assert.AreEqual("Executing dotnet list package", logger.Out.ToString(0, 29));

      Logger.ResetProvider();
    }
  }
}
