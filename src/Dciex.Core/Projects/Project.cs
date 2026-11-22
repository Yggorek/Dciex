namespace Dciex.Core.Projects
{
    public class Project
    {
        public int Id { get; init; }
        public required string Name { get; init; }
        public required string Path { get; init; }
        public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    }
}
