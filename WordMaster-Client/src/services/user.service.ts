import api from "./api";
import type { ServiceResult, ServiceResultWithData } from "@/types/api";

export interface UserProfile {
    userName: string | null;
    email: string | null;
    phone: string | null;
    birthDate: string | null;
    gender: number | null;
}

export interface UpdateProfileRequest {
    userName: string | null;
    email: string | null;
    phone: string | null;
    birthDate: string | null;
    gender: number | null;
}

export const userService = {
    getProfile: async () => {
        const response = await api.get<ServiceResultWithData<UserProfile>>("/user/profile");
        return response.data;
    },

    updateProfile: async (data: UpdateProfileRequest) => {
        const response = await api.put<ServiceResult>("/user/profile", data);
        return response.data;
    },
};
