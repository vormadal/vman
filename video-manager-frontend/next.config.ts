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
    NEXT_PUBLIC_API_URL: process.env.services__apiservice__http__0 || process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000',
  },
};

export default nextConfig;
