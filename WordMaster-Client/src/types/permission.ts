export interface PermissionResponse {
    id: string;
    key: string;
    description: string;
    areaName: string;
    controllerName: string;
    actionName: string;
    httpMethod: string;
}

export interface UpdateRolePermissionsRequest {
    roleId: string;
    permissionIds: string[];
}

export interface PermissionScanResponse {
    addedCount: number;
    deletedCount: number;
}
