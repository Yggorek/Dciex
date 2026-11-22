namespace Dciex.Application.Configuration;

public class ConfigurationReadException : Exception
{
    public ConfigurationReadException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
