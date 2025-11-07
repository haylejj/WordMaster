using Core.UnitOfWork;

namespace Repository.UnitOfWork;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public void Commit() => context.SaveChanges();
    public async Task CommitAsync() => await context.SaveChangesAsync();
}
