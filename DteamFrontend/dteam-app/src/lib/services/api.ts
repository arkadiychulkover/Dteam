import { API_BASE_URL } from '../utils/constants';

class ApiClient {
  private token: string | null = null;
  private refreshToken: string | null = null;
  // Щоб паралельні 401-запити не спричиняли декілька одночасних /auth/refresh
  private refreshPromise: Promise<string | null> | null = null;

  constructor() {
    if (typeof window !== 'undefined' && window.localStorage) {
      try {
        this.token = localStorage.getItem('dteam_token');
        this.refreshToken = localStorage.getItem('dteam_refresh_token');
      } catch (e) {
        console.warn('[API] Failed to read token from localStorage:', e);
      }
    }
  }

  public setToken(token: string | null) {
    this.token = token;
    if (typeof window !== 'undefined' && window.localStorage) {
      try {
        if (token) {
          localStorage.setItem('dteam_token', token);
        } else {
          localStorage.removeItem('dteam_token');
        }
      } catch (e) {
        console.warn('[API] Failed to save token to localStorage:', e);
      }
    }
  }

  public getToken(): string | null {
    if (!this.token && typeof window !== 'undefined' && window.localStorage) {
      try {
        this.token = localStorage.getItem('dteam_token');
      } catch {
        // ignore
      }
    }
    return this.token;
  }

  public setRefreshToken(refreshToken: string | null) {
    this.refreshToken = refreshToken;
    if (typeof window !== 'undefined' && window.localStorage) {
      try {
        if (refreshToken) {
          localStorage.setItem('dteam_refresh_token', refreshToken);
        } else {
          localStorage.removeItem('dteam_refresh_token');
        }
      } catch (e) {
        console.warn('[API] Failed to save refresh token to localStorage:', e);
      }
    }
  }

  public getRefreshToken(): string | null {
    if (!this.refreshToken && typeof window !== 'undefined' && window.localStorage) {
      try {
        this.refreshToken = localStorage.getItem('dteam_refresh_token');
      } catch {
        // ignore
      }
    }
    return this.refreshToken;
  }

  // Викликається після успішного логіну/реєстрації/рефрешу — зберігає обидва токени одразу.
  public setTokens(accessToken: string | null, refreshToken: string | null) {
    this.setToken(accessToken);
    this.setRefreshToken(refreshToken);
  }

  // Намагається оновити access-токен за допомогою refresh-токена.
  // Повертає новий access-токен або null, якщо оновити не вдалося.
  private async refreshAccessToken(): Promise<string | null> {
    const currentRefreshToken = this.getRefreshToken();
    if (!currentRefreshToken) {
      return null;
    }

    if (!this.refreshPromise) {
      this.refreshPromise = (async () => {
        try {
          const url = `${API_BASE_URL}/auth/refresh`;
          const response = await fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ refreshToken: currentRefreshToken }),
          });

          if (!response.ok) {
            this.setTokens(null, null);
            return null;
          }

          const data = await response.json();
          const newAccessToken: string | null = data?.accessToken ?? null;
          const newRefreshToken: string | null = data?.refreshToken ?? null;
          this.setTokens(newAccessToken, newRefreshToken);
          return newAccessToken;
        } catch (e) {
          console.warn('[API] Failed to refresh access token:', e);
          this.setTokens(null, null);
          return null;
        } finally {
          this.refreshPromise = null;
        }
      })();
    }

    return this.refreshPromise;
  }

  public async request<T>(endpoint: string, options: RequestInit = {}, isRetry: boolean = false): Promise<T> {
    const url = `${API_BASE_URL}${endpoint.startsWith('/') ? endpoint : `/${endpoint}`}`;
    
    const headers = new Headers(options.headers);
    if (!headers.has('Content-Type') && !(options.body instanceof FormData)) {
      headers.set('Content-Type', 'application/json');
    }

    const currentToken = this.getToken();
    if (currentToken && !headers.has('Authorization')) {
      headers.set('Authorization', `Bearer ${currentToken}`);
    }

    try {
      const response = await fetch(url, {
        ...options,
        headers,
      });

      if (!response.ok) {
        // Токен протух — пробуємо один раз оновити його через refresh-токен і повторити запит,
        // щоб користувача не викидало з "Спільноти" чи інших авторизованих дій кожні 15 хвилин.
        if (
          response.status === 401 &&
          !isRetry &&
          !endpoint.includes('/auth/refresh') &&
          !endpoint.includes('/auth/login') &&
          !endpoint.includes('/auth/register') &&
          this.getRefreshToken()
        ) {
          const newAccessToken = await this.refreshAccessToken();
          if (newAccessToken) {
            return this.request<T>(endpoint, options, true);
          }
        }

        let errorMessage = `HTTP Error ${response.status}: ${response.statusText}`;
        let status = response.status;
        try {
          const errorData = await response.json();
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
          // ignore json parse error
        }

        const err: any = new Error(errorMessage);
        err.status = status;
        throw err;
      }

      if (response.status === 204) {
        return {} as T;
      }

      return await response.json();
    } catch (err: any) {
      console.warn(`[API] Error request to ${url}:`, err.message);
      throw err;
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
