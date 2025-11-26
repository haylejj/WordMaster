import api from "./api";
import type { ServiceResult, ServiceResultWithData, PagedResult } from "@/types/api";
import type {
  WordResponse,
  CreateWordRequest,
  UpdateWordRequest,
  FavoriteWithWordResponse,
  UnknowsWithWordResponse,
  PracticeWordResponse,
} from "@/types/word";

const buildQueryString = (search: string, page: number, pageSize: number) => {
  const params = new URLSearchParams();
  if (search) params.append("search", search);
  params.append("page", page.toString());
  params.append("pageSize", pageSize.toString());
  return params.toString();
};

const normalizeWordFromWrapper = (
  paged: PagedResult<FavoriteWithWordResponse | UnknowsWithWordResponse>,
  relation: "favorite" | "unknown"
): PagedResult<WordResponse> => {
  return {
    ...paged,
    items: paged.items
      .map((item) => {
        if (!item.word) return null;
        return {
          id: Number(item.word.id ?? item.wordId),
          englishWord: item.word.englishWord ?? "",
          turkishWord: item.word.turkishWord ?? "",
          favoriteId: relation === "favorite" ? Number(item.id) : null,
          unknowsId: relation === "unknown" ? Number(item.id) : null,
        } as WordResponse;
      })
      .filter(Boolean) as WordResponse[],
  };
};

export const wordService = {
  getWords: async (search = "", page = 1, pageSize = 10) => {
    const query = buildQueryString(search, page, pageSize);
    const response = await api.get<ServiceResultWithData<PagedResult<WordResponse>>>(`/words?${query}`);
    return response.data;
  },

  getFavorites: async (search = "", page = 1, pageSize = 10) => {
    const query = buildQueryString(search, page, pageSize);
    const response = await api.get<ServiceResultWithData<PagedResult<FavoriteWithWordResponse>>>(`/favorites?${query}`);
    const normalized = response.data.data
      ? normalizeWordFromWrapper(response.data.data, "favorite")
      : {
        items: [],
        pageNumber: page,
        pageSize,
        totalCount: 0,
        totalPages: 0,
      };
    return {
      ...response.data,
      data: normalized,
    } as ServiceResultWithData<PagedResult<WordResponse>>;
  },

  getUnknowns: async (search = "", page = 1, pageSize = 10) => {
    const query = buildQueryString(search, page, pageSize);
    const response = await api.get<ServiceResultWithData<PagedResult<UnknowsWithWordResponse>>>(`/unknows?${query}`);
    const normalized = response.data.data
      ? normalizeWordFromWrapper(response.data.data, "unknown")
      : {
        items: [],
        pageNumber: page,
        pageSize,
        totalCount: 0,
        totalPages: 0,
      };
    return {
      ...response.data,
      data: normalized,
    } as ServiceResultWithData<PagedResult<WordResponse>>;
  },

  addWord: async (data: CreateWordRequest) => {
    const response = await api.post<ServiceResult>("/words", data);
    return response.data;
  },

  updateWord: async (data: UpdateWordRequest) => {
    const response = await api.put<ServiceResultWithData<WordResponse>>("/words", data);
    return response.data;
  },

  deleteWord: async (id: number) => {
    const response = await api.delete<ServiceResult>(`/words/${id}`);
    return response.data;
  },

  toggleFavorite: async (wordId: number) => {
    const response = await api.post<ServiceResultWithData<boolean>>("/favorites/toggle", { wordId });
    return response.data;
  },

  toggleUnknown: async (wordId: number) => {
    const response = await api.post<ServiceResultWithData<boolean>>("/unknows/toggle", { wordId });
    return response.data;
  },

  getUserWords: async () => {
    const response = await api.get<ServiceResultWithData<WordResponse[]>>("/words/user-words");
    return response.data;
  },

  importCsv: async (file: File) => {
    const formData = new FormData();
    formData.append("file", file);
    const response = await api.post<ServiceResult>("/words/import-csv", formData, {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    });
    return response.data;
  },

  getPracticeRandomWord: async () => {
    const response = await api.get<ServiceResultWithData<PracticeWordResponse>>("/words/practice/random");
    return response.data;
  },

  checkPracticeTranslation: async (wordId: number, answer: string) => {
    const response = await api.post<ServiceResultWithData<boolean>>("/words/practice/check", { wordId, answer });
    return response.data;
  },

  getPracticeRandomFavorite: async () => {
    const response = await api.get<ServiceResultWithData<PracticeWordResponse>>("/favorites/practice/random");
    return response.data;
  },

  checkPracticeFavorite: async (wordId: number, answer: string) => {
    const response = await api.post<ServiceResultWithData<boolean>>("/favorites/practice/check", { wordId, answer });
    return response.data;
  },

  getPracticeRandomUnknown: async () => {
    const response = await api.get<ServiceResultWithData<PracticeWordResponse>>("/unknows/practice/random");
    return response.data;
  },

  checkPracticeUnknown: async (wordId: number, answer: string) => {
    const response = await api.post<ServiceResultWithData<boolean>>("/unknows/practice/check", { wordId, answer });
    return response.data;
  },

  bulkUpdateStats: async (results: { wordId: number; isCorrect: boolean }[]) => {
    const response = await api.post<ServiceResultWithData<boolean>>("/words/practice/batch-update", { results });
    return response.data;
  },
};
