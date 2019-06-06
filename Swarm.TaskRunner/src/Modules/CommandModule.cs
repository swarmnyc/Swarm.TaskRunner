using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using YamlDotNet.RepresentationModel;

namespace Swarm.TaskRunner.Modules {
  public sealed class CommandModule : Module<CommandModuleDefinition>, IDisposable {
    private Process process;

    public override CommandModuleDefinition Parse(string version, YamlMappingNode node) {
      var definition = new CommandModuleDefinition(this, node) {
        FilePath = (string)node.Children["file"]
      };

      if (node.Children.ContainsKey("pwd")) {
        definition.WorkingDirectory = (string)node.Children["pwd"];
      }

      if (node.Children.ContainsKey("args")) {
        var child = node.Children["args"];
        if (child is YamlScalarNode) {
          definition.Arguments = (string)child;
        } else {
          if (node.Children["args"] is YamlSequenceNode args) {
            foreach (var item in args) {
              definition.ArgumentList.Add((string)item);
            }
          }
        }
      }

      return definition;
    }

    public override void Execute(TaskContext context, CommandModuleDefinition definition) {
      ProcessStartInfo startInfo = new ProcessStartInfo();
      string filepath = context.GetValue(definition.FilePath);
      if (filepath.StartsWith(".")) {
        filepath = Path.GetFullPath(filepath);
      }

      startInfo.FileName = filepath;
      if (definition.WorkingDirectory == null) {
        startInfo.WorkingDirectory = context.WorkingDirectory;
      } else {
        startInfo.WorkingDirectory = context.GetValue(definition.WorkingDirectory);
      }

      if (definition.Arguments == null) {
        var arguments = definition.ArgumentList.Select(a => context.GetValue(a));
        startInfo.Arguments = string.Join(" ", arguments);
      } else {
        startInfo.Arguments = context.GetValue(definition.Arguments);
      }

      foreach (var item in context.EnvironmentVariables) {
        startInfo.EnvironmentVariables.Add(item.Key, item.Value);
      }

      startInfo.CreateNoWindow = true;
      startInfo.UseShellExecute = false;
      startInfo.ErrorDialog = false;
      startInfo.RedirectStandardError = true;
      startInfo.RedirectStandardOutput = true;

      using (process = new Process()) {
        process.StartInfo = startInfo;

        process.OutputDataReceived += (sender, e) => {
          if (!Logger.IsVerbose) {
            return;
          }

          if (e.Data != null && e.Data.Length > 0) {
            Logger.LogInfo(e.Data);
          }
        };

        process.ErrorDataReceived += (sender, e) => {
          if (e.Data != null && e.Data.Length > 0) {
            Logger.LogInfo(e.Data);
          }
        };

        process.Start();

        Logger.LogInfo($"Executing {filepath} {startInfo.Arguments} and PID is {process.Id}");

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        if (process.ExitCode != 0) {
          throw new Exception($"Step failed with code {process.ExitCode}");
        }
      }

      process = null;
    }

    public override void Abort() {
      if (process != null) {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
          Process.Start(new ProcessStartInfo() {
            FileName = "taskkill",
            Arguments = $"/PID {process.Id} /f /t",
            CreateNoWindow = true,
            UseShellExecute = false
          });
        } else {
          Process.Start(new ProcessStartInfo() {
            FileName = "kill",
            Arguments = $"-TERM {process.Id}",
            CreateNoWindow = true,
            UseShellExecute = false
          });
        }

        process.Close();
        process = null;
      }
    }

    public void Dispose() {
      process.Close();
      process = null;
    }
  }

  public class CommandModuleDefinition : ModuleDefinition {
    public CommandModuleDefinition(IModule module) : base(module) {
    }

    public CommandModuleDefinition(IModule module, YamlMappingNode node) : base(module, node) {
    }

    public string FilePath { get; set; }

    public string WorkingDirectory { get; set; }

    public string Arguments { get; set; }

    public IList<string> ArgumentList { get; } = new List<string>();
  }
}