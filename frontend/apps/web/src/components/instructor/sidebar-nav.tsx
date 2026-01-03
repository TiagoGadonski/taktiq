'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { cn } from '@/lib/utils';
import { Badge } from '@/components/ui/badge';
import {
  LayoutDashboard,
  Users,
  UsersRound,
  ShoppingBag,
  DollarSign,
  Settings,
  Menu,
  X
} from 'lucide-react';
import { useState } from 'react';
import { Button } from '@/components/ui/button';

interface NavItem {
  href: string;
  icon: any;
  label: string;
  badge?: number;
}

interface SidebarNavProps {
  clientCount?: number;
  pendingInvites?: number;
}

export function InstructorSidebarNav({ clientCount, pendingInvites }: SidebarNavProps) {
  const pathname = usePathname();
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

  const navItems: NavItem[] = [
    {
      href: '/instructor',
      icon: LayoutDashboard,
      label: 'Dashboard'
    },
    {
      href: '/instructor/clients',
      icon: Users,
      label: 'Clientes',
      badge: clientCount
    },
    {
      href: '/instructor/groups',
      icon: UsersRound,
      label: 'Grupos'
    },
    {
      href: '/instructor/marketplace',
      icon: ShoppingBag,
      label: 'Marketplace'
    },
    {
      href: '/instructor/financial',
      icon: DollarSign,
      label: 'Financeiro'
    },
    {
      href: '/instructor/settings',
      icon: Settings,
      label: 'Configurações'
    },
  ];

  const isActive = (href: string) => {
    if (href === '/instructor') {
      return pathname === href;
    }
    return pathname?.startsWith(href);
  };

  return (
    <>
      {/* Mobile Header with Hamburger */}
      <div className="lg:hidden fixed top-0 left-0 right-0 z-50 bg-white border-b px-4 py-3 flex items-center justify-between">
        <h1 className="text-lg font-semibold">Painel do Instrutor</h1>
        <Button
          variant="ghost"
          size="icon"
          onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
        >
          {isMobileMenuOpen ? (
            <X className="h-5 w-5" />
          ) : (
            <Menu className="h-5 w-5" />
          )}
        </Button>
      </div>

      {/* Mobile Menu Overlay */}
      {isMobileMenuOpen && (
        <div
          className="lg:hidden fixed inset-0 bg-black/50 z-40"
          onClick={() => setIsMobileMenuOpen(false)}
        />
      )}

      {/* Desktop Sidebar */}
      <aside className={cn(
        "fixed top-0 left-0 bottom-0 w-64 bg-white border-r z-50 transition-transform duration-200",
        "hidden lg:block"
      )}>
        <div className="p-6">
          <h1 className="text-xl font-bold text-primary">TaktIQ</h1>
          <p className="text-sm text-muted-foreground">Painel do Instrutor</p>
        </div>

        <nav className="px-3 space-y-1">
          {navItems.map((item) => {
            const Icon = item.icon;
            const active = isActive(item.href);

            return (
              <Link
                key={item.href}
                href={item.href}
                className={cn(
                  'flex items-center gap-3 px-3 py-2.5 rounded-lg transition-colors group',
                  active
                    ? 'bg-primary text-primary-foreground'
                    : 'hover:bg-gray-100 text-gray-700'
                )}
              >
                <Icon className={cn(
                  'h-5 w-5 flex-shrink-0',
                  active ? 'text-primary-foreground' : 'text-gray-500 group-hover:text-gray-700'
                )} />
                <span className="flex-1 font-medium">{item.label}</span>
                {item.badge !== undefined && item.badge > 0 && (
                  <Badge
                    variant={active ? 'secondary' : 'default'}
                    className={cn(
                      "ml-auto",
                      active && "bg-primary-foreground text-primary"
                    )}
                  >
                    {item.badge}
                  </Badge>
                )}
              </Link>
            );
          })}
        </nav>
      </aside>

      {/* Mobile Sidebar */}
      <aside className={cn(
        "lg:hidden fixed top-0 left-0 bottom-0 w-64 bg-white border-r z-50 transition-transform duration-200",
        isMobileMenuOpen ? "translate-x-0" : "-translate-x-full"
      )}>
        <div className="p-6">
          <h1 className="text-xl font-bold text-primary">TaktIQ</h1>
          <p className="text-sm text-muted-foreground">Painel do Instrutor</p>
        </div>

        <nav className="px-3 space-y-1">
          {navItems.map((item) => {
            const Icon = item.icon;
            const active = isActive(item.href);

            return (
              <Link
                key={item.href}
                href={item.href}
                onClick={() => setIsMobileMenuOpen(false)}
                className={cn(
                  'flex items-center gap-3 px-3 py-2.5 rounded-lg transition-colors group',
                  active
                    ? 'bg-primary text-primary-foreground'
                    : 'hover:bg-gray-100 text-gray-700'
                )}
              >
                <Icon className={cn(
                  'h-5 w-5 flex-shrink-0',
                  active ? 'text-primary-foreground' : 'text-gray-500 group-hover:text-gray-700'
                )} />
                <span className="flex-1 font-medium">{item.label}</span>
                {item.badge !== undefined && item.badge > 0 && (
                  <Badge
                    variant={active ? 'secondary' : 'default'}
                    className={cn(
                      "ml-auto",
                      active && "bg-primary-foreground text-primary"
                    )}
                  >
                    {item.badge}
                  </Badge>
                )}
              </Link>
            );
          })}
        </nav>
      </aside>

      {/* Mobile Bottom Tabs (Alternative to sidebar) */}
      {/* Uncomment if you prefer bottom navigation on mobile instead of hamburger menu
      <nav className="lg:hidden fixed bottom-0 left-0 right-0 bg-white border-t z-40">
        <div className="grid grid-cols-5 gap-1 p-2">
          {navItems.map((item) => {
            const Icon = item.icon;
            const active = isActive(item.href);

            return (
              <Link
                key={item.href}
                href={item.href}
                className={cn(
                  'flex flex-col items-center gap-1 p-2 rounded-lg transition-colors relative',
                  active
                    ? 'text-primary'
                    : 'text-gray-500 hover:text-gray-700'
                )}
              >
                <Icon className="h-5 w-5" />
                <span className="text-xs font-medium truncate w-full text-center">
                  {item.label}
                </span>
                {item.badge !== undefined && item.badge > 0 && (
                  <Badge
                    variant="destructive"
                    className="absolute -top-1 -right-1 h-5 w-5 p-0 flex items-center justify-center text-xs"
                  >
                    {item.badge}
                  </Badge>
                )}
              </Link>
            );
          })}
        </div>
      </nav>
      */}
    </>
  );
}
