import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../api/client';

export const adminKeys = {
  all: ['admin'] as const,
  users: () => [...adminKeys.all, 'users'] as const,
  invites: () => [...adminKeys.all, 'invites'] as const,
};

export function useAdminUsers() {
  return useQuery({
    queryKey: adminKeys.users(),
    queryFn: () => apiClient.getAdminUsers(),
  });
}

export function useAdminInvites() {
  return useQuery({
    queryKey: adminKeys.invites(),
    queryFn: () => apiClient.getAdminInvites(),
  });
}

export function useBlockUser() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ userId, block }: { userId: string; block: boolean }) =>
      block ? apiClient.blockUser(userId) : apiClient.unblockUser(userId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: adminKeys.users() });
    },
  });
}

export function useChangeUserRole() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ userId, role }: { userId: string; role: string }) =>
      apiClient.changeUserRole(userId, role),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: adminKeys.users() });
    },
  });
}

export function useCreateInvite() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (email: string) => apiClient.createInvite(email),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: adminKeys.invites() });
    },
  });
}
