import { describe, it, expect } from 'vitest';
import { formatDate, formatWeight, formatDuration, formatVolume } from './format';

describe('formatDate', () => {
  it('should format date in Portuguese', () => {
    const date = new Date('2025-01-15');
    const result = formatDate(date, 'pt-BR');
    expect(result).toContain('janeiro');
    expect(result).toContain('2025');
  });

  it('should format date in English', () => {
    const date = new Date('2025-01-15');
    const result = formatDate(date, 'en-US');
    expect(result).toContain('January');
    expect(result).toContain('2025');
  });
});

describe('formatWeight', () => {
  it('should format weight in kg', () => {
    const result = formatWeight(100, 'metric');
    expect(result).toBe('100.0 kg');
  });

  it('should format weight in lbs', () => {
    const result = formatWeight(100, 'imperial');
    expect(result).toContain('lbs');
    expect(parseFloat(result)).toBeGreaterThan(200);
  });
});

describe('formatDuration', () => {
  it('should format seconds only', () => {
    const result = formatDuration(45, 'pt-BR');
    expect(result).toBe('45s');
  });

  it('should format minutes and seconds', () => {
    const result = formatDuration(125, 'pt-BR');
    expect(result).toContain('min');
    expect(result).toContain('s');
  });

  it('should format hours and minutes', () => {
    const result = formatDuration(3725, 'pt-BR');
    expect(result).toContain('h');
    expect(result).toContain('min');
  });
});

describe('formatVolume', () => {
  it('should format volume in kg', () => {
    const result = formatVolume(1500, 'metric');
    expect(result).toBe('1500 kg');
  });

  it('should format volume in lbs', () => {
    const result = formatVolume(1500, 'imperial');
    expect(result).toContain('lbs');
  });
});
