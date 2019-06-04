using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Swarm.TaskRunner.Modules;

namespace Swarm.TaskRunner.Tests {
  public class CopyModuleTest {
    [Test]
    public void CopyTest() {
      var module = new CopyModule();
      var context = new TaskContext() {
        EnvironmentVariables = new Dictionary<string, string>(),
        SkippedSteps = new Dictionary<int, bool>(),
        WorkingDirectory = Environment.CurrentDirectory
      };

      var definition = new CopyModuleDefinition(module) {
        SourcePath = "./",
        TargetPath = "../test/",
        Pattern = "runner.dll"
      };

      module.Execute(context, definition);

      Assert.IsTrue(File.Exists("../test/runner.dll"));
    }
  }
}
