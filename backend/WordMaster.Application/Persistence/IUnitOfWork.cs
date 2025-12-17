using Microsoft.EntityFrameworkCore.Storage;

namespace WordMaster.Application.Persistence;

public interface IUnitOfWork : IAsyncDisposable
{
    Task CommitAsync();
    void Commit();
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
    IExecutionStrategy CreateExecutionStrategy();
}
