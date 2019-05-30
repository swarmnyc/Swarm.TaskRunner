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
using Swarm.TaskRunner.CLI.Attributes;

namespace Swarm.TaskRunner.CLI {
  [Command("runner", FullName = "Task Runner")]
  [VersionOptionFromMember("-v|--version", MemberName = nameof(GetVersion))]
  public class MainCommand {
    private static string GetVersion() {
      return typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
    }

    [Argument(0, "task definition", Description = "The yml file path of the task definition"), Required, FileExists]
    public string TaskDefinitionFilePath { get; }

    [Option("-w|--working-directory <path>", Description = "The path of the working directory")]
    public string WorkingDictionary { get; }

    [EnvironmentVariablesAttribute]
    public Dictionary<string, string> EnvironmentVariables { get; } = new Dictionary<string, string>();

    [SkippedSteps]
    public Dictionary<int, bool> SkippedSteps { get; } = new Dictionary<int, bool>();

    [Option("--no-color", Description = "If apply, disable colorful output")]
    public bool IsColorDisabled { get; }

    [Option("--verbose", Description = "If apply, output detail")]
    public bool IsVerbose { get; }

    private TaskExecuter executer;
    private int OnExecute() {
      Console.CancelKeyPress += OnExit;

      Logger.IsColorEnabled = !IsColorDisabled;
      Logger.IsVerbose = IsVerbose;

      try {
        var definition = TaskDefinitionParser.Parse(TaskDefinitionFilePath);
        var context = new TaskContext() {
          EnvironmentVariables = EnvironmentVariables,
          SkippedSteps = SkippedSteps,
          WorkingDirectory = WorkingDictionary ?? System.Environment.CurrentDirectory
        };

        executer = new TaskExecuter(context, definition);

        return executer.Execute();
      } catch (Exception ex) {
        Logger.LogError("ERROR: Task failed by {0}", ex);

        return 1;
      }
    }

    private void OnExit(object sender, EventArgs e) {
      if (executer != null) {
        executer.Abort();
      }
    }
  }
}
