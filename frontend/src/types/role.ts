export interface RoleResponse {
    id: string;
    name: string;
}

export interface RoleCreateRequest {
    name: string;
}

export interface RoleUpdateRequest {
    id: string;
    name: string;
}
