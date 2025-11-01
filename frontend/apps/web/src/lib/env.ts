/**
 * Environment configuration helper
 * Provides safe access to environment variables with fallbacks
 */

export const env = {
  apiBaseUrl:
    process.env.NEXT_PUBLIC_API_BASE_URL ||
    'http://localhost:5001/api',
  apiHost: (() => {
    const url =
      process.env.NEXT_PUBLIC_API_BASE_URL ||
      'http://localhost:5001/api';
    // Remove /api or /api/v1 suffix to get the host
    return url.replace(/\/api(\/v1)?$/, '');
  })(),
  isProduction: process.env.NODE_ENV === 'production',
  isDevelopment: process.env.NODE_ENV === 'development',
} as const;

/**
 * Helper to get full URL for uploaded files (like profile pictures)
 */
export function getAssetUrl(path?: string | null): string | undefined {
  if (!path) return undefined;

  // If path is already a full URL, return it
  if (path.startsWith('http://') || path.startsWith('https://')) {
    return path;
  }

  // Otherwise, prepend the API host
  return `${env.apiHost}${path}`;
}
