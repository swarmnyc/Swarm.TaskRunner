using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using System.Diagnostics;
using Swarm.TaskRunner.Definitions;
using Swarm.TaskRunner.CLI;

namespace Swarm.TaskRunner {
  class Program {
    public static int Main(string[] args) {
      return CommandLineApplication.Execute<MainCommand>(args);
    }
  }
}
