import { useMutation } from '@tanstack/react-query';
import { useRouter, useSearchParams } from 'next/navigation';
import { useAuthStore } from '@/lib/store/authStore';
import { apiClient } from '@/lib/api/client';
import type { LoginRequest, RegisterRequest, CompleteProfileRequest } from '@/lib/api/types';

export function useLogin() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const setAuth = useAuthStore((state) => state.setAuth);

  return useMutation({
    mutationFn: async (credentials: LoginRequest) => {
      return apiClient.login(credentials);
    },
    onSuccess: (data) => {
      const isProfileComplete = data.isProfileComplete ?? true;
      setAuth(data.user, data.accessToken, data.refreshToken, isProfileComplete);
      
      // If profile is incomplete, redirect to complete-profile
      if (!isProfileComplete) {
        router.push('/complete-profile');
        return;
      }
      
      // Redirect to the original page or default to /videos
      const redirectTo = searchParams?.get('redirect') || '/videos';
      router.push(redirectTo);
    },
  });
}

export function useRegister() {
  const router = useRouter();
  const setAuth = useAuthStore((state) => state.setAuth);

  return useMutation({
    mutationFn: async (data: RegisterRequest) => {
      return apiClient.register(data);
    },
    onSuccess: (data) => {
      // Auto-login after registration
      setAuth(data.user, data.accessToken, data.refreshToken);
      router.push('/videos');
    },
  });
}

export function useLogout() {
  const router = useRouter();
  const { clearAuth, refreshToken } = useAuthStore();

  return async () => {
    if (refreshToken) {
      await apiClient.logout(refreshToken);
    }
    clearAuth();
    router.push('/login');
  };
}

export function useAcceptInvite() {
  const router = useRouter();
  const setAuth = useAuthStore((state) => state.setAuth);

  return useMutation({
    mutationFn: ({ token, password }: { token: string; password: string }) =>
      apiClient.acceptInvite(token, password),
    onSuccess: (data) => {
      setAuth(data.user, data.accessToken, data.refreshToken, false);
      router.push('/complete-profile');
    },
  });
}

export function useCompleteProfile() {
  const router = useRouter();
  const { setAuth } = useAuthStore();

  return useMutation({
    mutationFn: (data: CompleteProfileRequest) => apiClient.completeProfile(data),
    onSuccess: (data) => {
      setAuth(data.user, data.accessToken, data.refreshToken, true);
      router.push('/videos');
    },
  });
}
