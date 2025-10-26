// Importe o módulo 'path' do Node.js no topo do ficheiro
const path = require('path'); 

/** @type {import('next').NextConfig} */
const nextConfig = {
  output: 'export',
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
    ignoreBuildErrors: true,
  },
  eslint: {
    ignoreDuringBuilds: true,
  },

  // Diz ao Next.js para compilar o pacote partilhado
  transpilePackages: ['@gymhero/shared'],

  // --- INÍCIO DA CORREÇÃO ---
  // Dizemos manualmente ao Webpack (o motor do 'next build')
  // como encontrar os seus atalhos.
  webpack: (config) => {
    config.resolve.alias = {
      ...config.resolve.alias,

      // ✔️ CORRETO: sem wildcard
      '@': path.resolve(__dirname, 'src'),

      // Opcional: só mantenha se você ainda não quer depender do pacote publicado na workspace
      '@gymhero/shared': path.resolve(__dirname, '../../packages/shared/src'),
    };
    return config;
  },
  
  // --- FIM DA CORREÇÃO ---
};

module.exports = nextConfig;