using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;

namespace Swarm.TaskRunner {
  public class TaskContext {
    private static readonly Regex EnvRegex = new Regex(@"\$\{(\w+)\}");

    public string TaskDefinitionFilePath { get; internal set; }

    public string WorkingDirectory { get; set; }

    public HashSet<int> SkippedSteps { get; set; }

    public Dictionary<string, string> EnvironmentVariables { get; set; }

    /**
    Geting value from EnvironmentVariables, if the string contain ${name}
    */
    public string GetValue(string input) {
      Contract.Requires(input != null);

      Match m;
      while ((m = EnvRegex.Match(input)).Success) {
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
          throw new Exception($"Environment Variable \"{key}\" is not found");
        }

        Contract.Requires(input != null);
        input = input.Replace(m.Value, value);
      }

      return input;
    }
  }
}