/**
 * Abstract storage interface that works for both web and mobile
 */
export interface Storage {
  getItem(key: string): Promise<string | null>;
  setItem(key: string, value: string): Promise<void>;
  removeItem(key: string): Promise<void>;
  clear(): Promise<void>;
}

/**
 * Web implementation using localStorage
 * Note: This should only be used in web environments
 */
export class WebStorage implements Storage {
  async getItem(key: string): Promise<string | null> {
    if (typeof window !== 'undefined' && window.localStorage) {
      return window.localStorage.getItem(key);
    }
    return null;
  }

  async setItem(key: string, value: string): Promise<void> {
    if (typeof window !== 'undefined' && window.localStorage) {
      window.localStorage.setItem(key, value);
    }
  }

  async removeItem(key: string): Promise<void> {
    if (typeof window !== 'undefined' && window.localStorage) {
      window.localStorage.removeItem(key);
    }
  }

  async clear(): Promise<void> {
    if (typeof window !== 'undefined' && window.localStorage) {
      window.localStorage.clear();
    }
  }
}

/**
 * Token storage manager
 */
export class TokenStorage {
  private storage: Storage;
  private accessTokenKey = 'gymhero_access_token';
  private refreshTokenKey = 'gymhero_refresh_token';
  private memoryAccessToken: string | null = null;
  private memoryRefreshToken: string | null = null;

  constructor(storage: Storage) {
    this.storage = storage;
  }

  async getAccessToken(): Promise<string | null> {
    if (this.memoryAccessToken) {
      return this.memoryAccessToken;
    }
    return this.storage.getItem(this.accessTokenKey);
  }

  async getRefreshToken(): Promise<string | null> {
    if (this.memoryRefreshToken) {
      return this.memoryRefreshToken;
    }
    return this.storage.getItem(this.refreshTokenKey);
  }

  async setTokens(accessToken: string, refreshToken: string): Promise<void> {
    this.memoryAccessToken = accessToken;
    this.memoryRefreshToken = refreshToken;
    await Promise.all([
      this.storage.setItem(this.accessTokenKey, accessToken),
      this.storage.setItem(this.refreshTokenKey, refreshToken),
    ]);
  }

  async clearTokens(): Promise<void> {
    this.memoryAccessToken = null;
    this.memoryRefreshToken = null;
    await Promise.all([
      this.storage.removeItem(this.accessTokenKey),
      this.storage.removeItem(this.refreshTokenKey),
    ]);
  }

  // Synchronous versions for API client (using memory cache with localStorage fallback)
  getAccessTokenSync(): string | null {
    // If in memory, return it
    if (this.memoryAccessToken) {
      return this.memoryAccessToken;
    }

    // Otherwise, try to load from localStorage synchronously
    if (typeof window !== 'undefined' && window.localStorage) {
      const token = window.localStorage.getItem(this.accessTokenKey);
      if (token) {
        this.memoryAccessToken = token;
        return token;
      }
    }

    return null;
  }

  getRefreshTokenSync(): string | null {
    // If in memory, return it
    if (this.memoryRefreshToken) {
      return this.memoryRefreshToken;
    }

    // Otherwise, try to load from localStorage synchronously
    if (typeof window !== 'undefined' && window.localStorage) {
      const token = window.localStorage.getItem(this.refreshTokenKey);
      if (token) {
        this.memoryRefreshToken = token;
        return token;
      }
    }

    return null;
  }

  setTokensSync(accessToken: string, refreshToken: string): void {
    this.memoryAccessToken = accessToken;
    this.memoryRefreshToken = refreshToken;
    // Fire and forget async storage
    this.storage.setItem(this.accessTokenKey, accessToken);
    this.storage.setItem(this.refreshTokenKey, refreshToken);
  }

  clearTokensSync(): void {
    this.memoryAccessToken = null;
    this.memoryRefreshToken = null;
    // Fire and forget async storage
    this.storage.removeItem(this.accessTokenKey);
    this.storage.removeItem(this.refreshTokenKey);
  }
}
