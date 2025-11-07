using WordMaster.Application.Persistence;

namespace WordMaster.Infrastructure.EfCore.UnitOfWork;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public void Commit() => context.SaveChanges();
    public async Task CommitAsync() => await context.SaveChangesAsync();
}
