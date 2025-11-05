import type { Metadata, Viewport } from 'next';
import { Inter } from 'next/font/google';
import './globals.css';
import { Providers } from './providers';
import { GymBackground } from '@/components/gym-background';

const inter = Inter({ subsets: ['latin'] });

export const metadata: Metadata = {
  title: 'TaktIQ - Seu ritmo, seus resultados.',
  description: 'Acompanhe seus treinos, desafie-se e alcance seus objetivos fitness com TaktIQ',
  icons: {
    icon: '/favicon.ico',
    apple: '/apple-icon.png',
  },
};

export const viewport: Viewport = {
  width: 'device-width',
  initialScale: 1,
  maximumScale: 5,
  userScalable: true,
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="pt-BR" suppressHydrationWarning>
      <body className={inter.className}>
        <GymBackground />
        <Providers>{children}</Providers>
      </body>
    </html>
  );
}
