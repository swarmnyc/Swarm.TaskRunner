using Swarm.TaskRunner.Definitions;
using Swarm.TaskRunner.Modules;
using System;
using System.IO;
using System.Linq;

namespace Swarm.TaskRunner {
  public class TaskExecuter {
    private ModuleDefinition currentStep;

    public TaskExecuter(TaskContext context) {
      Context = context;
    }

    public bool IsAborted { get; private set; }
    public TaskContext Context { get; private set; }
    public TaskDefinition Definition { get; private set; }

    public int Execute() {
      // prepare
      try {
        Definition = TaskDefinitionParser.Parse(Context.TaskDefinitionFilePath);

        PrepareWorkingDirectory();

        CheckEnvironmentVariables();
      } catch (Exception ex) {
        Logger.LogError("ERROR: {0}", ex);
        return (int)ExitCode.DefinitionFail;
      }

      // execute steps
      var start = DateTime.Now;

      try {
        Logger.LogInfo($"Start {Definition.Label}");

        ExecuteSteps();
        return (int)ExitCode.Success;
      } catch (Exception ex) {
        Logger.LogError("ERROR: {0}", ex);

        HandleOnError();
        return (int)ExitCode.ExecuteFail;
      } finally {
        var end = DateTime.Now - start;
        Logger.LogHint($"\nTotal elapsed: {end.ToString("c")}");
      }
    }

    private void PrepareWorkingDirectory() {
      if (!Directory.Exists(Context.WorkingDirectory)) {
        Directory.CreateDirectory(Context.WorkingDirectory);
      }
    }

    private void CheckEnvironmentVariables() {
      foreach (var env in Definition.EnvironmentDefinitions.Values) {
        if (env.Value != null && !Context.EnvironmentVariables.ContainsKey(env.Name)) {
          Context.EnvironmentVariables.Add(env.Name, env.Value);
        }

        if (env.IsRequired && !Context.EnvironmentVariables.ContainsKey(env.Name)) {
          throw new Exception($"Environment {env.Name} is required");
        }
      }
    }

    private void ExecuteSteps() {
      var time = DateTime.Now;
      foreach (var (step, index) in Definition.Steps.Select((step, index) => (step, index))) {
        if (IsAborted) return;

        if (Context.SkippedSteps.Contains(index)) {
          if (step.Label == null) {
            Logger.LogHint($"\n[{index + 1}/{Definition.Steps.Count}] skipped");
          } else {
            Logger.LogHint($"\n[{index + 1}/{Definition.Steps.Count}] {step.Label} is skipped");
          }
          continue;
        }

        Logger.LogHint($"\n[{index + 1}/{Definition.Steps.Count}] {step.Label}");

        var start = DateTime.Now;

        currentStep = step;

        step.Module.Execute(Context, step);

        var end = DateTime.Now - start;
        Logger.LogHint($"Step elapsed: {end.ToString("c")}");
      }

      currentStep = null;
    }

    private void HandleOnError() {
      try {
        foreach (var def in Definition.OnError) {
          def.Module.Execute(Context, def);
        }
      } catch (Exception ex) {
        Logger.LogError("ERROR: handle onError failed by {0}", ex);
      }
    }

    internal void Abort() {
      IsAborted = true;
      if (currentStep != null) {
        currentStep.Module.Abort();
        currentStep = null;
        Console.WriteLine("Aborted");
      }
    }
  }
}