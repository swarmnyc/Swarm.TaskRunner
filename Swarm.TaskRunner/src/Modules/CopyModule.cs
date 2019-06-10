using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using YamlDotNet.RepresentationModel;

namespace Swarm.TaskRunner.Modules {
  public class CopyModule : Module<CopyModuleDefinition> {
    public override CopyModuleDefinition Parse(string version, YamlMappingNode node) {
      return new CopyModuleDefinition(this, node);
    }

    public override void Execute(TaskContext context, CopyModuleDefinition definition) {
      Contract.Requires(context != null);
      Contract.Requires(definition != null);

      var source = GetDirectoryFullPath(context.GetValue(definition.SourcePath));
      var target = GetDirectoryFullPath(context.GetValue(definition.TargetPath));
      var pattern = context.GetValue(definition.Pattern);

      Logger.LogInfo($"Copy files from '{source}' to '{target}' with pattern '{pattern}'");

      // check target directory
      if (!Directory.Exists(target)) {
        Directory.CreateDirectory(target);
      }

      var checkedDir = new HashSet<string>();

      foreach (var path in Directory.EnumerateFileSystemEntries(source, pattern, SearchOption.AllDirectories)) {
        FileAttributes attr = File.GetAttributes(path);
        var newPath = path.Replace(source, target);
        if (attr.HasFlag(FileAttributes.Directory)) {
          if (Logger.IsVerbose) {
            Logger.LogInfo($"CREATE {newPath}");
          }

          if (!Directory.Exists(newPath)) {
            Directory.CreateDirectory(newPath);
          }
        } else {
          if (Logger.IsVerbose) {
            Logger.LogInfo($"COPY {newPath}");
          }

          // if dir not checked or not exist, create it
          var dir = Path.GetDirectoryName(newPath);

          if (!(checkedDir.Contains(dir) || Directory.Exists(dir))) {
            checkedDir.Add(dir);
            Directory.CreateDirectory(dir);
          }

          if (File.Exists(newPath)) {
            File.Delete(newPath);
          }

          File.Copy(path, newPath);
        }
      }
    }

    private static string GetDirectoryFullPath(string path) {
      var fullpath = Path.GetFullPath(path);

      if (!fullpath.EndsWith(Path.DirectorySeparatorChar)) {
        fullpath += Path.DirectorySeparatorChar;
      }

      return fullpath;
    }
  }

  public class CopyModuleDefinition : ModuleDefinition {
    public CopyModuleDefinition() {
    }

    public CopyModuleDefinition(IModule module, YamlMappingNode node) : base(module, node) {
      Contract.Requires(module != null);
      Contract.Requires(node != null);

      SourcePath = (string)node.Children["source"];
      TargetPath = (string)node.Children["target"];
      Pattern = node.Children.ContainsKey("pattern") ? (string)node.Children["pattern"] : "*";
    }

    public string SourcePath { get; set; }

    public string TargetPath { get; set; }

    public string Pattern { get; set; }
  }
}