using McMaster.Extensions.CommandLineUtils;
using Swarm.TaskRunner.CLI;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Swarm.TaskRunner.Tests")]

namespace Swarm.TaskRunner {
  internal class Program {
    internal static int Main(string[] args) {
      return CommandLineApplication.Execute<MainCommand>(args);
    }
  }
}