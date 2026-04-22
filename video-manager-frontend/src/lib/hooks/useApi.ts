import { useQuery, useMutation, useQueryClient, useInfiniteQuery } from '@tanstack/react-query';
import { apiClient } from '../api/client';
import type { CreateTagRequest, GetItemsParams, BulkAddFilteredItemsParams } from '../api/types';


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

// Sync query keys
export const syncKeys = {
  all: ['sync'] as const,
  status: (jobId?: string, provider?: string) => [...syncKeys.all, 'status', { jobId, provider }] as const,
};

// Collection query keys
export const collectionKeys = {
  all: ['collections'] as const,
  lists: () => [...collectionKeys.all, 'list'] as const,
  list: (page?: number) => [...collectionKeys.lists(), { page }] as const,
  details: () => [...collectionKeys.all, 'detail'] as const,
  detail: (id: string) => [...collectionKeys.details(), id] as const,
};

// People query keys
export const peopleKeys = {
  all: ['people'] as const,
  lists: () => [...peopleKeys.all, 'list'] as const,
  list: (search?: string) => [...peopleKeys.lists(), { search }] as const,
  details: () => [...peopleKeys.all, 'detail'] as const,
  detail: (id: string) => [...peopleKeys.details(), id] as const,
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

export function useInfiniteItems(params?: Omit<GetItemsParams, 'page'>) {
  return useInfiniteQuery({
    queryKey: [...itemKeys.lists(), params, 'infinite'],
    queryFn: ({ pageParam = 1 }) =>
      apiClient.getItems({ ...params, page: pageParam, pageSize: 50 }),
    getNextPageParam: (lastPage) => {
      const totalPages = Math.ceil(lastPage.totalCount / lastPage.pageSize);
      const nextPage = lastPage.page + 1;
      return nextPage <= totalPages ? nextPage : undefined;
    },
    initialPageParam: 1,
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

// Sync hooks
export function useSyncStatus(jobId?: string, provider = 'immich', enabled = true) {
  return useQuery({
    queryKey: syncKeys.status(jobId, provider),
    queryFn: () => apiClient.getSyncStatus(jobId, provider),
    enabled,
    refetchInterval: (query) => {
      const data = query.state.data;
      // Poll every 2 seconds while sync is in progress
      if (data?.status === 'Pending' || data?.status === 'InProgress') {
        return 2000;
      }
      return false;
    },
  });
}

export function useTriggerSync() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (provider?: string) => apiClient.triggerSync(provider || 'immich'),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: syncKeys.all });
    },
  });
}

export function useCancelSync() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (jobId: string) => apiClient.cancelSync(jobId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: syncKeys.all });
    },
  });
}

// Collection hooks
export function useCollections(page = 1) {
  return useQuery({
    queryKey: collectionKeys.list(page),
    queryFn: () => apiClient.getCollections(page),
  });
}

export function useCollection(id: string) {
  return useQuery({
    queryKey: collectionKeys.detail(id),
    queryFn: () => apiClient.getCollectionById(id),
    enabled: !!id,
  });
}

export function useCreateCollection() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (data: { name: string; description?: string }) => 
      apiClient.createCollection(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: collectionKeys.lists() });
    },
  });
}

export function useDeleteCollection() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (id: string) => apiClient.deleteCollection(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: collectionKeys.all });
    },
  });
}

export function useAddItemToCollection() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ collectionId, providerName, providerItemId }: { 
      collectionId: string; 
      providerName: string; 
      providerItemId: string 
    }) => apiClient.addItemToCollection(collectionId, providerName, providerItemId),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: collectionKeys.detail(variables.collectionId) });
      queryClient.invalidateQueries({ queryKey: collectionKeys.lists() });
    },
  });
}

export function useBulkAddFilteredItemsToCollection() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ collectionId, params }: { collectionId: string; params: BulkAddFilteredItemsParams }) =>
      apiClient.bulkAddFilteredItemsToCollection(collectionId, params),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: collectionKeys.detail(variables.collectionId) });
      queryClient.invalidateQueries({ queryKey: collectionKeys.lists() });
    },
  });
}

export function useUpdateCollectionItemNote() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ collectionId, itemId, note }: { collectionId: string; itemId: string; note: string | null }) =>
      apiClient.updateCollectionItemNote(collectionId, itemId, note),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: collectionKeys.detail(variables.collectionId) });
    },
  });
}

export function useRemoveItemFromCollection() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ collectionId, itemId }: { collectionId: string; itemId: string }) =>
      apiClient.removeItemFromCollection(collectionId, itemId),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: collectionKeys.detail(variables.collectionId) });
      queryClient.invalidateQueries({ queryKey: collectionKeys.lists() });
    },
  });
}

export function useUpdateCollectionItemOrder() {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ collectionId, items }: { 
      collectionId: string; 
      items: Array<{ itemId: string; newOrder: number }> 
    }) => apiClient.updateCollectionItemOrder(collectionId, items),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: collectionKeys.detail(variables.collectionId) });
    },
  });
}

export function useExportCollectionToShotcut() {
  return useMutation({
    mutationFn: (collectionId: string) => apiClient.exportCollectionToShotcut(collectionId),
  });
}

// People hooks
export function usePeople(search?: string, pageSize = 50) {
  return useQuery({
    queryKey: peopleKeys.list(search),
    queryFn: () => apiClient.getPeople(search, 1, pageSize),
  });
}

export function usePerson(id: string) {
  return useQuery({
    queryKey: peopleKeys.detail(id),
    queryFn: () => apiClient.getPersonById(id),
    enabled: !!id,
  });
}
