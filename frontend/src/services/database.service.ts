import api from "./api";

export const databaseService = {
    resetTable: async (tableName: string, password: string, targetUserId?: string) => {
        const response = await api.post("/admin/database/reset", {
            tableName,
            password,
            targetUserId: targetUserId || null,
        });
        return response.data;
    },
};
