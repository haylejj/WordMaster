import { z } from "zod";

export interface WordResponse {
  id: number;
  englishWord: string;
  turkishWord: string;
  favoriteId: number | null;
  unknowsId: number | null;
}

export interface PracticeWordResponse {
  id: number;
  englishWord: string;
  turkishWord: string;
}

export interface FavoriteWithWordResponse {
  id: number;
  createdTime: string;
  wordId: number;
  userId: string | null;
  word: WordResponse | null;
}

export interface UnknowsWithWordResponse {
  id: number;
  createdTime: string;
  wordId: number;
  userId: string | null;
  word: WordResponse | null;
}

export const createWordSchema = z.object({
  englishWord: z.string().min(1, "İngilizce kelime boş olamaz"),
  turkishWord: z.string().min(1, "Türkçe kelime boş olamaz"),
});

export type CreateWordRequest = z.infer<typeof createWordSchema>;

export const updateWordSchema = createWordSchema.extend({
  id: z.number(),
});

export type UpdateWordRequest = z.infer<typeof updateWordSchema>;

export interface ToggleFavoriteRequest {
  wordId: number;
}

export interface ToggleUnknowsRequest {
  wordId: number;
}

export interface CheckTranslationRequest {
  wordId: number;
  userTranslation: string;
  isFromFavorite?: boolean;
  isFromUnknows?: boolean;
}

