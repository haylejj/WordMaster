using WordMaster.Application.Responses.Statistics;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IStatisticsService
{
    Task<ServiceResult<DashboardStatisticsResponse>> GetDashboardStatisticsAsync(Guid userId);
}
