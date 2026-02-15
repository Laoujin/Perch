namespace Perch.Core.Deploy;

public sealed record DeployResult(string ModuleName, string SourcePath, string TargetPath, ResultLevel Level, string Message);
