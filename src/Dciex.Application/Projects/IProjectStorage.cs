using Dciex.Core.Projects;


namespace Dciex.Application.Projects
{
    public interface IProjectStorage
    {
        Task<IReadOnlyList<Project>> LoadAsync(
            CancellationToken cancellationToken = default);

        Task SaveAsync(
            IReadOnlyCollection<Project> projects,
            CancellationToken cancellationToken = default);
    }
}
