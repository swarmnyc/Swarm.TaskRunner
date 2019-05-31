using System.IO;
using YamlDotNet.RepresentationModel;

namespace Swarm.TaskRunner.Modules {
  public class CopyModuleDefinition : ModuleDefinition {

    public CopyModuleDefinition(Module module, YamlMappingNode node) : base(module, node) {
    }

    public string SourcePath { get; set; }
    public string TargetPath { get; set; }
    public string Pattern { get; set; }
  }

  public class CopyModule : Module {

    public override ModuleDefinition Parse(string version, YamlMappingNode node) {
      return new CopyModuleDefinition(this, node) {
        SourcePath = (string)node.Children["source"],
        TargetPath = (string)node.Children["target"],
        Pattern = node.Children.ContainsKey("pattern") ? (string)node.Children["pattern"] : "*"
      };
    }

    public override void Execute(TaskContext context, ModuleDefinition definition) {
      var def = definition as CopyModuleDefinition;
      var source = getDirectoryFullPath(context.GetValue(def.SourcePath));
      var target = getDirectoryFullPath(context.GetValue(def.TargetPath));
      var pattern = context.GetValue(def.Pattern);

      Logger.LogInfo($"Copy files from '{source}' to '{target}' with pattern '{pattern}'");

      //check target directory
      if (!Directory.Exists(target)) {
        Directory.CreateDirectory(target);
      }

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

          if (File.Exists(newPath)) {
            File.Delete(newPath);
          }

          File.Copy(path, newPath);
        }
      }
    }

    private static string getDirectoryFullPath(string path) {
      var fullpath = Path.GetFullPath(path);

      if (!fullpath.EndsWith(Path.DirectorySeparatorChar.ToString())) {
        fullpath += Path.DirectorySeparatorChar;
      }

      return fullpath;
    }
  }
}