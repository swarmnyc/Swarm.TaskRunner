using McMaster.Extensions.CommandLineUtils;
using Swarm.TaskRunner.CLI.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

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
    public HashSet<int> SkippedSteps { get; } = new HashSet<int>();

    [Option("--no-color", Description = "If apply, disable colorful output")]
    public bool IsColorDisabled { get; }

    [Option("--verbose", Description = "If apply, output detail")]
    public bool IsVerbose { get; }

    private TaskExecuter executer;

    internal int OnExecute() {
      Console.CancelKeyPress += OnExit;

      Logger.IsColorEnabled = !IsColorDisabled;
      Logger.IsVerbose = IsVerbose;

      var context = new TaskContext() {
        TaskDefinitionFilePath = TaskDefinitionFilePath,
        EnvironmentVariables = EnvironmentVariables,
        SkippedSteps = SkippedSteps,
        WorkingDirectory = WorkingDictionary ?? Environment.CurrentDirectory
      };

      executer = new TaskExecuter(context);

      return executer.Execute();
    }

    private void OnExit(object sender, EventArgs e) {
      if (executer != null) {
        executer.Abort();
      }
    }
  }
}