using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Swarm.TaskRunner {
  public class TaskContext {
    private readonly Regex EnvRegex = new Regex(@"\$\{(\w+)\}");
    public string WorkingDirectory { get; set; }
    public Dictionary<int, bool> SkippedSteps { get; set; }
    public Dictionary<string, string> EnvironmentVariables { get; set; }

    /**
    Geting value from EnvironmentVariables, if the string contain ${name}
     */
    public string GetValue(string input) {
      foreach (Match m in EnvRegex.Matches(input)) {
        string key = m.Groups[1].Value;
        string value;
        switch (key) {
          case "CWD":
            value = WorkingDirectory;
            break;
          default:
            EnvironmentVariables.TryGetValue(key, out value);
            break;
        }

        if (value == null) {
          throw new Exception($"{m.Value} is not found");
        }

        input = EnvRegex.Replace(input, value);
      }

      return input;
    }
  }
}