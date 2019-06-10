using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using Swarm.TaskRunner.Logging;
using Swarm.TaskRunner.Modules;
using Swarm.TaskRunner.Tests.Helpers;

namespace Swarm.TaskRunner.Tests {
  public class RequestModuleTest {
    [Test]
    public void RequestTest() {
      var args = "{\r\n  \"foo1\": \"bar1\",\r\n  \"foo2\": \"bar2\"\r\n}";
      StringBuilderLogger logger;
      Logger.LogProvider = logger = new StringBuilderLogger();

      // for real test
      // var module = new RequestModule();

      // for mock test
      var module = new MockRequestModule(new HttpResponseMessage() {
        Content = new StringContent("{\"args\":" + args + "}")
      });

      var context = new TaskContext() {
        EnvironmentVariables = new Dictionary<string, string>(),
        SkippedSteps = new HashSet<int>(),
        WorkingDirectory = Environment.CurrentDirectory
      };

      var url = "https://postman-echo.com/get?foo1=bar1&foo2=bar2";
      var definition = new RequestModuleDefinition() {
        Method = "Get",
        Url = url,
        OutputToEnv = "RES",
        OutputToFile = "res.txt"
      };

      module.Execute(context, definition);

      dynamic result = JsonConvert.DeserializeObject(context.EnvironmentVariables["RES"]);

      Assert.AreEqual($"Request Get {url}{Environment.NewLine}", logger.Out.ToString());
      Assert.AreEqual(args, result.args.ToString());
      Assert.IsTrue(File.Exists("res.txt"));

      Logger.ResetProvider();
    }

    [Test]
    public void RequestOutputImageTest() {
      StringBuilderLogger logger;
      Logger.LogProvider = logger = new StringBuilderLogger();

      // for real test
      // var module = new RequestModule();

      // for mock test
      var module = new MockRequestModule(new HttpResponseMessage() {
        Content = new ByteArrayContent(File.ReadAllBytes("Swarm.TaskRunner.Tests.deps.json")) {
          Headers = {
            { "Content-Type", System.Net.Mime.MediaTypeNames.Application.Octet }
          }
        }
      });

      var context = new TaskContext() {
        EnvironmentVariables = new Dictionary<string, string>(),
        SkippedSteps = new HashSet<int>(),
        WorkingDirectory = Environment.CurrentDirectory
      };

      var url = "https://via.placeholder.com/150";
      var definition = new RequestModuleDefinition() {
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
