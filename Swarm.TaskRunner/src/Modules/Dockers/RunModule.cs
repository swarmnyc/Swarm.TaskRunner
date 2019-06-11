using Docker.DotNet;
using Docker.DotNet.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace Swarm.TaskRunner.Modules.Dockers {
  public class RunModule : DockerModule<RunModuleDefinition> {
    public override RunModuleDefinition Parse(string version, YamlMappingNode node) {
      return new RunModuleDefinition(this, node);
    }

    public override void Execute(TaskContext context, RunModuleDefinition definition) {
      ExecuteAsync(context, definition).Wait();
    }

    private async Task ExecuteAsync(TaskContext context, RunModuleDefinition definition) {
      Contract.Requires(context != null);
      Contract.Requires(definition != null);

      var client = CreateClient(definition);

      await EnsureImageAsync(client, definition.Image);

      var result = await client.Containers.CreateContainerAsync(new CreateContainerParameters() {
        Cmd = definition.Cmd,
        Entrypoint = definition.Entrypoint,
        Env = definition.Env,
        Image = definition.Image,
        Labels = definition.Labels,
        Name = definition.Name,
        Volumes = definition.Volumes,
        WorkingDir = definition.WorkingDir
      });

      var containerId = result.ID;

      await client.Containers.StartContainerAsync(containerId, new ContainerStartParameters());

      context.EnvironmentVariables[$"DOCKER_CONTAINER_ID"] = containerId;

      Logger.LogInfo($"Container {containerId} created and ran");

      if (definition.Remove || definition.Wait) {
        await client.Containers.WaitContainerAsync(containerId);

        if (Logger.IsVerbose) {
          var logStream = await client.Containers.GetContainerLogsAsync(containerId, new ContainerLogsParameters() {
            ShowStdout = true
          });

          using (var reader = new StreamReader(logStream)) {
            Logger.LogInfo(reader.ReadToEnd());
          }
        }

        if (definition.Remove) {
          await client.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters());

          Logger.LogInfo($"Container {containerId} removed");
        }
      }
    }

    private async Task EnsureImageAsync(DockerClient client, string image) {
      // check images
      var images = await client.Images.ListImagesAsync(new ImagesListParameters() {
        MatchName = image
      });

      if (images.Count == 0) {
        var progress = new Progress<JSONMessage>(msg => {
          if (Logger.IsVerbose) {
            Logger.LogInfo($"{msg.Status}|{msg.ProgressMessage}|{msg.ErrorMessage}");
          }
        });

        await client.Images.CreateImageAsync(
          new ImagesCreateParameters() {
            FromImage = image
          },
          null,
          progress);
      }
    }
  }

  public class RunModuleDefinition : DockerModuleDefinition {
    public RunModuleDefinition() {
    }

    public RunModuleDefinition(IModule module, YamlMappingNode node) : base(module, node) {
      Image = (string)node.Children["image"];

      if (node.Children.ContainsKey("volumes")) {
        Volumes = new Dictionary<string, EmptyStruct>();
        var volumes = node.Children["volumes"] as YamlMappingNode;
        foreach (var volume in volumes.Children) {
          Volumes[$"{volume.Key}:{volume.Value}"] = default;
        }
      }

      if (node.Children.ContainsKey("labels")) {
        Labels = new Dictionary<string, string>();
        var labels = node.Children["labels"] as YamlMappingNode;
        foreach (var label in labels.Children) {
          Labels[(string)label.Key] = (string)label.Value;
        }
      }

      if (node.Children.ContainsKey("cmd")) {
        Cmd = new List<string>();
        var args = node.Children["cmd"] as YamlSequenceNode;
        foreach (var arg in args) {
          Cmd.Add((string)arg);
        }
      }

      if (node.Children.ContainsKey("entrypoint")) {
        Entrypoint = new List<string>();
        var args = node.Children["entrypoint"] as YamlSequenceNode;
        foreach (var arg in args) {
          Entrypoint.Add((string)arg);
        }
      }

      if (node.Children.ContainsKey("env")) {
        Env = new List<string>();
        var list = node.Children["env"] as YamlMappingNode;
        foreach (var item in list.Children) {
          Env.Add($"{item.Key}={item.Value}");
        }
      }

      if (node.Children.ContainsKey("workdir")) {
        WorkingDir = (string)node.Children["workdir"];
      }

      if (node.Children.ContainsKey("name")) {
        Name = (string)node.Children["name"];
      }

      if (node.Children.ContainsKey("remove")) {
        Remove = bool.Parse((string)node.Children["remove"]);
      }

      if (node.Children.ContainsKey("wait")) {
        Wait = bool.Parse((string)node.Children["wait"]);
      }
    }

    public IDictionary<string, EmptyStruct> Volumes { get; set; }

    public IDictionary<string, string> Labels { get; set; }

    public IList<string> Cmd { get; set; }

    public IList<string> Entrypoint { get; set; }

    public IList<string> Env { get; set; }

    public string Image { get; set; }

    public string Name { get; set; }

    public string WorkingDir { get; set; }

    public bool Remove { get; set; }

    public bool Wait { get; set; }
  }
}