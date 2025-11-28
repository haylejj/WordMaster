namespace WordMaster.Application.Responses.Permission;

public class PermissionResponse
{
    public Guid Id { get; set; }
    public required string Key { get; set; }
    public required string Description { get; set; }
    public required string AreaName { get; set; }
    public required string ControllerName { get; set; }
    public required string ActionName { get; set; }
    public required string HttpMethod { get; set; }
}
