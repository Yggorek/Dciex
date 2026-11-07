using System;

namespace DevEnv.Application.Configuration;

public class ConfigurationValidationError
{
  public required string PropertyName { get; init; }
  public required string ErrorMessage { get; init; }
}
