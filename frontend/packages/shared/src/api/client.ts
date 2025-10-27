import { Tokens } from '../types';

type HttpMethod = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE';

type MaybePromise<T> = T | Promise<T>;

export interface CreateApiClientOptions {
  baseURL: string;
  getAccessToken: () => MaybePromise<string | null | undefined>;
  getRefreshToken: () => MaybePromise<string | null | undefined>;
  setTokens: (accessToken: string, refreshToken: string) => MaybePromise<void>;
  clearTokens: () => MaybePromise<void>;
  onUnauthorized?: () => void;
  refreshEndpoint?: string;
}

export interface ApiClientRequestOptions {
  headers?: Record<string, string>;
}

export interface ApiClient {
  request<T>(method: HttpMethod, path: string, body?: unknown, options?: ApiClientRequestOptions): Promise<T>;
  get<T>(path: string, options?: ApiClientRequestOptions): Promise<T>;
  post<T>(path: string, body?: unknown, options?: ApiClientRequestOptions): Promise<T>;
  put<T>(path: string, body?: unknown, options?: ApiClientRequestOptions): Promise<T>;
  patch<T>(path: string, body?: unknown, options?: ApiClientRequestOptions): Promise<T>;
  delete<T>(path: string, options?: ApiClientRequestOptions): Promise<T>;
}

export class ApiError<T = any> extends Error {
  public readonly status: number;
  public readonly data?: T;

  constructor(message: string, status: number, data?: T) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.data = data;
  }
}

export function createApiClient(options: CreateApiClientOptions): ApiClient {
  const refreshEndpoint = options.refreshEndpoint ?? '/auth/refresh';
  let refreshPromise: Promise<void> | null = null;

  const buildURL = (path: string) => {
    if (path.startsWith('http://') || path.startsWith('https://')) {
      return path;
    }

    if (path.startsWith('/')) {
      return `${options.baseURL}${path}`;
    }

    return `${options.baseURL.replace(/\/$/, '')}/${path}`;
  };

  const refreshTokens = async () => {
    if (!refreshPromise) {
      refreshPromise = (async () => {
        const refreshToken = await Promise.resolve(options.getRefreshToken());
        if (!refreshToken) {
          await Promise.resolve(options.clearTokens());
          options.onUnauthorized?.();
          throw new ApiError('Refresh token not available', 401);
        }

        const response = await fetch(buildURL(refreshEndpoint), {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({ refreshToken }),
        });

        if (!response.ok) {
          await Promise.resolve(options.clearTokens());
          options.onUnauthorized?.();
          let errorMessage = 'Unable to refresh session';
          let errorData: unknown;

          try {
            errorData = await response.json();
            errorMessage = (errorData as any)?.message ?? errorMessage;
          } catch (error) {
            // ignore JSON parse errors
          }

          throw new ApiError(errorMessage, response.status, errorData);
        }

        const tokens = (await response.json()) as Tokens;
        await Promise.resolve(options.setTokens(tokens.accessToken, tokens.refreshToken ?? refreshToken));
      })().finally(() => {
        refreshPromise = null;
      });
    }

    return refreshPromise;
  };

  const request = async <T>(method: HttpMethod, path: string, body?: unknown, requestOptions?: ApiClientRequestOptions): Promise<T> => {
    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
      ...requestOptions?.headers,
    };

    const accessToken = await Promise.resolve(options.getAccessToken());
    if (accessToken) {
      headers.Authorization = `Bearer ${accessToken}`;
    }

    const response = await fetch(buildURL(path), {
      method,
      headers,
      body: body !== undefined && method !== 'GET' && method !== 'DELETE' ? JSON.stringify(body) : undefined,
    });

    if (response.status === 401) {
      try {
        await refreshTokens();
      } catch (error) {
        if (error instanceof ApiError) {
          throw error;
        }
        throw new ApiError('Unauthorized', 401);
      }

      const retryHeaders = { ...headers };
      const retryAccessToken = await Promise.resolve(options.getAccessToken());
      if (retryAccessToken) {
        retryHeaders.Authorization = `Bearer ${retryAccessToken}`;
      } else {
        delete retryHeaders.Authorization;
      }

      const retryResponse = await fetch(buildURL(path), {
        method,
        headers: retryHeaders,
        body: body !== undefined && method !== 'GET' && method !== 'DELETE' ? JSON.stringify(body) : undefined,
      });

      if (!retryResponse.ok) {
        await Promise.resolve(options.clearTokens());
        options.onUnauthorized?.();
        return handleErrorResponse<T>(retryResponse);
      }

      return handleSuccessResponse<T>(retryResponse);
    }

    if (!response.ok) {
      return handleErrorResponse<T>(response);
    }

    return handleSuccessResponse<T>(response);
  };

  return {
    request,
    get: (path, options) => request('GET', path, undefined, options),
    post: (path, body, options) => request('POST', path, body, options),
    put: (path, body, options) => request('PUT', path, body, options),
    patch: (path, body, options) => request('PATCH', path, body, options),
    delete: (path, options) => request('DELETE', path, undefined, options),
  } satisfies ApiClient;
}

const handleSuccessResponse = async <T>(response: Response): Promise<T> => {
  if (response.status === 204) {
    return undefined as T;
  }

  const contentType = response.headers.get('Content-Type');
  if (contentType && contentType.includes('application/json')) {
    return (await response.json()) as T;
  }

  return (await response.text()) as unknown as T;
};

const handleErrorResponse = async <T>(response: Response): Promise<T> => {
  let errorMessage = response.statusText || 'Request failed';
  let errorData: unknown;

  const contentType = response.headers.get('Content-Type');
  if (contentType && contentType.includes('application/json')) {
    try {
      errorData = await response.json();
      errorMessage = (errorData as any)?.message ?? errorMessage;
    } catch (error) {
      // ignore JSON parse errors
    }
  } else {
    try {
      const text = await response.text();
      if (text) {
        errorMessage = text;
      }
    } catch (error) {
      // ignore text parse errors
    }
  }

  throw new ApiError(errorMessage, response.status, errorData);
};
