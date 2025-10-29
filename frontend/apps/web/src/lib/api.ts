import { createApiClient, createApiEndpoints, WebStorage, TokenStorage } from '@gymhero/shared';

const storage = new WebStorage();
const tokenStorage = new TokenStorage(storage);

export const apiClient = createApiClient({
  baseURL:
    process.env.NEXT_PUBLIC_API_BASE_URL ||
    'https://taktiq-api-cua5a8aucpawb9fk.brazilsouth-01.azurewebsites.net/api',
  getAccessToken: () => tokenStorage.getAccessTokenSync(),
  getRefreshToken: () => tokenStorage.getRefreshTokenSync(),
  setTokens: (accessToken, refreshToken) => tokenStorage.setTokensSync(accessToken, refreshToken),
  clearTokens: () => tokenStorage.clearTokensSync(),
  onUnauthorized: () => {
    if (typeof window !== 'undefined') {
      window.location.href = '/login';
    }
  },
});

export const api = createApiEndpoints(apiClient);
export { tokenStorage };
