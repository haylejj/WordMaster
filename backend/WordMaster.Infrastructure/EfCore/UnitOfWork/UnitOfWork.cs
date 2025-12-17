using Microsoft.EntityFrameworkCore.Storage;
using WordMaster.Application.Persistence;

namespace WordMaster.Infrastructure.EfCore.UnitOfWork;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    private IDbContextTransaction? _transaction;

    public void Commit()
    {
        context.SaveChanges();
    }

    public async Task CommitAsync()
    {
        await context.SaveChangesAsync();
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        _transaction = await context.Database.BeginTransactionAsync();
        return _transaction;
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public IExecutionStrategy CreateExecutionStrategy()
    {
        return context.Database.CreateExecutionStrategy();
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
        }
    }
}
