using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Swarm.TaskRunner;

namespace Swarm.TaskRunner.Tests {
  public class CliTest {
    [Test]
    public void ShowHelpTest() {
      using (var outStream = new MemoryStream())
      using (var errorStream = new MemoryStream())
      using (var outStreamWriter = new StreamWriter(outStream))
      using (var errorStreamSwriter = new StreamWriter(errorStream)) {
        var oldOutWriter = Console.Out;
        var oldErrorWriter = Console.Error;

        Console.SetOut(outStreamWriter);
        Console.SetError(errorStreamSwriter);

        var exitCode = Program.Main(new string[] { "--help" });
        Console.Out.Flush();
        Console.Error.Flush();

        outStream.Position = 0;
        errorStream.Position = 0;

        using (var outStreamReader = new StreamReader(outStream))
        using (var errorStreamReader = new StreamReader(errorStream)) {
          var outStr = outStreamReader.ReadLine().Trim();
          var errorStr = errorStreamReader.ReadToEnd().Trim();

          Assert.AreEqual(0, exitCode);

          var version = typeof(Program).Assembly.GetName().Version.ToString(3);
          Assert.AreEqual($"Task Runner {version}", outStr);
          Assert.AreEqual(string.Empty, errorStr);
        }

        Console.SetOut(oldOutWriter);
        Console.SetError(oldErrorWriter);
      }
    }

    [Test]
    public void FailIfNoArgTest() {
      using (var outStream = new MemoryStream())
      using (var errorStream = new MemoryStream())
      using (var outStreamWriter = new StreamWriter(outStream))
      using (var errorStreamSwriter = new StreamWriter(errorStream)) {
        var oldOutWriter = Console.Out;
        var oldErrorWriter = Console.Error;

        Console.SetOut(outStreamWriter);
        Console.SetError(errorStreamSwriter);

        var exitCode = Program.Main(Array.Empty<string>());
        Console.Out.Flush();
        Console.Error.Flush();

        outStream.Position = 0;
        errorStream.Position = 0;

        using (var outStreamReader = new StreamReader(outStream))
        using (var errorStreamReader = new StreamReader(errorStream)) {
          var outStr = outStreamReader.ReadToEnd().Trim();
          var errorStr = errorStreamReader.ReadToEnd().Trim();

          Assert.AreEqual((int)ExitCode.CliFail, exitCode);

          Assert.AreEqual("Specify --help for a list of available options and commands.", outStr);
          Assert.AreEqual("The task definition field is required.", errorStr);
        }

        Console.SetOut(oldOutWriter);
        Console.SetError(oldErrorWriter);
      }
    }

    [Test]
    public void FailIfMissingRequireEnv() {
      using (var outStream = new MemoryStream())
      using (var errorStream = new MemoryStream())
      using (var outStreamWriter = new StreamWriter(outStream))
      using (var errorStreamSwriter = new StreamWriter(errorStream)) {
        var oldOutWriter = Console.Out;
        var oldErrorWriter = Console.Error;

        Console.SetOut(outStreamWriter);
        Console.SetError(errorStreamSwriter);

        var path = Path.GetFullPath("../../../../example-definitions/example1.yml");
        var exitCode = Program.Main(new string[] { path });
        Console.Out.Flush();
        Console.Error.Flush();

        outStream.Position = 0;
        errorStream.Position = 0;

        using (var outStreamReader = new StreamReader(outStream))
        using (var errorStreamReader = new StreamReader(errorStream)) {
          var outStr = outStreamReader.ReadToEnd().Trim();
          var errorStr = errorStreamReader.ReadToEnd().Trim();
          Assert.AreEqual((int)ExitCode.DefinitionFail, exitCode);

          Assert.AreEqual(string.Empty, outStr);
          Assert.AreEqual("ERROR: Environment TARGET_PATH is required", errorStr);
        }

        Console.SetOut(oldOutWriter);
        Console.SetError(oldErrorWriter);
      }
    }

    [Test]
    public void SuccessWithRequireEnv() {
      var path = Path.GetFullPath("../../../../example-definitions/example1.yml");
      var exitCode = Program.Main(new string[] { path, "-e", "TARGET_PATH=./" });

      Assert.AreEqual((int)ExitCode.Success, exitCode);
    }
  }
}