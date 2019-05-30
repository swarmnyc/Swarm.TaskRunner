using System.Runtime.CompilerServices;
using McMaster.Extensions.CommandLineUtils;
using Swarm.TaskRunner.CLI;

[assembly: InternalsVisibleTo("Swarm.TaskRunner.Tests")]

namespace Swarm.TaskRunner {
  class Program {
    public static int Main(string[] args) {
      return CommandLineApplication.Execute<MainCommand>(args);
    }
  }
}
