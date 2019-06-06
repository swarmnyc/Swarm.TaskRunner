using System;
using System.Text;

namespace Swarm.TaskRunner.Logging {
  public class StringBuilderLogger : LogProvider {
    public StringBuilder Out { get; set; } = new StringBuilder();
    public StringBuilder Error { get; set; } = new StringBuilder();

    public override void LogInfo(string message) {
      Out.AppendLine(message);
    }

    public override void LogHint(string message) {
      Out.AppendLine(message);
    }

    public override void LogWarning(string message) {
      Out.AppendLine(message);
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
        errorMessage = "";
      } else {
        errorMessage = error.Message;
        if (error.InnerException != null) {
          errorMessage += ", " + error.InnerException.Message;
        }
      }

      Error.AppendFormat(message + "\n", errorMessage);

      if (IsVerbose && error != null) {
        Error.AppendLine(error.ToString());
      }
    }


  }
}
