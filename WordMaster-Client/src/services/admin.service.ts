import api from "./api";
import type { ServiceResultWithData } from "@/types/api";
import type { AdminDashboardResponse } from "@/types/admin";

export const adminService = {
    getDashboard: async () => {
        const response = await api.get<ServiceResultWithData<AdminDashboardResponse>>("/admin/dashboard");
        return response.data;
    },
};
