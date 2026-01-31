import { useEffect, useState } from 'react';
import { useAuthStore } from '@/lib/store/authStore';

/**
 * Hook to ensure Zustand store has been hydrated from storage.
 * This prevents race conditions where components try to access auth state
 * before it's been loaded from localStorage.
 */
export function useHydration() {
  const [hydrated, setHydrated] = useState(false);

  useEffect(() => {
    // Check if already hydrated
    if (useAuthStore.getState()._hasHydrated) {
      setHydrated(true);
      return;
    }

    // Subscribe to state changes
    const unsubscribe = useAuthStore.subscribe(
      (state) => {
        if (state._hasHydrated && !hydrated) {
          setHydrated(true);
        }
      }
    );

    return () => unsubscribe();
  }, [hydrated]);

  return hydrated;
}
