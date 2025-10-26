/** @type {import('next').NextConfig} */
const nextConfig = {
    // Removemos os aliases e deixamos a configuração mais simples
    reactStrictMode: true,
    output: 'standalone', // Mantemos este para o App Service Node.js
    
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
    
    // Deixamos a transpilação para o monorepo
    transpilePackages: ['@gymhero/shared'], 

    typescript: {
        ignoreBuildErrors: true,
    },
    eslint: {
        ignoreDuringBuilds: true,
    },
};

module.exports = nextConfig;