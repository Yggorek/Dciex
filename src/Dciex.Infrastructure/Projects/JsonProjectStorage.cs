using Dciex.Core.Projects;
using Dciex.Application.Projects;
using System.Text.Json;

namespace Dciex.Infrastructure.Projects
{
    public class JsonProjectStorage : IProjectStorage
    {
        private readonly string _filePath;
        public JsonProjectStorage(string filePath)
        {
            _filePath = filePath;
        }
        public async Task<IReadOnlyList<Project>> LoadAsync(CancellationToken cancellationToken = default)
        {
            if(!File.Exists(_filePath))
               return Array.Empty<Project>();
            string json = await File.ReadAllTextAsync(_filePath, cancellationToken);
            var projects = JsonSerializer.Deserialize<List<Project>>(json);
            if (projects == null)
                throw new InvalidDataException("Failed to deserialize projects from JSON.");
            return projects;
        }
        public async Task SaveAsync(IReadOnlyCollection<Project> projects, CancellationToken cancellationToken = default)
        {
            string? path = Path.GetDirectoryName(_filePath);
            if (path is not null)
               Directory.CreateDirectory(path);
            await using FileStream file = File.Create(_filePath);
            await JsonSerializer.SerializeAsync(file, projects, cancellationToken: cancellationToken);
            
        }

    }
}
