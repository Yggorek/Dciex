using System;

namespace DevEnv.Core.Configuration;

public class EnvironmentConfiguration
{
  public string? Name { get; set; }
  public List<ServiceConfiguration>? Services { get; set; }
  public ApplicationConfiguration? Application { get; set; }
  public EnvironmentMode? EnvironmentMode { get; set; }
}
public enum  EnvironmentMode
{
    External,
    Managed
}