import api from "@/services/api";
import type { LogHistoryListResponse, GetLogHistoryRequest } from "@/types/logHistory";
import type { ServiceResultWithData } from "@/types/api";

const BASE_URL = "/admin/logHistory";

export const logHistoryService = {
    getLogs: async (params: GetLogHistoryRequest): Promise<ServiceResultWithData<LogHistoryListResponse>> => {
        const queryParams = new URLSearchParams();
        queryParams.append("page", params.page.toString());
        queryParams.append("size", params.size.toString());
        if (params.searchTerm) {
            queryParams.append("searchTerm", params.searchTerm);
        }
        queryParams.append("searchInEmail", params.searchInEmail.toString());
        queryParams.append("searchInUserId", params.searchInUserId.toString());
        queryParams.append("searchInIp", params.searchInIp.toString());

        const response = await api.get<ServiceResultWithData<LogHistoryListResponse>>(`${BASE_URL}?${queryParams.toString()}`);
        return response.data;
    }
};
