namespace WordMaster.Domain.Entities;

public class Permission
{
    public Guid Id { get; set; }
    public required string Key { get; set; } // Controller_Action_HTTPMETHOD
    public string? Description { get; set; }
    public required string AreaName { get; set; }
    public required string ControllerName { get; set; }
    public required string ActionName { get; set; }
    public required string HttpMethod { get; set; }
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
