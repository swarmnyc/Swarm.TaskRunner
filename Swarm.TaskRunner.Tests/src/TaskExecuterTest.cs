using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Swarm.TaskRunner;

namespace Swarm.TaskRunner.Tests {
  public class TaskExecuterTest {
    [Test]
    public void OnErrorGotCallRequireEnv() {
      var path = Path.GetFullPath("../../../../example-definitions/example2.yml");

      var context = new TaskContext() {
        TaskDefinitionFilePath = path,
        EnvironmentVariables = new Dictionary<string, string>(),
        SkippedSteps = new Dictionary<int, bool>(),
        WorkingDirectory = Environment.CurrentDirectory
      };

      var executer = new TaskExecuter(context);
      var exitCode = executer.Execute();
      var testModule = executer.Definition.Modules["test"] as TestModule;

      Assert.AreEqual((int)ExitCode.ExecuteFail, exitCode);
      Assert.AreEqual(1, testModule.CallCount);
    }

    [Test]
    public void SuccessTest() {
      var path = Path.GetFullPath("../../../../example-definitions/example3.yml");

      var context = new TaskContext() {
        TaskDefinitionFilePath = path,
        EnvironmentVariables = new Dictionary<string, string>(),
        SkippedSteps = new Dictionary<int, bool>(),
        WorkingDirectory = Environment.CurrentDirectory
      };

      var executer = new TaskExecuter(context);
      var exitCode = executer.Execute();
      var testModule = executer.Definition.Modules["test"] as TestModule;

      Assert.AreEqual((int)ExitCode.Success, exitCode);
      Assert.AreEqual(3, testModule.CallCount);
    }

    [Test]
    public void SkipTest() {
      var path = Path.GetFullPath("../../../../example-definitions/example3.yml");

      var context = new TaskContext() {
        TaskDefinitionFilePath = path,
        EnvironmentVariables = new Dictionary<string, string>(),
        SkippedSteps = new Dictionary<int, bool>(),
        WorkingDirectory = Environment.CurrentDirectory
      };

      var executer = new TaskExecuter(context);
      var exitCode = executer.Execute();
      var testModule = executer.Definition.Modules["test"] as TestModule;

      Assert.AreEqual((int)ExitCode.Success, exitCode);
      Assert.AreEqual(3, testModule.CallCount);
    }
  }
}