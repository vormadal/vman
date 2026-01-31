import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { imageApi } from '@/lib/api/imageApi';
import { Tag } from '@/lib/api/mockData';

export function useImages(params?: { page?: number; pageSize?: number; tag?: string }) {
  return useQuery({
    queryKey: ['images', params?.page, params?.pageSize, params?.tag],
    queryFn: () => imageApi.getImages(params),
  });
}

export function useImage(id: string) {
  return useQuery({
    queryKey: ['image', id],
    queryFn: () => imageApi.getImage(id),
    enabled: !!id,
  });
}

export function useTags() {
  return useQuery({
    queryKey: ['tags'],
    queryFn: () => imageApi.getTags(),
  });
}

export function useAddTagToImage() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ imageId, tag }: { imageId: string; tag: Tag }) =>
      imageApi.addTagToImage(imageId, tag),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['images'] });
      queryClient.invalidateQueries({ queryKey: ['image', variables.imageId] });
    },
  });
}

export function useRemoveTagFromImage() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ imageId, tagId }: { imageId: string; tagId: string }) =>
      imageApi.removeTagFromImage(imageId, tagId),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['images'] });
      queryClient.invalidateQueries({ queryKey: ['image', variables.imageId] });
    },
  });
}

export function useCreateTag() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ name, color }: { name: string; color?: string }) =>
      imageApi.createTag(name, color),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tags'] });
    },
  });
}
