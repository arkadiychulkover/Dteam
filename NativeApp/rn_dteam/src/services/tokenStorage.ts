import AsyncStorage from '@react-native-async-storage/async-storage';

const ACCESS_TOKEN_KEY = '@dteam_access_token';
const REFRESH_TOKEN_KEY = '@dteam_refresh_token';

class TokenStorage {
  private cachedAccessToken: string | null = null;
  private cachedRefreshToken: string | null = null;
  private isInitialized = false;
  private listeners: Set<(token: string | null) => void> = new Set();

  /**
   * Initializes cache from AsyncStorage. Called on app boot.
   */
  public async init(): Promise<void> {
    if (this.isInitialized) return;
    try {
      const [token, refreshToken] = await Promise.all([
        AsyncStorage.getItem(ACCESS_TOKEN_KEY),
        AsyncStorage.getItem(REFRESH_TOKEN_KEY),
      ]);
      this.cachedAccessToken = token;
      this.cachedRefreshToken = refreshToken;
      this.isInitialized = true;
    } catch (e) {
      console.warn('[TokenStorage] Error initializing tokens:', e);
    }
  }

  /**
   * Synchronously returns cached access token if available.
   */
  public getSyncToken(): string | null {
    return this.cachedAccessToken;
  }

  /**
   * Synchronously returns cached refresh token if available.
   */
  public getSyncRefreshToken(): string | null {
    return this.cachedRefreshToken;
  }

  /**
   * Asynchronously retrieves access token (checks storage if cache is empty).
   */
  public async getToken(): Promise<string | null> {
    if (this.cachedAccessToken) return this.cachedAccessToken;
    try {
      const token = await AsyncStorage.getItem(ACCESS_TOKEN_KEY);
      this.cachedAccessToken = token;
      return token;
    } catch (e) {
      console.warn('[TokenStorage] Error getting access token:', e);
      return null;
    }
  }

  /**
   * Asynchronously retrieves refresh token.
   */
  public async getRefreshToken(): Promise<string | null> {
    if (this.cachedRefreshToken) return this.cachedRefreshToken;
    try {
      const refreshToken = await AsyncStorage.getItem(REFRESH_TOKEN_KEY);
      this.cachedRefreshToken = refreshToken;
      return refreshToken;
    } catch (e) {
      console.warn('[TokenStorage] Error getting refresh token:', e);
      return null;
    }
  }

  /**
   * Saves access token in memory and persistent storage.
   */
  public async setToken(token: string | null): Promise<void> {
    this.cachedAccessToken = token;
    try {
      if (token) {
        await AsyncStorage.setItem(ACCESS_TOKEN_KEY, token);
      } else {
        await AsyncStorage.removeItem(ACCESS_TOKEN_KEY);
      }
      this.notifyListeners(token);
    } catch (e) {
      console.warn('[TokenStorage] Error setting access token:', e);
    }
  }

  /**
   * Saves refresh token in memory and persistent storage.
   */
  public async setRefreshToken(refreshToken: string | null): Promise<void> {
    this.cachedRefreshToken = refreshToken;
    try {
      if (refreshToken) {
        await AsyncStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
      } else {
        await AsyncStorage.removeItem(REFRESH_TOKEN_KEY);
      }
    } catch (e) {
      console.warn('[TokenStorage] Error setting refresh token:', e);
    }
  }

  /**
   * Saves both tokens together.
   */
  public async setTokens(accessToken: string | null, refreshToken: string | null): Promise<void> {
    await Promise.all([
      this.setToken(accessToken),
      this.setRefreshToken(refreshToken),
    ]);
  }

  /**
   * Clears all stored authentication tokens.
   */
  public async clearTokens(): Promise<void> {
    this.cachedAccessToken = null;
    this.cachedRefreshToken = null;
    try {
      await Promise.all([
        AsyncStorage.removeItem(ACCESS_TOKEN_KEY),
        AsyncStorage.removeItem(REFRESH_TOKEN_KEY),
      ]);
      this.notifyListeners(null);
    } catch (e) {
      console.warn('[TokenStorage] Error clearing tokens:', e);
    }
  }

  /**
   * Subscribes to token change events.
   */
  public onTokenChange(callback: (token: string | null) => void): () => void {
    this.listeners.add(callback);
    return () => this.listeners.delete(callback);
  }

  private notifyListeners(token: string | null) {
    this.listeners.forEach((listener) => {
      try {
        listener(token);
      } catch (e) {
        console.warn('[TokenStorage] Listener error:', e);
      }
    });
  }

  /**
   * Parses JWT expiration without external libraries.
   */
  public isTokenExpired(token?: string | null): boolean {
    const t = token ?? this.cachedAccessToken;
    if (!t) return true;

    try {
      const parts = t.split('.');
      if (parts.length !== 3) return false;

      // Base64Url decode
      let base64 = parts[1].replace(/-/g, '+').replace(/_/g, '/');
      while (base64.length % 4) {
        base64 += '=';
      }

      // atob is available in React Native environment
      const jsonPayload = decodeURIComponent(
        Array.prototype.map
          .call(atob(base64), (c: string) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
          .join('')
      );

      const payload = JSON.parse(jsonPayload);
      if (!payload.exp) return false;

      // Expired if current time is within 30 seconds of expiry
      return Date.now() >= payload.exp * 1000 - 30000;
    } catch {
      return false;
    }
  }
}

export const tokenStorage = new TokenStorage();
