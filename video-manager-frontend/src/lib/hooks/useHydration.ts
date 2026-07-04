import { useSyncExternalStore } from 'react';
import { useAuthStore } from '@/lib/store/authStore';

/**
 * Hook to ensure Zustand store has been hydrated from storage.
 * This prevents race conditions where components try to access auth state
 * before it's been loaded from localStorage.
 */
export function useHydration() {
  return useSyncExternalStore(
    useAuthStore.subscribe,
    () => useAuthStore.getState()._hasHydrated,
    () => false
  );
}
