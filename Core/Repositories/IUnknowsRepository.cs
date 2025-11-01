using Core.Entity;

namespace Core.Repositories;

public interface IUnknowsRepository : IGenericRepository<Unknows>
{
    Task<Unknows> GetLastUnknows();
}
