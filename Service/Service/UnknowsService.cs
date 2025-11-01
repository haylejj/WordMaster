using Core.Entity;
using Core.Repositories;
using Core.Service;
using Core.UnitOfWorks;
using System.Linq.Expressions;

namespace Service.Services
{
    public class UnknowsService : IUnknowsService
    {
        private readonly IGenericRepository<Unknows> _repository;
        private readonly IUnknowsRepository _unknowsRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UnknowsService(IGenericRepository<Unknows> repository, IUnitOfWork unitOfWork, IUnknowsRepository unknowsRepository)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _unknowsRepository = unknowsRepository;
        }

        public IQueryable<Unknows> Where(Expression<Func<Unknows, bool>> predicate)
        {
            return _repository.Where(predicate);
        }

        public async Task<Unknows?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<bool> AnyAsync(Expression<Func<Unknows, bool>> predicate)
        {
            return await _repository.AnyAsync(predicate);
        }

        public async Task AddAsync(Unknows entity)
        {
            await _repository.AddAsync(entity);
            await _unitOfWork.CommitAsync();
        }

        public async Task UpdateAsync(Unknows entity)
        {
            _repository.Update(entity);
            await _unitOfWork.CommitAsync();
        }

        public async Task RemoveAsync(Unknows entity)
        {
            _repository.Remove(entity);
            await _unitOfWork.CommitAsync();
        }

        public async Task<Unknows> GetLastUnknows()
        {
            return await _unknowsRepository.GetLastUnknows();
        }
    }
}
