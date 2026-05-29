import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Allow images from external sources (Unsplash for mock data)
  images: {
    remotePatterns: [
      {
        protocol: 'https',
        hostname: 'images.unsplash.com',
      },
    ],
  },
  
  // Configure environment variables for Aspire
  env: {
    // Aspire will inject service URLs via environment variables
    // e.g., services__apiservice__http__0 or services__apiservice__https__0
    // Aspire dev: injected via services__apiservice__http__0
    // Standalone dev: set NEXT_PUBLIC_API_URL in .env (e.g. http://localhost:5001)
    // Production (pre-built image pulled by Coolify): empty string → relative URLs, nginx routes /api/*
    NEXT_PUBLIC_API_URL: process.env.services__apiservice__http__0 || process.env.NEXT_PUBLIC_API_URL || '',
  },

  // Enable standalone output for Docker deployments
  output: 'standalone',
};

export default nextConfig;
