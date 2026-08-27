import { API_BASE_URL } from '../utils/constants';

class ApiClient {
  private token: string | null = null;

  constructor() {
    if (typeof window !== 'undefined' && window.localStorage) {
      try {
        this.token = localStorage.getItem('dteam_token');
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
      } catch {}
    }
    return this.token;
  }

  public async request<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
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
        } catch {}

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
