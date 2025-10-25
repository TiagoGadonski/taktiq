import * as SecureStore from 'expo-secure-store';
import { createApiClient, createApiEndpoints, type Storage } from '@gymhero/shared';
import Constants from 'expo-constants';

// Mobile storage implementation using Expo SecureStore
class MobileStorage implements Storage {
  async getItem(key: string): Promise<string | null> {
    return await SecureStore.getItemAsync(key);
  }

  async setItem(key: string, value: string): Promise<void> {
    await SecureStore.setItemAsync(key, value);
  }

  async removeItem(key: string): Promise<void> {
    await SecureStore.deleteItemAsync(key);
  }

  async clear(): Promise<void> {
    // Note: SecureStore doesn't have a clear all method
    // You'd need to track keys separately if needed
  }
}

import { TokenStorage } from '@gymhero/shared';

const storage = new MobileStorage();
const tokenStorage = new TokenStorage(storage);

export const apiClient = createApiClient({
  baseURL:
    Constants.expoConfig?.extra?.apiBaseUrl ||
    process.env.EXPO_PUBLIC_API_BASE_URL ||
    'https://localhost:5001',
  getAccessToken: () => tokenStorage.getAccessTokenSync(),
  getRefreshToken: () => tokenStorage.getRefreshTokenSync(),
  setTokens: (accessToken, refreshToken) => tokenStorage.setTokensSync(accessToken, refreshToken),
  clearTokens: () => tokenStorage.clearTokensSync(),
  onUnauthorized: () => {
    // Will be handled by navigation
  },
});

export const api = createApiEndpoints(apiClient);
export { tokenStorage };
