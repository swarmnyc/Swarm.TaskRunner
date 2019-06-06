using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Swarm.TaskRunner;

namespace Swarm.TaskRunner.Tests {
  public class CliTest {
    [Test]
    public void ShowHelpTest() {
      var outStream = new MemoryStream();
      var errorStream = new MemoryStream();
      Console.SetOut(new StreamWriter(outStream));
      Console.SetError(new StreamWriter(errorStream));

      var exitCode = Program.Main(new string[] { "--help" });
      Console.Out.Flush();
      Console.Error.Flush();

      outStream.Position = 0;
      errorStream.Position = 0;

      var outStr = new StreamReader(outStream).ReadLine().Trim();
      var errorStr = new StreamReader(errorStream).ReadToEnd().Trim();

      Assert.AreEqual(0, exitCode);

      var version = typeof(Program).Assembly.GetName().Version.ToString(3);
      Assert.AreEqual($"Task Runner {version}", outStr);
      Assert.AreEqual(string.Empty, errorStr);
    }

    [Test]
    public void FailIfNoArgTest() {
      var outStream = new MemoryStream();
      var errorStream = new MemoryStream();
      Console.SetOut(new StreamWriter(outStream));
      Console.SetError(new StreamWriter(errorStream));

      var exitCode = Program.Main(Array.Empty<string>());
      Console.Out.Flush();
      Console.Error.Flush();

      outStream.Position = 0;
      errorStream.Position = 0;

      var outStr = new StreamReader(outStream).ReadToEnd().Trim();
      var errorStr = new StreamReader(errorStream).ReadToEnd().Trim();

      Assert.AreEqual((int)ExitCode.CliFail, exitCode);

      Assert.AreEqual("Specify --help for a list of available options and commands.", outStr);
      Assert.AreEqual("The task definition field is required.", errorStr);
    }

    [Test]
    public void FailIfMissingRequireEnv() {
      var outStream = new MemoryStream();
      var errorStream = new MemoryStream();
      Console.SetOut(new StreamWriter(outStream));
      Console.SetError(new StreamWriter(errorStream));

      var path = Path.GetFullPath("../../../../example-definitions/example1.yml");
      var exitCode = Program.Main(new string[] { path });
      Console.Out.Flush();
      Console.Error.Flush();

      outStream.Position = 0;
      errorStream.Position = 0;

      var outStr = new StreamReader(outStream).ReadToEnd().Trim();
      var errorStr = new StreamReader(errorStream).ReadToEnd().Trim();
      Assert.AreEqual((int)ExitCode.DefinitionFail, exitCode);

      Assert.AreEqual(string.Empty, outStr);
      Assert.AreEqual("ERROR: Environment TARGET_PATH is required", errorStr);
    }

    [Test]
    public void SuccessWithRequireEnv() {
      var path = Path.GetFullPath("../../../../example-definitions/example1.yml");
      var exitCode = Program.Main(new string[] { path, "-e", "TARGET_PATH=./" });

      Assert.AreEqual((int)ExitCode.Success, exitCode);
    }
  }
}