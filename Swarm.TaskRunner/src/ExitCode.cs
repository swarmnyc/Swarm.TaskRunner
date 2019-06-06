namespace Swarm.TaskRunner {
  public enum ExitCode : int {
    Success = 0,
    CliFail = 1,
    DefinitionFail = 2,
    ExecuteFail = 3
  }
}