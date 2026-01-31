'use client';

import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ReactNode, useState, useEffect } from 'react';
import { useAuthStore } from '@/lib/store/authStore';

export function Providers({ children }: { children: ReactNode }) {
  const [queryClient] = useState(
    () =>
      new QueryClient({
        defaultOptions: {
          queries: {
            staleTime: 60 * 1000, // 1 minute
            refetchOnWindowFocus: false,
          },
        },
      })
  );

  const [isHydrated, setIsHydrated] = useState(false);

  useEffect(() => {
    // Check if already hydrated
    if (useAuthStore.getState()._hasHydrated) {
      setIsHydrated(true);
      return;
    }

    // Subscribe to hydration state changes
    const unsubscribe = useAuthStore.subscribe(
      (state) => {
        if (state._hasHydrated && !isHydrated) {
          setIsHydrated(true);
        }
      }
    );

    return () => unsubscribe();
  }, [isHydrated]);

  // Show nothing until store is hydrated to prevent flash of wrong state
  if (!isHydrated) {
    return null;
  }

  return (
    <QueryClientProvider client={queryClient}>
      {children}
    </QueryClientProvider>
  );
}
