using System;

namespace Swarm.TaskRunner.Logging {
  public abstract class LogProvider {
    public bool IsColorEnabled { get; set; }
    public bool IsVerbose { get; set; }

    public abstract void LogInfo(string message);

    public abstract void LogHint(string message);

    public abstract void LogWarning(string message);

    public abstract void LogError(string message, Exception error = null);
  }
}
