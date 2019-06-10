using System;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using Docker.DotNet;
using YamlDotNet.RepresentationModel;

namespace Swarm.TaskRunner.Modules.Dockers {
  public abstract class DockerModule<T> : Module<T> where T : DockerModuleDefinition {
    protected DockerClient CreateClient(T definition) {
      Contract.Requires(definition != null);

      var uri = new Uri(definition.ApiEndpoint);

      using (var config = new DockerClientConfiguration(uri)) {
        return config.CreateClient();
      }
    }
  }

  public abstract class DockerModuleDefinition : ModuleDefinition {
    public DockerModuleDefinition() {
      SetDefaultUrl();
    }

    public DockerModuleDefinition(IModule module, YamlMappingNode node) : base(module, node) {
      Contract.Requires(module != null);
      Contract.Requires(node != null);

      if (node.Children.ContainsKey("apiEndpoint")) {
        ApiEndpoint = (string)node.Children["apiEndpoint"];
      } else {
        SetDefaultUrl();
      }
    }

    /// <summary>the endpoint of the docker API, the default value is "/var/run/docker.sock" on unix-like and "npipe://./pipe/docker_engine" on window</summary>
    public string ApiEndpoint { get; set; }

    // public string CredentialUsername { get; set; }

    // public string CredentialPassword { get; set; }

    // public string CredentialCertificateFilepath { get; set; }

    // public string CredentialCertificatePassword { get; set; }

    private void SetDefaultUrl() {
      var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
      ApiEndpoint = isWindows ? "npipe://./pipe/docker_engine" : "unix:/var/run/docker.sock";
    }
  }
}