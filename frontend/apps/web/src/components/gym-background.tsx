'use client';

import { useEffect, useState } from 'react';
import { cn } from '@/lib/utils';

interface GymBackgroundProps {
  className?: string;
  variant?: 'default' | 'auth' | 'dashboard';
}

export function GymBackground({ className, variant = 'default' }: GymBackgroundProps) {
  const [mounted, setMounted] = useState(false);

  useEffect(() => {
    setMounted(true);
  }, []);

  if (!mounted) return null;

  return (
    <div className={cn('fixed inset-0 -z-10 overflow-hidden', className)}>
      {/* Base gradient */}
      <div className="absolute inset-0 bg-gradient-to-br from-background via-background to-primary/5" />

      {/* Animated gradient orbs */}
      <div className="absolute top-0 right-0 w-[600px] h-[600px] bg-primary/10 rounded-full blur-3xl animate-pulse-primary opacity-20" />
      <div
        className="absolute bottom-0 left-0 w-[500px] h-[500px] bg-primary/15 rounded-full blur-3xl opacity-20"
        style={{ animation: 'pulse-primary 3s ease-in-out infinite' }}
      />

      {/* Geometric pattern overlay */}
      <div className="absolute inset-0 gym-pattern opacity-40" />

      {/* Dumbbell pattern (CSS-only) */}
      <svg className="absolute inset-0 w-full h-full opacity-[0.02]" xmlns="http://www.w3.org/2000/svg">
        <defs>
          <pattern id="dumbbell-pattern" x="0" y="0" width="200" height="200" patternUnits="userSpaceOnUse">
            {/* Dumbbell icon pattern */}
            <g transform="translate(100, 100)">
              {/* Left weight */}
              <rect x="-60" y="-15" width="15" height="30" rx="3" fill="currentColor" opacity="0.3" />
              {/* Bar */}
              <rect x="-45" y="-5" width="90" height="10" rx="5" fill="currentColor" opacity="0.3" />
              {/* Right weight */}
              <rect x="45" y="-15" width="15" height="30" rx="3" fill="currentColor" opacity="0.3" />
            </g>
          </pattern>
        </defs>
        <rect width="100%" height="100%" fill="url(#dumbbell-pattern)" />
      </svg>

      {/* Grid pattern */}
      <div
        className="absolute inset-0 opacity-[0.015]"
        style={{
          backgroundImage: `
            linear-gradient(hsl(var(--primary)) 1px, transparent 1px),
            linear-gradient(90deg, hsl(var(--primary)) 1px, transparent 1px)
          `,
          backgroundSize: '100px 100px',
        }}
      />

      {/* Vignette effect */}
      <div className="absolute inset-0 bg-gradient-radial from-transparent via-transparent to-background/80" />
    </div>
  );
}
