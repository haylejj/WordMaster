import api from "./api";
import type { ServiceResult, ServiceResultWithData } from "@/types/api";
import type { FolderResponse, FolderWordRequest } from "@/types/folder";
import type { WordResponse } from "@/types/word";

export const folderService = {
    getFolders: async () => {
        const response = await api.get<ServiceResultWithData<FolderResponse[]>>("/folders");
        return response.data;
    },

    getFolder: async (id: number) => {
        const response = await api.get<ServiceResultWithData<FolderResponse>>(`/folders/${id}`);
        return response.data;
    },

    createFolder: async (name: string) => {
        const response = await api.post<ServiceResultWithData<FolderResponse>>("/folders", { name });
        return response.data;
    },

    updateFolder: async (id: number, name: string) => {
        const response = await api.put<ServiceResultWithData<FolderResponse>>("/folders", { id, name });
        return response.data;
    },

    deleteFolder: async (id: number) => {
        const response = await api.delete<ServiceResult>(`/folders/${id}`);
        return response.data;
    },

    getWords: async (folderId: number) => {
        const response = await api.get<ServiceResultWithData<WordResponse[]>>(`/folders/${folderId}/words`);
        return response.data;
    },

    addWordToFolder: async (payload: FolderWordRequest) => {
        const response = await api.post<ServiceResult>("/folders/words", payload);
        return response.data;
    },

    removeWordFromFolder: async (payload: FolderWordRequest) => {
        const response = await api.delete<ServiceResult>("/folders/words", { data: payload });
        return response.data;
    },

    checkPracticeTranslation: async (wordId: number, answer: string) => {
        const response = await api.post<ServiceResultWithData<boolean>>("/folders/practice/check", { wordId, answer });
        return response.data;
    },
};

