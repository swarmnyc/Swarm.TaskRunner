using System;

namespace Swarm.TaskRunner.Logging {
  public class ConsoleLogger : LogProvider {
    public override void LogInfo(string message) {
      Console.WriteLine(message);
    }

    public override void LogHint(string message) {
      if (IsColorEnabled) {
        Console.ForegroundColor = ConsoleColor.DarkBlue;
        Console.WriteLine(message);
        Console.ResetColor();
      } else {
        Console.WriteLine(message);
      }
    }

    public override void LogWarning(string message) {
      if (IsColorEnabled) {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine(message);
        Console.ResetColor();
      } else {
        Console.WriteLine(message);
      }
    }

    public override void LogError(string message, Exception error = null) {
      string errorMessage;
      if (error == null) {
        errorMessage = string.Empty;
      } else {
        errorMessage = error.Message;
        if (error.InnerException != null) {
          errorMessage += ", " + error.InnerException.Message;
        }
      }

      if (IsColorEnabled) {
        Console.ForegroundColor = ConsoleColor.DarkRed;
      }

      Console.Error.WriteLine(message, errorMessage);

      if (IsVerbose && error != null) {
        Console.Error.WriteLine(error.ToString());
      }

      if (IsColorEnabled) {
        Console.ResetColor();
      }
    }
  }
}
