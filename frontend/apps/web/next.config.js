// --- CORREÇÃO AQUI: A importação do 'path' deve estar no topo ---
const path = require('path'); 

/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
  
  // 1. O Next.js sempre precisa da instrução 'output' para saber o que fazer
  //output: 'standalone', 
  
  env: {
    NEXT_PUBLIC_API_BASE_URL: process.env.NEXT_PUBLIC_API_BASE_URL,
  },
  images: {
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

  transpilePackages: ['@gymhero/shared'],

  // 2. A CORREÇÃO FINAL DOS ALIASES
  webpack: (config) => {
    config.resolve.alias = {
      ...config.resolve.alias,
      // Usa __dirname que é mais robusto
      "@/*": path.resolve(__dirname, 'src'),
      "@gymhero/shared": path.resolve(__dirname, '../../packages/shared/src'),
    };
    return config;
  },
};

module.exports = nextConfig;