using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Swarm.TaskRunner.Definitions;
using Swarm.TaskRunner.Modules;

namespace Swarm.TaskRunner {
  public class TaskExecuter {
    private readonly TaskContext context;
    private readonly TaskDefinition definition;
    private bool isAborted = false;
    private ModuleDefinition currentStep;

    public TaskExecuter(TaskContext context, TaskDefinition definition) {
      this.context = context;
      this.definition = definition;
    }

    public int Execute() {
      var start = DateTime.Now;
      try {
        Logger.LogInfo($"Start {definition.Label}");
        PrepareWorkingDirectory();

        CheckEnvironmentVariables();

        ExecuteSteps();
        return 0;
      } catch (Exception ex) {
        Logger.LogError("ERROR: {0}", ex);

        HandleOnError();
        return 1;
      } finally {
        var end = DateTime.Now - start;
        Logger.LogHint($"\nTotal elapsed: {end.ToString("c")}");
      }
    }

    private void PrepareWorkingDirectory() {
      if (!Directory.Exists(context.WorkingDirectory)) {
        Directory.CreateDirectory(context.WorkingDirectory);
      }
    }

    private void CheckEnvironmentVariables() {
      foreach (var env in definition.EnvironmentDefinitions.Values) {
        if (env.Value != null && !this.context.EnvironmentVariables.ContainsKey(env.Name)) {
          this.context.EnvironmentVariables.Add(env.Name, env.Value);
        }

        if (env.IsRequired && !this.context.EnvironmentVariables.ContainsKey(env.Name)) {
          throw new Exception($"Environment {env.Name} is required");
        }
      }
    }

    private void ExecuteSteps() {
      var time = DateTime.Now;
      foreach (var (step, index) in definition.Steps.Select((step, index) => (step, index))) {
        if (isAborted) return;

        bool isSkipped;
        if (context.SkippedSteps.TryGetValue(index, out isSkipped) && isSkipped) {
          if (step.Label == null) {
            Logger.LogHint($"\n[{index + 1}/{definition.Steps.Count}] skipped");
          } else {
            Logger.LogHint($"\n[{index + 1}/{definition.Steps.Count}] {step.Label} is skipped");
          }
          continue;
        }

        Logger.LogHint($"\n[{index + 1}/{definition.Steps.Count}] {step.Label}");

        var start = DateTime.Now;

        currentStep = step;

        step.Module.Execute(context, step);

        var end = DateTime.Now - start;
        Logger.LogHint($"Step elapsed: {end.ToString("c")}");
      }

      currentStep = null;
    }

    private void HandleOnError() {
      try {
        foreach (var def in definition.OnError) {
          def.Module.Execute(context, def);
        }
      } catch (Exception ex) {
        Logger.LogError("ERROR: handle onError failed by {0}", ex);
      }
    }

    internal void Abort() {
      isAborted = true;
      if (currentStep != null) {
        currentStep.Module.Abort();
        currentStep = null;
        Console.WriteLine("Aborted");
      }
    }
  }
}