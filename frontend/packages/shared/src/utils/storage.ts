type MaybePromise<T> = T | Promise<T>;

export interface StorageAdapter {
  getItem(key: string): MaybePromise<string | null>;
  setItem(key: string, value: string): MaybePromise<void>;
  removeItem(key: string): MaybePromise<void>;
  clear(): MaybePromise<void>;
}

const createMemoryStorage = (): StorageAdapter => {
  const store = new Map<string, string>();

  return {
    getItem: (key: string) => store.get(key) ?? null,
    setItem: (key: string, value: string) => {
      store.set(key, value);
    },
    removeItem: (key: string) => {
      store.delete(key);
    },
    clear: () => {
      store.clear();
    },
  };
};

export class WebStorage implements StorageAdapter {
  private readonly storage: StorageAdapter;

  constructor() {
    this.storage =
      typeof window !== 'undefined' && window.localStorage
        ? {
            getItem: (key: string) => window.localStorage.getItem(key),
            setItem: (key: string, value: string) => {
              window.localStorage.setItem(key, value);
            },
            removeItem: (key: string) => {
              window.localStorage.removeItem(key);
            },
            clear: () => {
              window.localStorage.clear();
            },
          }
        : createMemoryStorage();
  }

  async getItem(key: string): Promise<string | null> {
    return await Promise.resolve(this.storage.getItem(key));
  }

  async setItem(key: string, value: string): Promise<void> {
    await Promise.resolve(this.storage.setItem(key, value));
  }

  async removeItem(key: string): Promise<void> {
    await Promise.resolve(this.storage.removeItem(key));
  }

  async clear(): Promise<void> {
    await Promise.resolve(this.storage.clear());
  }
}

export interface TokenStorageOptions {
  accessTokenKey?: string;
  refreshTokenKey?: string;
}

export class TokenStorage {
  private accessToken: string | null = null;
  private refreshToken: string | null = null;
  private hydrated = false;
  private hydratePromise: Promise<void> | null = null;

  private readonly accessTokenKey: string;
  private readonly refreshTokenKey: string;

  constructor(private readonly storage: StorageAdapter, options?: TokenStorageOptions) {
    this.accessTokenKey = options?.accessTokenKey ?? 'gh_access_token';
    this.refreshTokenKey = options?.refreshTokenKey ?? 'gh_refresh_token';
    void this.ensureHydrated();
  }

  private ensureHydrated(): Promise<void> {
    if (this.hydrated) {
      return Promise.resolve();
    }

    if (!this.hydratePromise) {
      this.hydratePromise = (async () => {
        this.accessToken = await Promise.resolve(this.storage.getItem(this.accessTokenKey));
        this.refreshToken = await Promise.resolve(this.storage.getItem(this.refreshTokenKey));
        this.hydrated = true;
      })();
    }

    return this.hydratePromise;
  }

  async getAccessToken(): Promise<string | null> {
    await this.ensureHydrated();
    return this.accessToken;
  }

  getAccessTokenSync(): string | null {
    return this.accessToken;
  }

  async getRefreshToken(): Promise<string | null> {
    await this.ensureHydrated();
    return this.refreshToken;
  }

  getRefreshTokenSync(): string | null {
    return this.refreshToken;
  }

  async setTokens(accessToken: string, refreshToken: string): Promise<void> {
    this.accessToken = accessToken;
    this.refreshToken = refreshToken;
    await Promise.resolve(this.storage.setItem(this.accessTokenKey, accessToken));
    await Promise.resolve(this.storage.setItem(this.refreshTokenKey, refreshToken));
    this.hydrated = true;
  }

  setTokensSync(accessToken: string, refreshToken: string): void {
    this.accessToken = accessToken;
    this.refreshToken = refreshToken;
    void Promise.resolve(this.storage.setItem(this.accessTokenKey, accessToken));
    void Promise.resolve(this.storage.setItem(this.refreshTokenKey, refreshToken));
    this.hydrated = true;
  }

  async clearTokens(): Promise<void> {
    this.accessToken = null;
    this.refreshToken = null;
    await Promise.resolve(this.storage.removeItem(this.accessTokenKey));
    await Promise.resolve(this.storage.removeItem(this.refreshTokenKey));
    this.hydrated = true;
  }

  clearTokensSync(): void {
    this.accessToken = null;
    this.refreshToken = null;
    void Promise.resolve(this.storage.removeItem(this.accessTokenKey));
    void Promise.resolve(this.storage.removeItem(this.refreshTokenKey));
    this.hydrated = true;
  }
}

export type Storage = StorageAdapter;
