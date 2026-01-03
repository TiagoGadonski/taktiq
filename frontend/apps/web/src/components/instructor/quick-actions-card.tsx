'use client';

import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { LucideIcon } from 'lucide-react';
import { cn } from '@/lib/utils';

interface QuickActionsCardProps {
  title: string;
  description: string;
  icon: LucideIcon;
  onClick?: () => void;
  href?: string;
  variant?: 'default' | 'primary' | 'secondary';
}

export function QuickActionsCard({
  title,
  description,
  icon: Icon,
  onClick,
  href,
  variant = 'default'
}: QuickActionsCardProps) {
  const variantStyles = {
    default: 'border-gray-200 hover:border-gray-300',
    primary: 'border-primary/20 bg-primary/5 hover:bg-primary/10',
    secondary: 'border-blue-200 bg-blue-50/50 hover:bg-blue-50',
  };

  const iconStyles = {
    default: 'text-gray-600',
    primary: 'text-primary',
    secondary: 'text-blue-600',
  };

  const buttonVariants = {
    default: 'default' as const,
    primary: 'default' as const,
    secondary: 'secondary' as const,
  };

  const CardWrapper = href ? 'a' : 'div';
  const cardProps = href ? { href } : {};

  return (
    <Card
      className={cn(
        'transition-all duration-200 hover:shadow-md cursor-pointer group',
        variantStyles[variant]
      )}
      onClick={onClick}
      {...cardProps}
    >
      <CardContent className="p-6">
        <div className="flex flex-col items-center text-center space-y-4">
          {/* Icon */}
          <div className={cn(
            'p-4 rounded-full bg-white shadow-sm group-hover:shadow-md transition-shadow',
            variant === 'primary' && 'bg-primary/10',
            variant === 'secondary' && 'bg-blue-100'
          )}>
            <Icon className={cn('h-8 w-8', iconStyles[variant])} />
          </div>

          {/* Text */}
          <div className="space-y-2">
            <h3 className="font-semibold text-lg">{title}</h3>
            <p className="text-sm text-muted-foreground">{description}</p>
          </div>

          {/* CTA Button */}
          <Button
            variant={buttonVariants[variant]}
            size="sm"
            className="w-full mt-2"
          >
            {title}
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}
