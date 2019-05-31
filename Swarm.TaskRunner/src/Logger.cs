using System;

namespace Swarm.TaskRunner {
  public static class Logger {
    public static bool IsColorEnabled { get; set; }
    public static bool IsVerbose { get; set; }

    public static void LogInfo(string message) {
      Console.WriteLine(message);
    }

    public static void LogHint(string message) {
      if (IsColorEnabled) {
        Console.ForegroundColor = ConsoleColor.DarkBlue;
        Console.WriteLine(message);
        Console.ResetColor();
      } else {
        Console.WriteLine(message);
      }
    }

    public static void LogWarning(string message) {
      if (IsColorEnabled) {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine(message);
        Console.ResetColor();
      } else {
        Console.WriteLine(message);
      }
    }

    public static void LogError(string message, Exception error = null) {
      string errorMessage;
      if (error == null) {
        errorMessage = "";
      } else {
        errorMessage = error.Message;
        if (error.InnerException != null) {
          errorMessage += ", " + error.InnerException.Message;
        }
      }

      if (IsColorEnabled) {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.Error.WriteLine(message, errorMessage);

        if (IsVerbose && error != null) {
          Console.Error.WriteLine(error.ToString());
        }

        Console.ResetColor();
      } else {
        Console.Error.WriteLine(message, errorMessage);
      }
    }
  }
}