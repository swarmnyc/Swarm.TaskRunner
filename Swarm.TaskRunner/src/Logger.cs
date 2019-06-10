using System;
using Swarm.TaskRunner.Logging;

namespace Swarm.TaskRunner {
  public static class Logger {
    static Logger() {
      LogProvider = DefaultLogger;
    }

    public static LogProvider LogProvider { get; set; }

    public static bool IsColorEnabled {
      get {
        return LogProvider.IsColorEnabled;
      }

      set {
        LogProvider.IsColorEnabled = value;
      }
    }

    public static bool IsVerbose {
      get {
        return LogProvider.IsVerbose;
      }

      set {
        LogProvider.IsVerbose = value;
      }
    }

    private static LogProvider DefaultLogger { get; set; } = new ConsoleLogger();

    public static void LogInfo(string message) {
      LogProvider.LogInfo(message);
    }

    public static void LogHint(string message) {
      LogProvider.LogHint(message);
    }

    public static void LogWarning(string message) {
      LogProvider.LogWarning(message);
    }

    public static void LogError(string message, Exception error = null) {
      LogProvider.LogError(message, error);
    }

    public static void ResetProvider() {
      LogProvider = DefaultLogger;
    }
  }
}