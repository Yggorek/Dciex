using DevEnv.Core.Projects;


namespace DevEnv.Application.Projects
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
