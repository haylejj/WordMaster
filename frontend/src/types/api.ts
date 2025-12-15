// Common Service Result Wrapper
export interface ServiceResult {
  isSuccess: boolean;
  errorList: string[] | null;
  urlAsCreated: string | null;
}

export interface ServiceResultWithData<T> extends ServiceResult {
  data: T;
}

export interface PagedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

/**
 * Login işlemi sonucu dönen response.
 * NOT: RefreshToken artık HttpOnly cookie ile gönderiliyor, body'de yer almaz.
 */
export interface LoginResponse {
  accessToken: string;
  expiresAt: string;
}

/**
 * Token yenileme işlemi sonucu dönen response.
 * NOT: Yeni RefreshToken HttpOnly cookie ile gönderiliyor, body'de yer almaz.
 */
export interface RefreshTokenResponse {
  accessToken: string;
  expiresAt: string;
}
