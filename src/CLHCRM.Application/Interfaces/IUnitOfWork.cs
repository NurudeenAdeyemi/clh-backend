namespace CLHCRM.Application.Interfaces;

/// <summary>
/// Unit of Work pattern for managing transactions across multiple repositories
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // Repositories will be added here as features are implemented
    // Example: IStudentRepository Students { get; }
    // Example: ICourseRepository Courses { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
