import { API_BASE_URL } from '../utils/constants';
import { tokenStorage } from './tokenStorage';

export class ApiError extends Error {
  public status: number;
  public details?: any;

  constructor(message: string, status: number, details?: any) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.details = details;
  }
}

class ApiClient {
  private refreshPromise: Promise<string | null> | null = null;

  public async getValidToken(): Promise<string | null> {
    const token = await tokenStorage.getToken();
    if (!token) return null;

    if (tokenStorage.isTokenExpired(token)) {
      const refreshed = await this.refreshAccessToken();
      if (refreshed) return refreshed;
    }

    return token;
  }

  public async refreshAccessToken(): Promise<string | null> {
    const refreshToken = await tokenStorage.getRefreshToken();
    if (!refreshToken) {
      return null;
    }

    if (!this.refreshPromise) {
      this.refreshPromise = (async () => {
        try {
          const url = `${API_BASE_URL}/auth/refresh`;
          const response = await fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ refreshToken }),
          });

          if (!response.ok) {
            await tokenStorage.clearTokens();
            return null;
          }

          const data = await response.json();
          const newAccessToken: string | null = data?.accessToken ?? null;
          const newRefreshToken: string | null = data?.refreshToken ?? null;
          await tokenStorage.setTokens(newAccessToken, newRefreshToken);
          return newAccessToken;
        } catch (e) {
          console.warn('[API] Failed to refresh access token:', e);
          await tokenStorage.clearTokens();
          return null;
        } finally {
          this.refreshPromise = null;
        }
      })();
    }

    return this.refreshPromise;
  }

  public async request<T>(
    endpoint: string,
    options: RequestInit = {},
    isRetry = false
  ): Promise<T> {
    const cleanEndpoint = endpoint.startsWith('/') ? endpoint : `/${endpoint}`;
    const url = `${API_BASE_URL}${cleanEndpoint}`;

    const headers: Record<string, string> = {
      ...(options.headers as Record<string, string>),
    };

    if (!headers['Content-Type'] && !(options.body instanceof FormData)) {
      headers['Content-Type'] = 'application/json';
    }

    const token = tokenStorage.getSyncToken() || (await tokenStorage.getToken());
    if (token && !headers['Authorization']) {
      headers['Authorization'] = `Bearer ${token}`;
    }

    try {
      const response = await fetch(url, {
        ...options,
        headers,
      });

      if (!response.ok) {
        if (
          response.status === 401 &&
          !isRetry &&
          !endpoint.includes('/auth/refresh') &&
          !endpoint.includes('/auth/login') &&
          !endpoint.includes('/auth/register') &&
          (await tokenStorage.getRefreshToken())
        ) {
          const newAccessToken = await this.refreshAccessToken();
          if (newAccessToken) {
            return this.request<T>(endpoint, options, true);
          }
        }

        let errorMessage = `HTTP Error ${response.status}: ${response.statusText}`;
        let details: any = null;

        try {
          const errorData = await response.json();
          details = errorData;
          if (errorData.message) {
            errorMessage = errorData.message;
          } else if (errorData.errors && typeof errorData.errors === 'object') {
            const firstKey = Object.keys(errorData.errors)[0];
            if (firstKey && Array.isArray(errorData.errors[firstKey]) && errorData.errors[firstKey].length > 0) {
              errorMessage = errorData.errors[firstKey][0];
            }
          } else if (errorData.title) {
            errorMessage = errorData.title;
          }
        } catch {
        }

        throw new ApiError(errorMessage, response.status, details);
      }

      if (response.status === 204) {
        return {} as T;
      }

      return (await response.json()) as T;
    } catch (err: any) {
      if (err instanceof ApiError) {
        throw err;
      }
      console.warn(`[API] Network error requesting ${url}:`, err.message);
      throw new ApiError(err.message || 'Помилка мережі при запиті до сервера', 0);
    }
  }

  public get<T>(endpoint: string, options?: RequestInit): Promise<T> {
    return this.request<T>(endpoint, { ...options, method: 'GET' });
  }

  public post<T>(endpoint: string, body?: any, options?: RequestInit): Promise<T> {
    return this.request<T>(endpoint, {
      ...options,
      method: 'POST',
      body: body instanceof FormData ? body : JSON.stringify(body),
    });
  }

  public put<T>(endpoint: string, body?: any, options?: RequestInit): Promise<T> {
    return this.request<T>(endpoint, {
      ...options,
      method: 'PUT',
      body: body instanceof FormData ? body : JSON.stringify(body),
    });
  }

  public delete<T>(endpoint: string, options?: RequestInit): Promise<T> {
    return this.request<T>(endpoint, { ...options, method: 'DELETE' });
  }
}

export const api = new ApiClient();
