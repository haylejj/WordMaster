import api from "./api";
import type { ServiceResult, ServiceResultWithData } from "@/types/api";
import type { AllowedIpAddressResponse, AllowedIpAddressCreateRequest, AllowedIpAddressUpdateRequest } from "@/types/ip";

export const ipService = {
    getAll: async (): Promise<ServiceResultWithData<AllowedIpAddressResponse[]>> => {
        const response = await api.get("/admin/allowed-ips");
        return response.data;
    },
    create: async (data: AllowedIpAddressCreateRequest): Promise<ServiceResult> => {
        const response = await api.post("/admin/allowed-ips", data);
        return response.data;
    },
    update: async (data: AllowedIpAddressUpdateRequest): Promise<ServiceResult> => {
        const response = await api.put("/admin/allowed-ips", data);
        return response.data;
    },
    delete: async (id: number): Promise<ServiceResult> => {
        const response = await api.delete(`/admin/allowed-ips/${id}`);
        return response.data;
    }
};
