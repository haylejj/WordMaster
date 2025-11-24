using System.Collections.Generic;
using WordMaster.Application.Responses;

namespace WordMaster.Application.Responses.AllowedIpAddress;

public class AllowedIpAddressListResponse
{
    public List<AllowedIpAddressResponse> AllowedIpAddresses { get; set; } = [];
}
