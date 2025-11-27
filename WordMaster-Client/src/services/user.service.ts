import api from "./api";
import type { ServiceResult, ServiceResultWithData, PagedResult } from "@/types/api";
import type { UserWithRolesResponse, UserDetailResponse, UserUpdateRequest, UserProfileResponse, UpdateProfileRequest } from "@/types/user";

export const userService = {
    getPagedUsers: async (page: number = 1, pageSize: number = 10, search?: string): Promise<ServiceResultWithData<PagedResult<UserWithRolesResponse>>> => {
        const params = new URLSearchParams();
        params.append("page", page.toString());
        params.append("pageSize", pageSize.toString());
        if (search) params.append("search", search);

        const response = await api.get(`/admin/users?${params.toString()}`);
        return response.data;
    },

    getUserDetail: async (id: string): Promise<ServiceResultWithData<UserDetailResponse>> => {
        const response = await api.get(`/admin/users/${id}/detail`);
        return response.data;
    },

    updateUser: async (data: UserUpdateRequest): Promise<ServiceResult> => {
        const response = await api.put("/admin/users", data);
        return response.data;
    },

    deleteUser: async (id: string): Promise<ServiceResult> => {
        const response = await api.delete(`/admin/users/${id}`);
        return response.data;
    },

    resetPassword: async (id: string): Promise<ServiceResultWithData<string>> => {
        const response = await api.post(`/admin/users/${id}/reset-password`);
        return response.data;
    },

    getProfile: async (): Promise<ServiceResultWithData<UserProfileResponse>> => {
        const response = await api.get("/user/profile");
        return response.data;
    },

    updateProfile: async (data: UpdateProfileRequest): Promise<ServiceResult> => {
        const response = await api.put("/user/profile", data);
        return response.data;
    }
};
