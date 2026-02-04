'use client';

import { useState, useEffect } from 'react';
import { apiClient } from '@/lib/api/client';
import { cn } from '@/lib/utils';

interface AuthenticatedImageProps extends React.ImgHTMLAttributes<HTMLImageElement> {
  src: string;
  alt: string;
  fallback?: React.ReactNode;
}

export function AuthenticatedImage({
  src,
  alt,
  className,
  fallback,
  ...props
}: AuthenticatedImageProps) {
  const [blobUrl, setBlobUrl] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(false);

  useEffect(() => {
    let isMounted = true;

    setLoading(true);
    setError(false);
    setBlobUrl(null);

    async function fetchImage() {
      try {
        // Parse provider and itemId from URL
        // Expected format: /api/providers/{provider}/items/{itemId}/thumbnail or /preview
        const match = src.match(/\/api\/providers\/([^/]+)\/items\/([^/]+)\/(thumbnail|preview)/);

        if (!match) {
          throw new Error('Invalid thumbnail URL format');
        }

        const [, provider, itemId, type] = match;

        // Use API client to fetch thumbnail with authentication
        const blob = type === 'thumbnail'
          ? await apiClient.getThumbnail(provider, itemId)
          : await apiClient.getPreview(provider, itemId);

        if (!isMounted) return;

        const url = URL.createObjectURL(blob);
        setBlobUrl(url);
        setLoading(false);
      } catch (err) {
        if (!isMounted) return;

        console.error('Failed to load authenticated image:', err);
        setError(true);
        setLoading(false);
      }
    }

    if (src) {
      fetchImage();
    }

    return () => {
      isMounted = false;
      if (blobUrl) {
        URL.revokeObjectURL(blobUrl);
      }
    };
  }, [src]);

  if (loading) {
    return (
      <div className={cn("bg-muted animate-pulse", className)} {...props} />
    );
  }

  if (error || !blobUrl) {
    return fallback ? (
      <>{fallback}</>
    ) : (
      <div className={cn("bg-muted flex items-center justify-center", className)} />
    );
  }

  return (
    <img
      src={blobUrl}
      alt={alt}
      className={className}
      {...props}
    />
  );
}
