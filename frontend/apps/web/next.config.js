const path = require('path');

/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
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
  experimental: {
    // Esta flag permite ao Next.js compilar ficheiros
    // que estão fora do diretório da aplicação (ex: na pasta 'packages')
    externalDir: true,
  },
  webpack: (config) => {
    config.resolve.alias = {
      ...config.resolve.alias,
      
      // Diz ao Webpack: quando vires "@/*", procura na pasta "./src"
      '@/*': path.resolve(__dirname, 'src'),
      
      // Diz ao Webpack: quando vires "@gymhero/shared", procura em "../../packages/shared/src"
      '@gymhero/shared': path.resolve(__dirname, '../../packages/shared/src'),
    };
    return config;
  },
};

module.exports = nextConfig;
