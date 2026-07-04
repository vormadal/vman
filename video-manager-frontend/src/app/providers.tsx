'use client';

import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { ReactNode, useState, useSyncExternalStore } from 'react';
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

  const isHydrated = useSyncExternalStore(
    useAuthStore.subscribe,
    () => useAuthStore.getState()._hasHydrated,
    () => false
  );

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
