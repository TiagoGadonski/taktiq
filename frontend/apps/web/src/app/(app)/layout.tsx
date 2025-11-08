'use client';

import { useEffect, useState } from 'react';
import { useRouter, usePathname } from 'next/navigation';
import Link from 'next/link';
import {
  Home,
  Dumbbell,
  Target,
  Activity,
  Users,
  LucideIcon,
  Shield,
  UserCog,
  Info,
  Settings,
  LogOut,
  Bell,
  User,
  Moon,
  Sun,
  Calendar,
  Trophy,
} from 'lucide-react';
import { useAuth } from '@/hooks/use-auth';
import { Button } from '@/components/ui/button';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { ThemeSwitcher } from '@/components/theme-switcher';
import { TaktIQLogo } from '@/components/taktiq-logo';
import { getAssetUrl } from '@/lib/env';
import { useTheme } from 'next-themes';

// Main navigation - 5 clean, intuitive tabs
const navigation: Array<{ name: string; href: string; icon: LucideIcon }> = [
  { name: 'Início', href: '/dashboard', icon: Home },
  { name: 'Treinar', href: '/ai-workout', icon: Dumbbell },
  { name: 'Planos', href: '/plans', icon: Target },
  { name: 'Atividade', href: '/activity', icon: Activity },
  { name: 'Comunidade', href: '/friends', icon: Users },
];

export default function AppLayout({ children }: { children: React.ReactNode }) {
  const { user, isLoading, isAuthenticated, logout } = useAuth();
  const router = useRouter();
  const pathname = usePathname();
  const { theme, setTheme } = useTheme();

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      router.push('/login');
    }
  }, [isLoading, isAuthenticated, router]);

  if (isLoading) {
    return (
      <div className="flex h-screen items-center justify-center">
        <p>Carregando...</p>
      </div>
    );
  }

  if (!isAuthenticated) {
    return null;
  }

  // Helper to check if route is active (includes sub-routes)
  const isRouteActive = (href: string) => {
    if (href === '/dashboard') return pathname === href;
    return pathname.startsWith(href);
  };

  // Build additional navigation items based on role
  const additionalNavItems = [];
  if (user?.role === 'Admin') {
    additionalNavItems.push({ name: 'Admin', href: '/admin', icon: Shield });
  }
  if (user?.role === 'PersonalTrainer') {
    additionalNavItems.push({ name: 'Instrutor', href: '/instructor', icon: UserCog });
  }

  return (
    <div className="flex min-h-screen flex-col bg-background gym-pattern">
      {/* Top Navigation Bar - Desktop */}
      <header className="sticky top-0 z-50 hidden border-b glass-strong lg:block">
        <div className="container mx-auto flex h-16 items-center gap-8 px-6">
          {/* Logo */}
          <Link href="/dashboard" className="hover-lift tap-scale">
            <TaktIQLogo width={95} height={27} className="transition-transform hover:scale-105" />
          </Link>

          {/* Desktop Navigation Tabs */}
          <nav className="flex flex-1 items-center gap-1">
            {navigation.map((item) => {
              const isActive = isRouteActive(item.href);
              return (
                <Link
                  key={item.name}
                  href={item.href}
                  className={`flex items-center gap-2 rounded-lg px-4 py-2 text-sm font-medium transition-all hover-lift tap-scale ${
                    isActive
                      ? 'bg-primary text-primary-foreground shadow-md'
                      : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground'
                  }`}
                >
                  <item.icon className="h-4 w-4" />
                  {item.name}
                </Link>
              );
            })}
          </nav>

          {/* Right side - Notifications + Avatar Dropdown */}
          <div className="flex items-center gap-3">
            {/* Notifications */}
            <Button variant="ghost" size="icon" className="hover-lift tap-scale">
              <Bell className="h-5 w-5" />
            </Button>

            {/* Avatar Dropdown Menu */}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button
                  variant="ghost"
                  className="relative h-10 w-10 rounded-full hover-lift tap-scale"
                >
                  <Avatar className="h-10 w-10 ring-2 ring-primary/20 hover:ring-primary/50 transition-all">
                    <AvatarImage src={getAssetUrl(user?.profilePictureUrl)} />
                    <AvatarFallback className="bg-primary/20 text-primary font-bold">
                      {user?.name?.charAt(0).toUpperCase()}
                    </AvatarFallback>
                  </Avatar>
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-56 glass-card">
                <DropdownMenuLabel>
                  <div className="flex flex-col space-y-1">
                    <p className="text-sm font-medium leading-none">{user?.name}</p>
                    <p className="text-xs leading-none text-muted-foreground">{user?.email}</p>
                  </div>
                </DropdownMenuLabel>
                <DropdownMenuSeparator />
                <DropdownMenuItem asChild>
                  <Link href="/profile" className="cursor-pointer">
                    <User className="mr-2 h-4 w-4" />
                    Meu Perfil
                  </Link>
                </DropdownMenuItem>
                <DropdownMenuItem asChild>
                  <Link href="/profile" className="cursor-pointer">
                    <Settings className="mr-2 h-4 w-4" />
                    Configurações
                  </Link>
                </DropdownMenuItem>
                <DropdownMenuItem asChild>
                  <Link href="/training-split" className="cursor-pointer">
                    <Calendar className="mr-2 h-4 w-4" />
                    Divisão de Treinos
                  </Link>
                </DropdownMenuItem>
                <DropdownMenuItem asChild>
                  <Link href="/challenges" className="cursor-pointer">
                    <Trophy className="mr-2 h-4 w-4" />
                    Desafios
                  </Link>
                </DropdownMenuItem>
                <DropdownMenuItem asChild>
                  <Link href="/about" className="cursor-pointer">
                    <Info className="mr-2 h-4 w-4" />
                    Sobre Nós
                  </Link>
                </DropdownMenuItem>

                {/* Role-based items */}
                {additionalNavItems.length > 0 && (
                  <>
                    <DropdownMenuSeparator />
                    {additionalNavItems.map((item) => (
                      <DropdownMenuItem key={item.name} asChild>
                        <Link href={item.href} className="cursor-pointer">
                          <item.icon className="mr-2 h-4 w-4" />
                          {item.name}
                        </Link>
                      </DropdownMenuItem>
                    ))}
                  </>
                )}

                <DropdownMenuSeparator />

                {/* Theme Toggle */}
                <DropdownMenuItem
                  onClick={() => setTheme(theme === 'dark' ? 'light' : 'dark')}
                  className="cursor-pointer"
                >
                  {theme === 'dark' ? (
                    <>
                      <Sun className="mr-2 h-4 w-4" />
                      Modo Claro
                    </>
                  ) : (
                    <>
                      <Moon className="mr-2 h-4 w-4" />
                      Modo Escuro
                    </>
                  )}
                </DropdownMenuItem>

                <DropdownMenuSeparator />
                <DropdownMenuItem onClick={() => logout()} className="cursor-pointer text-destructive">
                  <LogOut className="mr-2 h-4 w-4" />
                  Sair
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          </div>
        </div>
      </header>

      {/* Mobile Top Bar */}
      <header className="sticky top-0 z-50 border-b glass-strong lg:hidden">
        <div className="flex h-14 items-center justify-between px-4">
          {/* Logo */}
          <Link href="/dashboard" className="hover-lift tap-scale">
            <TaktIQLogo width={90} height={26} className="transition-transform" />
          </Link>

          {/* Right side - Notifications + Avatar */}
          <div className="flex items-center gap-2">
            <Button variant="ghost" size="icon" className="h-9 w-9 hover-lift tap-scale">
              <Bell className="h-5 w-5" />
            </Button>

            {/* Avatar Dropdown Menu */}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button
                  variant="ghost"
                  className="relative h-9 w-9 rounded-full hover-lift tap-scale"
                >
                  <Avatar className="h-9 w-9 ring-2 ring-primary/20">
                    <AvatarImage src={getAssetUrl(user?.profilePictureUrl)} />
                    <AvatarFallback className="bg-primary/20 text-primary font-bold text-xs">
                      {user?.name?.charAt(0).toUpperCase()}
                    </AvatarFallback>
                  </Avatar>
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-56 glass-card">
                <DropdownMenuLabel>
                  <div className="flex flex-col space-y-1">
                    <p className="text-sm font-medium leading-none">{user?.name}</p>
                    <p className="text-xs leading-none text-muted-foreground">{user?.email}</p>
                  </div>
                </DropdownMenuLabel>
                <DropdownMenuSeparator />
                <DropdownMenuItem asChild>
                  <Link href="/profile" className="cursor-pointer">
                    <User className="mr-2 h-4 w-4" />
                    Meu Perfil
                  </Link>
                </DropdownMenuItem>
                <DropdownMenuItem asChild>
                  <Link href="/profile" className="cursor-pointer">
                    <Settings className="mr-2 h-4 w-4" />
                    Configurações
                  </Link>
                </DropdownMenuItem>
                <DropdownMenuItem asChild>
                  <Link href="/training-split" className="cursor-pointer">
                    <Calendar className="mr-2 h-4 w-4" />
                    Divisão de Treinos
                  </Link>
                </DropdownMenuItem>
                <DropdownMenuItem asChild>
                  <Link href="/challenges" className="cursor-pointer">
                    <Trophy className="mr-2 h-4 w-4" />
                    Desafios
                  </Link>
                </DropdownMenuItem>
                <DropdownMenuItem asChild>
                  <Link href="/about" className="cursor-pointer">
                    <Info className="mr-2 h-4 w-4" />
                    Sobre Nós
                  </Link>
                </DropdownMenuItem>

                {/* Role-based items */}
                {additionalNavItems.length > 0 && (
                  <>
                    <DropdownMenuSeparator />
                    {additionalNavItems.map((item) => (
                      <DropdownMenuItem key={item.name} asChild>
                        <Link href={item.href} className="cursor-pointer">
                          <item.icon className="mr-2 h-4 w-4" />
                          {item.name}
                        </Link>
                      </DropdownMenuItem>
                    ))}
                  </>
                )}

                <DropdownMenuSeparator />

                {/* Theme Toggle */}
                <DropdownMenuItem
                  onClick={() => setTheme(theme === 'dark' ? 'light' : 'dark')}
                  className="cursor-pointer"
                >
                  {theme === 'dark' ? (
                    <>
                      <Sun className="mr-2 h-4 w-4" />
                      Modo Claro
                    </>
                  ) : (
                    <>
                      <Moon className="mr-2 h-4 w-4" />
                      Modo Escuro
                    </>
                  )}
                </DropdownMenuItem>

                <DropdownMenuSeparator />
                <DropdownMenuItem onClick={() => logout()} className="cursor-pointer text-destructive">
                  <LogOut className="mr-2 h-4 w-4" />
                  Sair
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="flex-1 overflow-y-auto pb-20 lg:pb-6">
        <div className="container mx-auto p-3 sm:p-4 md:p-6">{children}</div>
      </main>

      {/* Bottom Tab Navigation - Mobile Only */}
      <nav className="fixed bottom-0 left-0 right-0 z-50 border-t glass-strong lg:hidden">
        <div className="flex h-16 items-center justify-around px-2">
          {navigation.map((item) => {
            const isActive = isRouteActive(item.href);
            return (
              <Link
                key={item.name}
                href={item.href}
                className={`flex flex-1 flex-col items-center justify-center gap-1 rounded-lg py-2 transition-all tap-scale ${
                  isActive
                    ? 'text-primary'
                    : 'text-muted-foreground hover:text-foreground'
                }`}
              >
                <item.icon className={`h-6 w-6 ${isActive ? 'scale-110' : ''} transition-transform`} />
                <span className={`text-xs font-medium ${isActive ? 'font-semibold' : ''}`}>
                  {item.name}
                </span>
              </Link>
            );
          })}
        </div>
      </nav>
    </div>
  );
}
