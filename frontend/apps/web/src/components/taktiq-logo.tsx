'use client';

import { useTheme } from 'next-themes';
import { useEffect, useState } from 'react';
import Image from 'next/image';

interface TaktIQLogoProps {
  size?: number;
  width?: number;
  height?: number;
  className?: string;
}

export function TaktIQLogo({
  size = 120,
  width,
  height,
  className = ''
}: TaktIQLogoProps) {
  const { theme, resolvedTheme } = useTheme();
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);
  }, []);

  // Prevent hydration mismatch by showing a placeholder during SSR
  if (!mounted) {
    return (
      <div
        style={{ width: width || size, height: height || (size / 3.5) }}
        className={`animate-pulse bg-primary/20 rounded ${className}`}
      />
    );
  }

  // Determine which theme is actually being used
  const currentTheme = theme === 'system' ? resolvedTheme : theme;
  const isDark = currentTheme === 'dark';

  // Select the appropriate PNG logo based on theme
  const logoSrc = isDark
    ? '/taktiq-logo-dark.png'  // Light/white logo for dark theme
    : '/taktiq-logo-light.png'; // Dark logo for light theme

  const logoWidth = width || size;
  const logoHeight = height || (size / 3.5); // Maintain aspect ratio

  return (
    <Image
      src={logoSrc}
      alt="TaktIQ"
      width={logoWidth}
      height={logoHeight}
      className={className}
      priority
    />
  );
}
