import api from "./api";
import type { ServiceResult, ServiceResultWithData } from "@/types/api";
import type { RoleResponse, RoleCreateRequest, RoleUpdateRequest } from "@/types/role";

export const roleService = {
    getRoles: async (): Promise<ServiceResultWithData<RoleResponse[]>> => {
        const response = await api.get("/admin/roles");
        return response.data;
    },
    createRole: async (data: RoleCreateRequest): Promise<ServiceResult> => {
        const response = await api.post("/admin/roles", data);
        return response.data;
    },
    updateRole: async (data: RoleUpdateRequest): Promise<ServiceResult> => {
        const response = await api.put("/admin/roles", data);
        return response.data;
    },
    deleteRole: async (id: string): Promise<ServiceResult> => {
        const response = await api.delete(`/admin/roles/${id}`);
        return response.data;
    }
};
