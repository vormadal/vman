import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { apiClient } from '../api/client';
import type { CreateTagRequest, GetItemsParams } from '../api/types';

// Tag query keys
export const tagKeys = {
  all: ['tags'] as const,
  lists: () => [...tagKeys.all, 'list'] as const,
  list: (search?: string) => [...tagKeys.lists(), { search }] as const,
  details: () => [...tagKeys.all, 'detail'] as const,
  detail: (id: string) => [...tagKeys.details(), id] as const,
};

// Item query keys
export const itemKeys = {
  all: ['items'] as const,
  lists: () => [...itemKeys.all, 'list'] as const,
  list: (params?: GetItemsParams) => [...itemKeys.lists(), params] as const,
  details: () => [...itemKeys.all, 'detail'] as const,
  detail: (provider: string, id: string) => [...itemKeys.details(), provider, id] as const,
  byTag: (tagId: string) => [...itemKeys.all, 'byTag', tagId] as const,
};

// Tag hooks
export function useTags(search?: string) {
  return useQuery({
    queryKey: tagKeys.list(search),
    queryFn: () => apiClient.getTags(search),
  });
}

export function useTag(id: string) {
  return useQuery({
    queryKey: tagKeys.detail(id),
    queryFn: () => apiClient.getTagById(id),
    enabled: !!id,
  });
}

export function useCreateTag() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (data: CreateTagRequest) => apiClient.createTag(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: tagKeys.lists() });
    },
  });
}

export function useRenameTag() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ id, newName }: { id: string; newName: string }) => 
      apiClient.renameTag(id, newName),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: tagKeys.lists() });
      queryClient.invalidateQueries({ queryKey: tagKeys.detail(variables.id) });
    },
  });
}

export function useDeleteTag() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (id: string) => apiClient.deleteTag(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: tagKeys.all });
      queryClient.invalidateQueries({ queryKey: itemKeys.all });
    },
  });
}

// Item hooks
export function useItems(params?: GetItemsParams) {
  return useQuery({
    queryKey: itemKeys.list(params),
    queryFn: () => apiClient.getItems(params),
  });
}

export function useItem(provider: string, id: string) {
  return useQuery({
    queryKey: itemKeys.detail(provider, id),
    queryFn: () => apiClient.getItemById(provider, id),
    enabled: !!provider && !!id,
  });
}

export function useItemsByTag(tagId: string, page = 1) {
  return useQuery({
    queryKey: [...itemKeys.byTag(tagId), page],
    queryFn: () => apiClient.getItemsByTag(tagId, page),
    enabled: !!tagId,
  });
}

export function useAddTagToItem() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ provider, itemId, tagId }: { provider: string; itemId: string; tagId: string }) =>
      apiClient.addTagToItem(provider, itemId, tagId),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: itemKeys.detail(variables.provider, variables.itemId) });
      queryClient.invalidateQueries({ queryKey: itemKeys.lists() });
      queryClient.invalidateQueries({ queryKey: tagKeys.all });
    },
  });
}

export function useRemoveTagFromItem() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ provider, itemId, tagId }: { provider: string; itemId: string; tagId: string }) =>
      apiClient.removeTagFromItem(provider, itemId, tagId),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: itemKeys.detail(variables.provider, variables.itemId) });
      queryClient.invalidateQueries({ queryKey: itemKeys.lists() });
      queryClient.invalidateQueries({ queryKey: tagKeys.all });
    },
  });
}
