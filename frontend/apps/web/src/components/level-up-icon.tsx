'use client';

import { cn } from '@/lib/utils';

interface LevelUpIconProps {
  className?: string;
  size?: number;
  animate?: boolean;
}

export function LevelUpIcon({ className, size = 32, animate = false }: LevelUpIconProps) {
  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 100 100"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      className={cn(
        'level-up-icon',
        animate && 'animate-level-up',
        className
      )}
    >
      {/* Outer shield/diamond shape */}
      <path
        d="M50 5 L85 35 L85 65 L50 95 L15 65 L15 35 Z"
        className="fill-primary/20 stroke-primary stroke-[3]"
        strokeLinejoin="round"
      />

      {/* Inner gradient fill */}
      <defs>
        <linearGradient id="levelUpGradient" x1="0%" y1="0%" x2="0%" y2="100%">
          <stop offset="0%" stopColor="hsl(var(--primary))" stopOpacity="0.4" />
          <stop offset="100%" stopColor="hsl(var(--primary))" stopOpacity="0.1" />
        </linearGradient>
        <filter id="glow">
          <feGaussianBlur stdDeviation="2" result="coloredBlur"/>
          <feMerge>
            <feMergeNode in="coloredBlur"/>
            <feMergeNode in="SourceGraphic"/>
          </feMerge>
        </filter>
      </defs>

      <path
        d="M50 10 L80 37 L80 63 L50 90 L20 63 L20 37 Z"
        fill="url(#levelUpGradient)"
      />

      {/* Dumbbell - left weight */}
      <rect
        x="25"
        y="55"
        width="8"
        height="15"
        rx="2"
        className="fill-primary"
        filter="url(#glow)"
      />

      {/* Dumbbell - bar */}
      <rect
        x="33"
        y="60"
        width="34"
        height="5"
        rx="2.5"
        className="fill-primary"
        filter="url(#glow)"
      />

      {/* Dumbbell - right weight */}
      <rect
        x="67"
        y="55"
        width="8"
        height="15"
        rx="2"
        className="fill-primary"
        filter="url(#glow)"
      />

      {/* Ascending arrow integrated into design */}
      <path
        d="M50 30 L50 75 M50 30 L40 40 M50 30 L60 40"
        className="stroke-primary stroke-[4]"
        strokeLinecap="round"
        strokeLinejoin="round"
        filter="url(#glow)"
      />

      {/* Upward momentum lines */}
      <path
        d="M35 45 L35 50 M45 40 L45 45 M55 40 L55 45 M65 45 L65 50"
        className="stroke-primary/60 stroke-[2]"
        strokeLinecap="round"
      />

      {/* Center highlight dot */}
      <circle
        cx="50"
        cy="50"
        r="3"
        className="fill-primary"
        opacity="0.8"
      />
    </svg>
  );
}

// Simplified logo version for small spaces
export function LevelUpIconSimple({ className, size = 24 }: { className?: string; size?: number }) {
  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 100 100"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      className={cn('level-up-icon-simple', className)}
    >
      <defs>
        <linearGradient id="simpleGradient" x1="0%" y1="0%" x2="100%" y2="100%">
          <stop offset="0%" stopColor="hsl(var(--primary))" />
          <stop offset="100%" stopColor="hsl(var(--primary))" stopOpacity="0.6" />
        </linearGradient>
      </defs>

      {/* Diamond/shield outline */}
      <path
        d="M50 10 L85 50 L50 90 L15 50 Z"
        fill="url(#simpleGradient)"
        className="stroke-primary stroke-[2]"
      />

      {/* Arrow up */}
      <path
        d="M50 35 L50 65 M50 35 L40 45 M50 35 L60 45"
        className="stroke-background stroke-[5]"
        strokeLinecap="round"
        strokeLinejoin="round"
      />
    </svg>
  );
}
