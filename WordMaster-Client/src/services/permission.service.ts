import api from "./api";
import type { ServiceResult, ServiceResultWithData } from "@/types/api";
import type { PermissionResponse, UpdateRolePermissionsRequest, PermissionScanResponse } from "@/types/permission";

export const permissionService = {
    getAll: async (): Promise<ServiceResultWithData<PermissionResponse[]>> => {
        const response = await api.get("/admin/permissions");
        return response.data;
    },
    getByRole: async (roleId: string): Promise<ServiceResultWithData<PermissionResponse[]>> => {
        const response = await api.get(`/admin/permissions/role/${roleId}`);
        return response.data;
    },
    updateRolePermissions: async (data: UpdateRolePermissionsRequest): Promise<ServiceResult> => {
        const response = await api.put("/admin/permissions/role", data);
        return response.data;
    },
    scan: async (): Promise<ServiceResultWithData<PermissionScanResponse>> => {
        const response = await api.post("/admin/permissions/scan");
        return response.data;
    }
};
