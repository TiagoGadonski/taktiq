/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
  transpilePackages: ['@gymhero/shared'],
  env: {
    NEXT_PUBLIC_API_BASE_URL: process.env.NEXT_PUBLIC_API_BASE_URL,
  },
  images: {
    remotePatterns: [
      {
        protocol: 'https',
        hostname: 'raw.githubusercontent.com',
      },
      {
        protocol: 'http',
        hostname: 'localhost',
      },
    ],
  },
  typescript: {
    // !! AVISO !!
    // Isto irá ignorar TODOS os erros de TypeScript durante o build.
    // A sua aplicação pode "crashar" em produção.
    ignoreBuildErrors: true,
  },
  eslint: {
    // Ignora TODOS os avisos/erros do ESLint no build
    ignoreDuringBuilds: true,
  },
  transpilePackages: ['@gymhero/shared'],
};

module.exports = nextConfig;
