const path = require('path'); 

/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
  
  // 1. Diz ao Next.js para gerar uma pasta 'out' para o deploy estático
  output: 'standalone', 
  
  env: {
    NEXT_PUBLIC_API_BASE_URL: process.env.NEXT_PUBLIC_API_BASE_URL,
  },
  images: {
    // 2. Desativa a otimização de imagens (necessário para 'export')
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

  // 3. Esta é a forma moderna de compilar pacotes do workspace
  transpilePackages: ['@gymhero/shared'],

  // 4. A sua configuração do Webpack está correta
  webpack: (config) => {
    config.resolve.alias = {
      ...config.resolve.alias,
      "@/*": path.resolve(__dirname, "src"),
      "@gymhero/shared": path.resolve(__dirname, "../../packages/shared/src"),
    };
    return config;
  },
};

// Não usamos mais o "withTM"
module.exports = nextConfig;