// Importe o módulo 'path' do Node.js no topo do ficheiro
const path = require('path'); 

/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
  
  // 1. Diz ao Next.js para gerar uma pasta 'out' para o deploy estático
  output: 'export', 
  
  env: {
    NEXT_PUBLIC_API_BASE_URL: process.env.NEXT_PUBLIC_API_BASE_URL,
  },
  images: {
    // 2. Desativa a otimização de imagens (necessário para 'export')
    unoptimized: true, 
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

  // 3. Diz ao Next.js para compilar o pacote partilhado
  transpilePackages: ['@gymhero/shared'],

  // 4. Diz ao Webpack (o motor do 'next build') como encontrar os atalhos
  webpack: (config) => {
    config.resolve.alias = {
      ...config.resolve.alias,
      "@/*": path.resolve(__dirname, "src"),
      "@gymhero/shared": path.resolve(__dirname, "../../packages/shared/src"),
    };
    return config;
  },
};

module.exports = nextConfig;