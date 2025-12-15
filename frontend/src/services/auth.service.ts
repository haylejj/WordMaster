import api from "./api";
import type { LoginRequest, RegisterRequest, ForgotPasswordRequest, ResetPasswordRequest, ChangePasswordRequest } from "@/types/auth";
import type { LoginResponse, ServiceResult, ServiceResultWithData } from "@/types/api";

export const authService = {
    login: async (data: LoginRequest) => {
        const response = await api.post<ServiceResultWithData<LoginResponse>>("/auth/login", data);
        return response.data;
    },

    adminLogin: async (data: LoginRequest) => {
        const response = await api.post<ServiceResultWithData<LoginResponse>>("/auth/admin-login", data);
        return response.data;
    },

    register: async (data: RegisterRequest) => {
        const response = await api.post<ServiceResult>("/auth/register", data);
        return response.data;
    },

    forgotPassword: async (data: ForgotPasswordRequest) => {
        const response = await api.post<ServiceResult>("/auth/forget-password", data);
        return response.data;
    },

    resetPassword: async (data: ResetPasswordRequest) => {
        const response = await api.post<ServiceResult>("/auth/reset-password", data);
        return response.data;
    },

    changePassword: async (data: ChangePasswordRequest) => {
        const response = await api.post<ServiceResult>("/auth/change-password", data);
        return response.data;
    },

    logout: async () => {
        const response = await api.post<ServiceResult>("/auth/logout");
        return response.data;
    },

    checkSession: async () => {
        const response = await api.get<ServiceResult>("/auth/session-check");
        return response.data;
    },

    verifyEmail: async (userId: string, token: string) => {
        const response = await api.get<ServiceResult>(`/auth/confirm-email?userId=${encodeURIComponent(userId)}&token=${encodeURIComponent(token)}`);
        return response.data;
    },
};
