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
  MapPin,
  ShoppingBag,
  Search,
  CreditCard,
  MessageCircle,
  DollarSign,
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
import { NotificationsDropdown } from '@/components/notifications-dropdown';
import { AnnouncementPopup } from '@/components/announcement-popup';
import { ChatDrawer } from '@/components/chat/chat-drawer';
import { getAssetUrl } from '@/lib/env';
import { useTheme } from 'next-themes';
import { useChat } from '@/hooks/use-chat';
import { FEATURE_FLAGS } from '@/lib/feature-flags';

// Main navigation - 5 clean, intuitive tabs
const navigation: Array<{ name: string; href: string; icon: LucideIcon }> = [
  { name: 'Início', href: '/dashboard', icon: Home },
  { name: 'Treinar', href: '/ai-workout', icon: Dumbbell },
  { name: 'Planos', href: '/plans', icon: Target },
  { name: 'Atividade', href: '/activity', icon: Activity },
  ...(FEATURE_FLAGS.social ? [{ name: 'Comunidade', href: '/friends', icon: Users }] : []),
];

export default function AppLayout({ children }: { children: React.ReactNode }) {
  const { user, isLoading, isAuthenticated, logout } = useAuth();
  const router = useRouter();
  const pathname = usePathname();
  const { theme, setTheme } = useTheme();
  const { totalUnreadCount } = useChat();
  const [chatOpen, setChatOpen] = useState(false);

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
    additionalNavItems.push({ name: 'Saques', href: '/admin/withdrawals', icon: DollarSign });
  }
  if (user?.role === 'PersonalTrainer') {
    additionalNavItems.push({ name: 'Instrutor', href: '/instructor', icon: UserCog });
    additionalNavItems.push({ name: 'Ganhos', href: '/earnings', icon: DollarSign });
  }

  return (
    <div className="flex min-h-screen flex-col bg-background gym-pattern overflow-x-hidden">
      {/* Top Navigation Bar - Desktop */}
      <header className="sticky top-0 z-50 hidden border-b glass-strong lg:block">
        <div className="container mx-auto flex h-16 items-center gap-8 px-6 max-w-7xl">
          {/* Logo */}
          <Link href="/dashboard" className="hover-lift tap-scale flex-shrink-0">
            <TaktIQLogo width={140} height={40} className="transition-transform hover:scale-105" />
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

          {/* Right side - Chat + Notifications + Avatar Dropdown */}
          <div className="flex items-center gap-3">
            {/* Chat */}
            <Button
              variant="ghost"
              size="icon"
              className="relative hover-lift tap-scale"
              onClick={() => setChatOpen(true)}
            >
              <MessageCircle className="h-5 w-5" />
              {totalUnreadCount > 0 && (
                <span className="absolute -top-1 -right-1 h-5 w-5 rounded-full bg-destructive text-destructive-foreground text-xs flex items-center justify-center font-bold">
                  {totalUnreadCount > 9 ? '9+' : totalUnreadCount}
                </span>
              )}
            </Button>

            {/* Notifications */}
            <NotificationsDropdown />

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
                  <Link href="/settings" className="cursor-pointer">
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
                  <Link href="/gyms" className="cursor-pointer">
                    <MapPin className="mr-2 h-4 w-4" />
                    Academias Próximas
                  </Link>
                </DropdownMenuItem>
                {/* Hide certain options for Personal Trainers */}
                {user?.role !== 'PersonalTrainer' && FEATURE_FLAGS.marketplace && (
                  <>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem asChild>
                      <Link href="/trainers" className="cursor-pointer">
                        <Search className="mr-2 h-4 w-4" />
                        Encontrar Personal
                      </Link>
                    </DropdownMenuItem>
                    <DropdownMenuItem asChild>
                      <Link href="/discover" className="cursor-pointer">
                        <ShoppingBag className="mr-2 h-4 w-4" />
                        Descobrir Planos
                      </Link>
                    </DropdownMenuItem>
                    <DropdownMenuItem asChild>
                      <Link href="/transactions" className="cursor-pointer">
                        <CreditCard className="mr-2 h-4 w-4" />
                        Transações
                      </Link>
                    </DropdownMenuItem>
                  </>
                )}
                <DropdownMenuSeparator />
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
        <div className="flex h-14 items-center justify-between px-3">
          {/* Logo */}
          <Link href="/dashboard" className="hover-lift tap-scale flex-shrink-0">
            <TaktIQLogo width={100} height={28} className="transition-transform" />
          </Link>

          {/* Right side - Chat + Notifications + Avatar */}
          <div className="flex items-center gap-1.5">
            {/* Chat */}
            <Button
              variant="ghost"
              size="icon"
              className="relative hover-lift tap-scale h-9 w-9"
              onClick={() => setChatOpen(true)}
            >
              <MessageCircle className="h-5 w-5" />
              {totalUnreadCount > 0 && (
                <span className="absolute -top-1 -right-1 h-4 w-4 rounded-full bg-destructive text-destructive-foreground text-[10px] flex items-center justify-center font-bold">
                  {totalUnreadCount > 9 ? '9+' : totalUnreadCount}
                </span>
              )}
            </Button>

            <NotificationsDropdown />

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
                  <Link href="/settings" className="cursor-pointer">
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
                  <Link href="/gyms" className="cursor-pointer">
                    <MapPin className="mr-2 h-4 w-4" />
                    Academias Próximas
                  </Link>
                </DropdownMenuItem>
                {/* Hide certain options for Personal Trainers */}
                {user?.role !== 'PersonalTrainer' && FEATURE_FLAGS.marketplace && (
                  <>
                    <DropdownMenuSeparator />
                    <DropdownMenuItem asChild>
                      <Link href="/trainers" className="cursor-pointer">
                        <Search className="mr-2 h-4 w-4" />
                        Encontrar Personal
                      </Link>
                    </DropdownMenuItem>
                    <DropdownMenuItem asChild>
                      <Link href="/discover" className="cursor-pointer">
                        <ShoppingBag className="mr-2 h-4 w-4" />
                        Descobrir Planos
                      </Link>
                    </DropdownMenuItem>
                    <DropdownMenuItem asChild>
                      <Link href="/transactions" className="cursor-pointer">
                        <CreditCard className="mr-2 h-4 w-4" />
                        Transações
                      </Link>
                    </DropdownMenuItem>
                  </>
                )}
                <DropdownMenuSeparator />
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
      <main className="flex-1 overflow-y-auto pb-20 lg:pb-6 w-full">
        <div className="container mx-auto px-3 py-4 sm:px-4 sm:py-6 lg:px-6 max-w-7xl">{children}</div>
      </main>

      {/* Bottom Tab Navigation - Mobile Only */}
      <nav className="fixed bottom-0 left-0 right-0 z-50 border-t glass-strong lg:hidden">
        <div className="flex h-16 items-center justify-around px-1">
          {navigation.map((item) => {
            const isActive = isRouteActive(item.href);
            return (
              <Link
                key={item.name}
                href={item.href}
                className={`flex flex-1 flex-col items-center justify-center gap-0.5 rounded-lg py-1.5 transition-all tap-scale ${
                  isActive
                    ? 'text-primary'
                    : 'text-muted-foreground hover:text-foreground'
                }`}
              >
                <item.icon className={`h-5 w-5 sm:h-6 sm:w-6 ${isActive ? 'scale-110' : ''} transition-transform`} />
                <span className={`text-[10px] sm:text-xs font-medium ${isActive ? 'font-semibold' : ''}`}>
                  {item.name}
                </span>
              </Link>
            );
          })}
        </div>
      </nav>

      {/* Announcement Popup */}
      <AnnouncementPopup />

      {/* Chat Drawer */}
      <ChatDrawer open={chatOpen} onOpenChange={setChatOpen} />
    </div>
  );
}
