import api from "./api";
import type { ServiceResult, ServiceResultWithData, PagedResult } from "@/types/api";

export interface AdminWordResponse {
    id: number;
    englishWord: string;
    turkishWord: string;
    userId?: string;
    userName?: string;
    createdTime: string;
}

export interface UpdateWordRequest {
    englishWord: string;
    turkishWord: string;
}

export const adminWordService = {
    getPagedWords: async (page: number = 1, pageSize: number = 10, search?: string): Promise<ServiceResultWithData<PagedResult<AdminWordResponse>>> => {
        const params = new URLSearchParams();
        params.append("page", page.toString());
        params.append("pageSize", pageSize.toString());
        if (search) params.append("search", search);

        const response = await api.get(`/admin/words?${params.toString()}`);
        return response.data;
    },

    updateWord: async (id: number, data: UpdateWordRequest): Promise<ServiceResultWithData<AdminWordResponse>> => {
        const response = await api.put(`/admin/words/${id}`, data);
        return response.data;
    },

    deleteWord: async (id: number): Promise<ServiceResult> => {
        const response = await api.delete(`/admin/words/${id}`);
        return response.data;
    }
};
