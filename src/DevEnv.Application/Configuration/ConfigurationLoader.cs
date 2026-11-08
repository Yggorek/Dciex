namespace DevEnv.Application.Configuration
{
    public class ConfigurationLoader
    {
        private readonly IConfigurationReader _reader;
        private readonly ConfigurationValidator _validator;
        public ConfigurationLoader(IConfigurationReader reader, ConfigurationValidator validator)
        {
            ArgumentNullException.ThrowIfNull(reader);
            ArgumentNullException.ThrowIfNull(validator);

            _reader = reader;
            _validator = validator;
        }
        public async Task<ConfigurationLoadResult> LoadAsync(string path, CancellationToken cancellationToken = default)
        {
            var conf = await _reader.LoadAsync(path, cancellationToken);
            var validation = _validator.Validate(conf);
            if(validation.Count > 0)
            {
                return new ConfigurationLoadResult
                {
                    Configuration = null,
                    ValidationErrors = validation
                };
            }
            return new ConfigurationLoadResult
            {
                Configuration = conf,
                ValidationErrors = validation
            };
        }
    }
}
