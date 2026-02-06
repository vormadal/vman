import { create } from 'zustand';
import { persist, createJSONStorage } from 'zustand/middleware';
import { apiClient } from '@/lib/api/client';

export type UserRole = 'User' | 'Admin';

export interface UserDto {
  id: string;
  email: string;
  firstName?: string | null;
  lastName?: string | null;
  role: UserRole;
}

interface AuthState {
  user: UserDto | null;
  accessToken: string | null;
  refreshToken: string | null;
  isProfileComplete: boolean;
  _hasHydrated: boolean;
  setAuth: (user: UserDto, accessToken: string, refreshToken: string, isProfileComplete?: boolean) => void;
  clearAuth: () => void;
  isAuthenticated: () => boolean;
  isAdmin: () => boolean;
  setHasHydrated: (state: boolean) => void;
}

// Custom storage that syncs to both localStorage and cookies
const cookieStorage = {
  getItem: (name: string): string | null => {
    if (typeof window === 'undefined') return null;
    return localStorage.getItem(name);
  },
  setItem: (name: string, value: string): void => {
    if (typeof window === 'undefined') return;
    
    // Store in localStorage
    localStorage.setItem(name, value);
    
    // Also store in cookie for middleware access
    // Cookie expires in 7 days
    const expires = new Date();
    expires.setDate(expires.getDate() + 7);
    document.cookie = `${name}=${encodeURIComponent(value)}; path=/; expires=${expires.toUTCString()}; SameSite=Strict`;
  },
  removeItem: (name: string): void => {
    if (typeof window === 'undefined') return;
    
    // Remove from localStorage
    localStorage.removeItem(name);
    
    // Remove cookie
    document.cookie = `${name}=; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT; SameSite=Strict`;
  },
};

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      user: null,
      accessToken: null,
      refreshToken: null,
      isProfileComplete: true,
      _hasHydrated: false,
      setAuth: (user, accessToken, refreshToken, isProfileComplete = true) => {
        set({ user, accessToken, refreshToken, isProfileComplete });
        // Update API client immediately when auth changes
        apiClient.setAuthTokenGetter(() => useAuthStore.getState().accessToken);
      },
      clearAuth: () => {
        set({ user: null, accessToken: null, refreshToken: null, isProfileComplete: true });
        // Update API client immediately when auth is cleared
        apiClient.setAuthTokenGetter(() => null);
      },
      isAuthenticated: () => !!get().accessToken,
      isAdmin: () => get().user?.role === 'Admin',
      setHasHydrated: (state) => set({ _hasHydrated: state }),
    }),
    {
      name: 'auth-storage',
      storage: createJSONStorage(() => cookieStorage),
      onRehydrateStorage: () => {
        // This is called when the store is being rehydrated from storage
        return (state, error) => {
          if (error) {
            console.error('Failed to rehydrate auth store:', error);
            state?.setHasHydrated(true);
            return;
          }
          
          // Set up API client token getter after rehydration completes
          if (state) {
            apiClient.setAuthTokenGetter(() => useAuthStore.getState().accessToken);
            state.setHasHydrated(true);
          }
        };
      },
    }
  )
);

// Initial setup for API client - this ensures the getter function is always set
// The getter will return the current state value, which may be null until rehydration completes
apiClient.setAuthTokenGetter(() => useAuthStore.getState().accessToken);
