using DevEnv.Core.Projects;


namespace DevEnv.Application.Projects
{
    public class ProjectRegistry : IProjectRegistry
    {
        private readonly IProjectStorage _projectStorage;

        public ProjectRegistry(IProjectStorage projectStorage)
        {
            _projectStorage = projectStorage;
        }

        public Task<IReadOnlyList<Project>> GetAllAsync(
            CancellationToken cancellationToken = default)
        {
            return _projectStorage.LoadAsync(cancellationToken);
        }
        public async Task<Project?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            var projects = await _projectStorage.LoadAsync(cancellationToken);
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            return projects.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }
}
