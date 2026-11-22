
namespace Dciex.Application.Configuration
{
    public class ConfigurationLoader
    {
        private readonly IConfigurationReader _reader;
        private readonly ConfigurationValidator _validator;

        public ConfigurationLoader(
            IConfigurationReader reader,
            ConfigurationValidator validator)
        {
            ArgumentNullException.ThrowIfNull(reader);
            ArgumentNullException.ThrowIfNull(validator);

            _reader = reader;
            _validator = validator;
        }

        public async Task<ConfigurationLoadResult> LoadAsync(
            string path,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var configuration = await _reader.LoadAsync(path, cancellationToken);
                var validation = _validator.Validate(configuration);

                if (validation.Count > 0)
                {
                    return new ConfigurationLoadResult
                    {
                        Configuration = null,
                        ValidationErrors = validation
                    };
                }

                return new ConfigurationLoadResult
                {
                    Configuration = configuration,
                    ValidationErrors = validation
                };
            }
            catch (FileNotFoundException)
            {
                return new ConfigurationLoadResult
                {
                    LoadErrors =
                    [
                        new ConfigurationLoadError
                        {
                            Code = ConfigurationLoadErrorCode.FileNotFound,
                            Message = "Configuration file not found."
                        }
                    ]
                };
            }
            catch (UnauthorizedAccessException)
            {
                return new ConfigurationLoadResult
                {
                    LoadErrors =
                    [
                        new ConfigurationLoadError
                        {
                            Code = ConfigurationLoadErrorCode.AccessDenied,
                            Message = "Access to configuration file denied."
                        }
                    ]
                };
            }
            catch (IOException)
            {
                return new ConfigurationLoadResult
                {
                    LoadErrors =
                    [
                        new ConfigurationLoadError
                        {
                            Code = ConfigurationLoadErrorCode.ReadError,
                            Message = "Failed to read configuration file."
                        }
                    ]
                };
            }
            catch (InvalidDataException)
            {
                return new ConfigurationLoadResult
                {
                    LoadErrors =
                    [
                        new ConfigurationLoadError
                        {
                            Code = ConfigurationLoadErrorCode.InvalidConfigurationData,
                            Message = "Configuration file is empty or contains no configuration data."
                        }
                    ]
                };
            }
            catch (ConfigurationReadException)
            {
                return new ConfigurationLoadResult
                {
                    LoadErrors =
                    [
                        new ConfigurationLoadError
                        {
                            Code = ConfigurationLoadErrorCode.InvalidYaml,
                            Message = "Configuration file contains invalid YAML."
                        }
                    ]
                };
            }
        }
    }
}
