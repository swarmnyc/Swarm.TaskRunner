using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Swarm.TaskRunner;

namespace Swarm.TaskRunner.Tests {
  public class TaskExecuterTest {
    [Test]
    public void EnvTest() {
      var context = new TaskContext() {
        EnvironmentVariables = new Dictionary<string, string>() {
          { "A", "aa" },
          { "B", "bb" }
        }
      };

      Assert.AreEqual("aa-bb-aa", context.GetValue("${A}-${B}-${A}"));
    }

    [Test]
    public void EnvMissingTest() {
      try {
        var context = new TaskContext() {
          EnvironmentVariables = new Dictionary<string, string>() {
            { "A", "aa" }
          }
        };

        context.GetValue("${A}-${B}-${A}");
        Assert.Fail();
      } catch (System.Exception ex) {
        Assert.AreEqual("Environment Variable \"B\" is not found", ex.Message);
      }
    }

    [Test]
    public void OnErrorGotCallRequireEnv() {
      var path = Path.GetFullPath("../../../../example-definitions/example2.yml");

      var context = new TaskContext() {
        TaskDefinitionFilePath = path,
        EnvironmentVariables = new Dictionary<string, string>(),
        SkippedSteps = new HashSet<int>(),
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
        SkippedSteps = new HashSet<int>(),
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
        SkippedSteps = new HashSet<int>() { 0, 2 }, // zero-based
        WorkingDirectory = Environment.CurrentDirectory
      };

      var executer = new TaskExecuter(context);
      var exitCode = executer.Execute();
      var testModule = executer.Definition.Modules["test"] as TestModule;

      Assert.AreEqual((int)ExitCode.Success, exitCode);
      Assert.AreEqual(1, testModule.CallCount);
      Assert.AreEqual("2", testModule.CallArgs[0]);
    }
  }
}