using System;
using System.Collections.Generic;
using WordMaster.Application.Responses;

namespace WordMaster.Application.Responses.Unknows;

public class UnknowsListResponse
{
    public List<UnknowsWithWordResponse> Unknows { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public string? Search { get; set; }

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
}
