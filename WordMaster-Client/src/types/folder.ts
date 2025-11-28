import { z } from "zod";
import type { WordResponse } from "./word";

export interface FolderResponse {
    id: number;
    name: string;
    createdTime: string;
    wordCount: number;
}

export interface FolderWordResponse {
    id: number;
    englishWord: string;
    turkishWord: string;
}

export interface FolderDetailResponse {
    folderId: number;
    folderName: string;
    words: WordResponse[];
    allWords: WordResponse[];
}

export const folderNameSchema = z.object({
    name: z.string().min(1, "Klasör adı boş olamaz").max(50, "Klasör adı en fazla 50 karakter olabilir"),
});
export type FolderNameForm = z.infer<typeof folderNameSchema>;

export interface FolderWordRequest {
    folderId: number;
    wordId: number;
}

