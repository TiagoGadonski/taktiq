'use client';

import { useEffect, useState } from 'react';
import { useRouter, usePathname } from 'next/navigation';
import Link from 'next/link';
import {
  Dumbbell,
  Home,
  Trophy,
  TrendingUp,
  Calendar,
  Target,
  LogOut,
  Menu,
  Sparkles,
  Users,
  LucideIcon,
  Shield,
  UserCog,
  Mail,
} from 'lucide-react';
import { useAuth } from '@/hooks/use-auth';
import { Button } from '@/components/ui/button';
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from '@/components/ui/sheet';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { ThemeSwitcher } from '@/components/theme-switcher';
import { LevelUpIconSimple } from '@/components/level-up-icon';
import { TaktIQLogo } from '@/components/taktiq-logo';
import { getAssetUrl } from '@/lib/env';

const navigation: Array<{ name: string; href: string; icon: LucideIcon }> = [
  { name: 'Dashboard', href: '/dashboard', icon: Home },
  { name: 'Treino IA', href: '/ai-workout', icon: Sparkles },
  { name: 'Treino do Dia', href: '/workout', icon: Dumbbell },
  { name: 'Histórico', href: '/history', icon: Calendar },
  { name: 'Planos', href: '/plans', icon: Target },
  { name: 'Desafios', href: '/challenges', icon: Trophy },
  { name: 'Amigos', href: '/friends', icon: Users },
  { name: 'Progresso', href: '/progress', icon: TrendingUp },
  { name: 'Contato', href: '/contact', icon: Mail },
];

export default function AppLayout({ children }: { children: React.ReactNode }) {
  const { user, isLoading, isAuthenticated, logout } = useAuth();
  const router = useRouter();
  const pathname = usePathname();
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

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

  const NavLinks = ({ onClick }: { onClick?: () => void }) => (
    <>
      {navigation.map((item) => {
        const isActive = pathname === item.href;
        return (
          <Link
            key={item.name}
            href={item.href}
            onClick={onClick}
            className={`flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-all hover-lift tap-scale ${
              isActive
                ? 'bg-primary text-primary-foreground shadow-lg'
                : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground'
            }`}
          >
            <item.icon className="h-5 w-5" />
            {item.name}
          </Link>
        );
      })}
    </>
  );

  // Add admin/instructor navigation based on user role
  const additionalNavigation = [];
  if (user?.role === 'Admin') {
    additionalNavigation.push({ name: 'Admin', href: '/admin', icon: Shield });
  }
  if (user?.role === 'PersonalTrainer') {
    additionalNavigation.push({ name: 'Instrutor', href: '/instructor', icon: UserCog });
  }

  return (
    <div className="flex h-screen bg-background gym-pattern">
      {/* Desktop Sidebar */}
      <aside className="hidden w-64 border-r glass-strong lg:block">
        <div className="flex h-full flex-col">
          <div className="flex h-16 items-center justify-between border-b border-border/50 px-6">
            <Link href="/dashboard" className="hover-lift tap-scale">
              <TaktIQLogo width={140} height={40} className="transition-transform hover:scale-105" />
            </Link>
          </div>

          <nav className="flex-1 space-y-1 px-3 py-4 overflow-y-auto">
            <NavLinks />
            {additionalNavigation.length > 0 && (
              <>
                <div className="my-2 border-t border-border/50" />
                {additionalNavigation.map((item) => {
                  const isActive = pathname === item.href;
                  return (
                    <Link
                      key={item.name}
                      href={item.href}
                      className={`flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-all hover-lift tap-scale ${
                        isActive
                          ? 'bg-primary text-primary-foreground shadow-lg'
                          : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground'
                      }`}
                    >
                      <item.icon className="h-5 w-5" />
                      {item.name}
                    </Link>
                  );
                })}
              </>
            )}
          </nav>

          <div className="border-t border-border/50 p-4">
            <Link
              href="/profile"
              className="mb-3 flex items-center gap-3 px-2 rounded-lg hover:bg-accent/50 transition-colors cursor-pointer group hover-lift tap-scale"
            >
              <Avatar className="h-10 w-10 ring-2 ring-primary/20 group-hover:ring-primary/50 transition-all">
                <AvatarImage src={getAssetUrl(user?.profilePictureUrl)} />
                <AvatarFallback className="bg-primary/20 text-primary font-bold">
                  {user?.name?.charAt(0).toUpperCase()}
                </AvatarFallback>
              </Avatar>
              <div className="flex-1 overflow-hidden">
                <p className="truncate text-sm font-medium">{user?.name}</p>
                <p className="truncate text-xs text-muted-foreground">{user?.email}</p>
              </div>
            </Link>
            <div className="flex gap-2">
              <Button
                variant="outline"
                className="flex-1 justify-start hover-lift tap-scale"
                onClick={() => logout()}
              >
                <LogOut className="mr-2 h-4 w-4" />
                Sair
              </Button>
              <ThemeSwitcher />
            </div>
          </div>
        </div>
      </aside>

      {/* Main content */}
      <main className="flex-1 overflow-y-auto">
        {/* Mobile Header */}
        <div className="sticky top-0 z-40 flex h-16 items-center gap-4 border-b glass-strong px-4 lg:hidden">
          <Sheet open={mobileMenuOpen} onOpenChange={setMobileMenuOpen}>
            <SheetTrigger asChild>
              <Button variant="ghost" size="icon" className="hover-lift tap-scale">
                <Menu className="h-6 w-6" />
                <span className="sr-only">Toggle menu</span>
              </Button>
            </SheetTrigger>
            <SheetContent side="left" className="w-64 p-0 glass-strong">
              <div className="flex h-full flex-col">
                <SheetHeader className="border-b border-border/50 p-6">
                  <SheetTitle>
                    <TaktIQLogo width={120} height={34} />
                  </SheetTitle>
                </SheetHeader>

                <nav className="flex-1 space-y-1 px-3 py-4 overflow-y-auto">
                  <NavLinks onClick={() => setMobileMenuOpen(false)} />
                  {additionalNavigation.length > 0 && (
                    <>
                      <div className="my-2 border-t border-border/50" />
                      {additionalNavigation.map((item) => {
                        const isActive = pathname === item.href;
                        return (
                          <Link
                            key={item.name}
                            href={item.href}
                            onClick={() => setMobileMenuOpen(false)}
                            className={`flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-all hover-lift tap-scale ${
                              isActive
                                ? 'bg-primary text-primary-foreground shadow-lg'
                                : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground'
                            }`}
                          >
                            <item.icon className="h-5 w-5" />
                            {item.name}
                          </Link>
                        );
                      })}
                    </>
                  )}
                </nav>

                <div className="border-t border-border/50 p-4">
                  <Link
                    href="/profile"
                    onClick={() => setMobileMenuOpen(false)}
                    className="mb-3 flex items-center gap-3 px-2 rounded-lg hover:bg-accent/50 transition-colors cursor-pointer group hover-lift tap-scale"
                  >
                    <Avatar className="h-10 w-10 ring-2 ring-primary/20 group-hover:ring-primary/50 transition-all">
                      <AvatarImage src={getAssetUrl(user?.profilePictureUrl)} />
                      <AvatarFallback className="bg-primary/20 text-primary font-bold">
                        {user?.name?.charAt(0).toUpperCase()}
                      </AvatarFallback>
                    </Avatar>
                    <div className="flex-1 overflow-hidden">
                      <p className="truncate text-sm font-medium">{user?.name}</p>
                      <p className="truncate text-xs text-muted-foreground">{user?.email}</p>
                    </div>
                  </Link>
                  <div className="flex gap-2">
                    <Button
                      variant="outline"
                      className="flex-1 justify-start hover-lift tap-scale"
                      onClick={() => {
                        logout();
                        setMobileMenuOpen(false);
                      }}
                    >
                      <LogOut className="mr-2 h-4 w-4" />
                      Sair
                    </Button>
                    <ThemeSwitcher />
                  </div>
                </div>
              </div>
            </SheetContent>
          </Sheet>

          <Link href="/dashboard" className="hover-lift tap-scale">
            <TaktIQLogo width={100} height={28} />
          </Link>
          <div className="ml-auto">
            <ThemeSwitcher />
          </div>
        </div>

        {/* Page Content */}
        <div className="container mx-auto p-3 sm:p-4 md:p-6">{children}</div>
      </main>
    </div>
  );
}
