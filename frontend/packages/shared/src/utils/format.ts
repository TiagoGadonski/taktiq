/**
 * Format date utilities
 */
export const formatDate = (date: string | Date, locale: string = 'pt-BR'): string => {
  const d = typeof date === 'string' ? new Date(date) : date;
  return new Intl.DateTimeFormat(locale, {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  }).format(d);
};

export const formatDateTime = (date: string | Date, locale: string = 'pt-BR'): string => {
  const d = typeof date === 'string' ? new Date(date) : date;
  return new Intl.DateTimeFormat(locale, {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  }).format(d);
};

export const formatTime = (date: string | Date, locale: string = 'pt-BR'): string => {
  const d = typeof date === 'string' ? new Date(date) : date;
  return new Intl.DateTimeFormat(locale, {
    hour: '2-digit',
    minute: '2-digit',
  }).format(d);
};

export const formatRelativeTime = (date: string | Date, locale: string = 'pt-BR'): string => {
  const d = typeof date === 'string' ? new Date(date) : date;
  const now = new Date();
  const diffMs = now.getTime() - d.getTime();
  const diffMins = Math.floor(diffMs / 60000);
  const diffHours = Math.floor(diffMs / 3600000);
  const diffDays = Math.floor(diffMs / 86400000);

  if (diffMins < 1) return locale === 'pt-BR' ? 'agora mesmo' : 'just now';
  if (diffMins < 60)
    return locale === 'pt-BR' ? `há ${diffMins} min` : `${diffMins} min ago`;
  if (diffHours < 24)
    return locale === 'pt-BR' ? `há ${diffHours} h` : `${diffHours} h ago`;
  if (diffDays < 7)
    return locale === 'pt-BR' ? `há ${diffDays} d` : `${diffDays} d ago`;

  return formatDate(d, locale);
};

/**
 * Format duration in seconds to human readable
 */
export const formatDuration = (seconds: number, locale: string = 'pt-BR'): string => {
  const hours = Math.floor(seconds / 3600);
  const minutes = Math.floor((seconds % 3600) / 60);
  const secs = seconds % 60;

  if (hours > 0) {
    return locale === 'pt-BR' ? `${hours}h ${minutes}min` : `${hours}h ${minutes}min`;
  }
  if (minutes > 0) {
    return locale === 'pt-BR' ? `${minutes}min ${secs}s` : `${minutes}min ${secs}s`;
  }
  return locale === 'pt-BR' ? `${secs}s` : `${secs}s`;
};

/**
 * Format weight with unit
 */
export const formatWeight = (kg: number, unit: 'metric' | 'imperial' = 'metric'): string => {
  if (unit === 'imperial') {
    const lbs = kg * 2.20462;
    return `${lbs.toFixed(1)} lbs`;
  }
  return `${kg.toFixed(1)} kg`;
};

/**
 * Format distance with unit
 */
export const formatDistance = (meters: number, unit: 'metric' | 'imperial' = 'metric'): string => {
  if (unit === 'imperial') {
    const miles = meters * 0.000621371;
    if (miles < 0.1) {
      const yards = meters * 1.09361;
      return `${yards.toFixed(0)} yd`;
    }
    return `${miles.toFixed(2)} mi`;
  }
  if (meters < 1000) {
    return `${meters.toFixed(0)} m`;
  }
  return `${(meters / 1000).toFixed(2)} km`;
};

/**
 * Format volume (weight × reps)
 */
export const formatVolume = (kg: number, unit: 'metric' | 'imperial' = 'metric'): string => {
  if (unit === 'imperial') {
    const lbs = kg * 2.20462;
    return `${lbs.toFixed(0)} lbs`;
  }
  return `${kg.toFixed(0)} kg`;
};

/**
 * Format number with thousands separator
 */
export const formatNumber = (num: number, locale: string = 'pt-BR'): string => {
  return new Intl.NumberFormat(locale).format(num);
};
