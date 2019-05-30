using System;
using System.IO;
using NUnit.Framework;
using Swarm.TaskRunner;

namespace Swarm.TaskRunner.Tests {
  public class CliTests {
    [SetUp]
    public void Setup() {
    }

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
      Assert.AreEqual("", errorStr);
    }
    [Test]
    public void FailIfNoArgTest() {
      var outStream = new MemoryStream();
      var errorStream = new MemoryStream();
      Console.SetOut(new StreamWriter(outStream));
      Console.SetError(new StreamWriter(errorStream));

      var exitCode = Program.Main(new string[] { });
      Console.Out.Flush();
      Console.Error.Flush();

      outStream.Position = 0;
      errorStream.Position = 0;

      var outStr = new StreamReader(outStream).ReadToEnd().Trim();
      var errorStr = new StreamReader(errorStream).ReadToEnd().Trim();

      Assert.AreEqual(1, exitCode);

      Assert.AreEqual("Specify --help for a list of available options and commands.", outStr);
      Assert.AreEqual("The task definition field is required.", errorStr);
    }
  }
}