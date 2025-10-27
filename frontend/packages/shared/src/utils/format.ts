export type DateInput = string | number | Date;

export const formatDate = (
  value: DateInput,
  locale: string = 'en-US',
  options: Intl.DateTimeFormatOptions = {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  }
): string => {
  const date = value instanceof Date ? value : new Date(value);
  if (Number.isNaN(date.getTime())) {
    return '';
  }

  return new Intl.DateTimeFormat(locale, options).format(date);
};

export const formatDuration = (totalMinutes: number): string => {
  if (!Number.isFinite(totalMinutes) || totalMinutes <= 0) {
    return '0 min';
  }

  const hours = Math.floor(totalMinutes / 60);
  const minutes = Math.round(totalMinutes % 60);

  const parts: string[] = [];
  if (hours > 0) {
    parts.push(`${hours}h`);
  }

  if (minutes > 0) {
    parts.push(`${minutes} min`);
  }

  return parts.join(' ');
};

export const formatNumber = (value: number, fractionDigits = 0) => {
  if (!Number.isFinite(value)) {
    return '0';
  }

  return value.toLocaleString(undefined, {
    minimumFractionDigits: fractionDigits,
    maximumFractionDigits: fractionDigits,
  });
};
