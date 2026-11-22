using System;

namespace Dciex.Application.Configuration;

public class ConfigurationValidationError
{
  public required string Code { get; init; }
  public required string PropertyName { get; init; }
  public required string ErrorMessage { get; init; }
}
