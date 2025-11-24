// Common Service Result Wrapper
export interface ServiceResult {
  isSuccess: boolean;
  errorList: string[] | null;
  urlAsCreated: string | null;
}

export interface ServiceResultWithData<T> extends ServiceResult {
  data: T;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiration: string;
}

export interface RefreshTokenResponse {
  accessToken: string;
  refreshToken: string;
}
