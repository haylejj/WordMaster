import api from "./api";
import type { LoginRequest, RegisterRequest, ForgotPasswordRequest } from "@/types/auth";
import type { LoginResponse, ServiceResult, ServiceResultWithData } from "@/types/api";

export const authService = {
    login: async (data: LoginRequest) => {
        const response = await api.post<ServiceResultWithData<LoginResponse>>("/auth/login", data);
        return response.data;
    },

    register: async (data: RegisterRequest) => {
        // Convert gender string to number for API
        const payload = {
            ...data,
            gender: parseInt(data.gender),
        };
        const response = await api.post<ServiceResult>("/auth/register", payload);
        return response.data;
    },

    forgotPassword: async (data: ForgotPasswordRequest) => {
        const response = await api.post<ServiceResult>("/auth/forget-password", data);
        return response.data;
    },

    logout: async () => {
        const response = await api.post<ServiceResult>("/auth/logout");
        return response.data;
    },
};
