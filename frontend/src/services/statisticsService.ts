import api from "./api";
import type { ServiceResultWithData } from "../types/api";
import type { DashboardStatisticsResponse } from "../types/statistics";

export const statisticsService = {
    getDashboardStatistics: async (): Promise<ServiceResultWithData<DashboardStatisticsResponse>> => {
        const response = await api.get<ServiceResultWithData<DashboardStatisticsResponse>>("/statistics/dashboard");
        return response.data;
    },
};
