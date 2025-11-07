namespace WordMaster.Application.Persistence;

public interface IUnitOfWork
{
    Task CommitAsync();
    void Commit();
}
