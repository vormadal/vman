import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { ApiClient } from '@/lib/utils/api-client';

interface Video {
  id: string;
  title: string;
  description?: string;
  provider: string;
  thumbnailPath?: string;
  duration?: string;
  createdAt: string;
}

export function useVideos(page = 1, pageSize = 20, provider?: string) {
  return useQuery({
    queryKey: ['videos', page, pageSize, provider],
    queryFn: () =>
      ApiClient.get<{ videos: Video[]; totalCount: number }>(
        `/api/videos?page=${page}&pageSize=${pageSize}${provider ? `&provider=${provider}` : ''}`
      ),
  });
}

export function useVideo(id: string) {
  return useQuery({
    queryKey: ['video', id],
    queryFn: () => ApiClient.get<Video>(`/api/videos/${id}`),
    enabled: !!id,
  });
}

export function useDeleteVideo() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => ApiClient.delete(`/api/videos/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['videos'] });
    },
  });
}
