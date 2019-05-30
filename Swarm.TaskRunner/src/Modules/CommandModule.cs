using System.Collections.Generic;
using System.Diagnostics;
using Swarm.TaskRunner.Definitions;
using YamlDotNet.RepresentationModel;
using System.Linq;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Swarm.TaskRunner.Modules {
  public class CommandModuleDefinition : ModuleDefinition {
    public CommandModuleDefinition(Module module, YamlMappingNode node) : base(module, node) {
    }

    public string FilePath { get; set; }
    public string WorkingDirectory { get; set; }
    public IList<string> Arguments { get; } = new List<string>();
  }

  public class CommandModule : Module {
    public Process process;

    public override ModuleDefinition Parse(string version, YamlMappingNode node) {
      var definition = new CommandModuleDefinition(this, node);
      definition.FilePath = (string)node.Children["file"];

      if (node.Children.ContainsKey("pwd")) {
        definition.WorkingDirectory = (string)node.Children["pwd"];
      }

      if (node.Children.ContainsKey("args")) {
        var args = node.Children["args"] as YamlSequenceNode;
        if (args != null) {
          foreach (var item in args) {
            definition.Arguments.Add((string)item);
          }
        }
      }

      return definition;
    }

    public override void Execute(TaskContext context, ModuleDefinition definition) {
      var def = definition as CommandModuleDefinition;

      ProcessStartInfo startInfo = new ProcessStartInfo();
      string filepath = context.GetValue(def.FilePath);
      if (filepath.StartsWith(".")) {
        filepath = Path.GetFullPath(filepath);
      }

      startInfo.FileName = filepath;
      if (def.WorkingDirectory == null) {
        startInfo.WorkingDirectory = context.WorkingDirectory;
      } else {
        startInfo.WorkingDirectory = context.GetValue(def.WorkingDirectory);
      }

      var arguments = def.Arguments.Select(a => context.GetValue(a));
      startInfo.Arguments = String.Join(" ", arguments);

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
          if (!Logger.IsVerbose) return;

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
          Process.Start(new ProcessStartInfo {
            FileName = "taskkill",
            Arguments = $"/PID {process.Id} /f /t",
            CreateNoWindow = true,
            UseShellExecute = false
          });
        } else {
          Process.Start(new ProcessStartInfo {
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
  }
}