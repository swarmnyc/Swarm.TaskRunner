using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using Swarm.TaskRunner.Logging;
using Swarm.TaskRunner.Modules;

namespace Swarm.TaskRunner.Tests {
  public class RequestModuleTest {
    [Test]
    public void RequestTest() {
      StringBuilderLogger logger;
      Logger.LogProvider = logger = new StringBuilderLogger();

      var module = new RequestModule();
      var context = new TaskContext() {
        EnvironmentVariables = new Dictionary<string, string>(),
        SkippedSteps = new HashSet<int>(),
        WorkingDirectory = Environment.CurrentDirectory
      };

      var url = "https://postman-echo.com/get?foo1=bar1&foo2=bar2";
      var definition = new RequestModuleDefinition(module) {
        Method = "Get",
        Url = url,
        OutputToEnv = "RES",
        OutputToFile = "res.txt"
      };

      module.Execute(context, definition);

      dynamic result = JsonConvert.DeserializeObject(context.EnvironmentVariables["RES"]);

      Assert.AreEqual($"Request Get {url}{Environment.NewLine}", logger.Out.ToString());
      Assert.AreEqual("{\r\n  \"foo1\": \"bar1\",\r\n  \"foo2\": \"bar2\"\r\n}", result.args.ToString());
      Assert.IsTrue(File.Exists("res.txt"));

      Logger.ResetProvider();
    }

    [Test]
    public void RequestOutputImageTest() {
      StringBuilderLogger logger;
      Logger.LogProvider = logger = new StringBuilderLogger();

      var module = new RequestModule();
      var context = new TaskContext() {
        EnvironmentVariables = new Dictionary<string, string>(),
        SkippedSteps = new HashSet<int>(),
        WorkingDirectory = Environment.CurrentDirectory
      };

      var url = "https://via.placeholder.com/150";
      var definition = new RequestModuleDefinition(module) {
        Method = "GET",
        Url = url,
        OutputToFile = "img.png"
      };

      module.Execute(context, definition);

      Assert.AreEqual($"Request GET {url}{Environment.NewLine}", logger.Out.ToString());
      Assert.IsTrue(File.Exists("img.png"));

      Logger.ResetProvider();
    }
  }
}
