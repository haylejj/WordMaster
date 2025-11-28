export interface AllowedIpAddressResponse {
    id: number;
    ipAddress: string;
    description?: string;
    createdAt: string;
    isActive: boolean;
}

export interface AllowedIpAddressCreateRequest {
    ipAddress: string;
    description?: string;
    isActive: boolean;
}

export interface AllowedIpAddressUpdateRequest {
    id: number;
    ipAddress: string;
    description?: string;
    isActive: boolean;
}
