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
        public async Task<Project> RegisterAsync(string path, CancellationToken cancellationToken = default)
        {
            if(string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path cannot be null or whitespace.", nameof(path));
            }

            if(!System.IO.Directory.Exists(path))
            {
                throw new ArgumentException($"The directory '{path}' does not exist.", nameof(path));
            }
            var normalizedPath = Path.GetFullPath(path);
            var projects = (await _projectStorage.LoadAsync(cancellationToken)).ToList();
            var comparison = OperatingSystem.IsWindows()
                 ? StringComparison.OrdinalIgnoreCase
                 : StringComparison.Ordinal;
            foreach (var project in projects)
            {
                if (string.Equals(project.Path, normalizedPath, comparison))
                {
                    throw new InvalidOperationException($"A project with the path '{normalizedPath}' is already registered.");
                }
            }
            int newId;
            if (projects.Count == 0)
               newId = 1;
            
            else
               newId = projects.Max(p => p.Id) + 1;
            
            var newProject = new Project
            {
               Id = newId,
               Path = normalizedPath,
               Name = Path.GetFileName(normalizedPath),
               CreatedAt = DateTimeOffset.UtcNow
           };
           
            projects.Add(newProject);
            await _projectStorage.SaveAsync(projects, cancellationToken);
            return newProject;
            
        }
    }
}
