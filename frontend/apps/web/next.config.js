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
    webpack: (config, { isServer }) => {
        // Alias para resolver o atalho do pacote compartilhado
        config.resolve.alias = {
            ...config.resolve.alias,
            
            // @/ deve apontar para o src local:
            '@/': path.join(__dirname, 'src/'),
            
            // @gymhero/shared deve apontar para a pasta 'packages/shared'
            // O uso de 'path.resolve' é o mais robusto.
            '@gymhero/shared': path.resolve(__dirname, '../../packages/shared/src'),
        };
        return config;
    },
};

module.exports = nextConfig;