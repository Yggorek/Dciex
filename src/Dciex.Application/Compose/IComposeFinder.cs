namespace Dciex.Application.Compose
{
    public interface IComposeFinder
    {
        ComposeDiscoveryResult Find(string path, CancellationToken cancellationToken = default);
    }
}
