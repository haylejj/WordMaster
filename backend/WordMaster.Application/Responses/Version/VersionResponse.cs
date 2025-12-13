namespace WordMaster.Application.Responses.Version;

public class VersionResponse
{
    public string? AppName { get; set; }
    public string? AppVersion { get; set; }
    public string? ApiVersion { get; set; }
    public string? Environment { get; set; }
}
