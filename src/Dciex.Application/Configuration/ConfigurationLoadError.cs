public class ConfigurationLoadError
{
    public ConfigurationLoadErrorCode Code {get; init;}
    public required string Message{get; init;}
}
public enum ConfigurationLoadErrorCode
{
    FileNotFound,
    AccessDenied,
    ReadError,
    InvalidConfigurationData,
    InvalidYaml
}
