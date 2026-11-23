namespace Dciex.Core.Configuration
{
    public class ComposeSettings
    {
        public ComposeSource Source { get; set; } = ComposeSource.Generated;
    }
    public enum ComposeSource
    {
        Generated,
        Existing
    }
}
