// TODO: Enforce consistency between Status and Paths.
// Found must contain exactly one path,
// NotFound and DirectoryNotFound must contain zero,
// Ambiguous must contain at least two.

namespace Dciex.Application.Compose
{
    public class ComposeDiscoveryResult
    {
        public ComposeDiscoveryStatus Status { get; init; }

        public IReadOnlyList<string> Paths { get; init; } = [];
        public string? SinglePath => (Status, Paths) switch
        {
            (ComposeDiscoveryStatus.Found, [var path]) => path,
            _ => null
        };
    }
    public enum ComposeDiscoveryStatus
    {
        Found,
        NotFound,
        Ambiguous,
        DirectoryNotFound
    }
}
