import type {
  AuthResponse,
  LoginRequest,
  RegisterRequest,
  TagDto,
  TagsResponse,
  CreateTagRequest,
  CreateTagResponse,
  RenameTagRequest,
  ItemDto,
  ItemDetailDto,
  ItemsResponse,
  GetItemsParams,
  AddTagToItemRequest,
  TriggerSyncRequest,
  TriggerSyncResponse,
  SyncStatusResponse,
  CancelSyncResponse,
  CollectionDto,
  CollectionsResponse,
  CreateCollectionRequest,
  CreateCollectionResponse,
  CollectionDetailDto,
  AddItemToCollectionRequest,
  AddItemToCollectionResponse,
  UpdateCollectionItemOrderRequest
} from './types';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5001';

class ApiClient {
  private baseUrl: string;
  private getAuthToken: (() => string | null) | null = null;

  constructor(baseUrl: string = API_BASE_URL) {
    this.baseUrl = baseUrl;
  }

  setAuthTokenGetter(getter: () => string | null) {
    this.getAuthToken = getter;
  }

  private async request<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<T> {
    const url = `${this.baseUrl}${endpoint}`;

    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
      ...(options.headers as Record<string, string>),
    };

    // Add auth token if available
    if (this.getAuthToken) {
      const token = this.getAuthToken();
      if (token) {
        headers['Authorization'] = `Bearer ${token}`;
      }
    }

    const response = await fetch(url, {
      ...options,
      headers,
    });

    if (!response.ok) {
      // Try to parse ProblemDetails format first
      const errorData = await response.json().catch(() => null);

      if (errorData?.detail) {
        // RFC 7807 ProblemDetails format
        throw new Error(errorData.detail);
      } else if (errorData?.error) {
        // Legacy format fallback
        throw new Error(errorData.error);
      } else {
        // Fallback to status text
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }
    }

    return response.json();
  }

  private async requestBinary(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<Blob> {
    const url = `${this.baseUrl}${endpoint}`;

    const headers: Record<string, string> = {
      ...(options.headers as Record<string, string>),
    };

    // Add auth token if available
    if (this.getAuthToken) {
      const token = this.getAuthToken();
      if (token) {
        headers['Authorization'] = `Bearer ${token}`;
      }
    }

    const response = await fetch(url, {
      ...options,
      headers,
    });

    if (!response.ok) {
      throw new Error(`HTTP ${response.status}: ${response.statusText}`);
    }

    return response.blob();
  }

  async register(data: RegisterRequest): Promise<AuthResponse> {
    return this.request<AuthResponse>('/api/auth/register', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  async login(data: LoginRequest): Promise<AuthResponse> {
    return this.request<AuthResponse>('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  // Tag methods
  async getTags(search?: string, page = 1, pageSize = 50): Promise<TagsResponse> {
    const params = new URLSearchParams();
    if (search) params.append('search', search);
    params.append('page', page.toString());
    params.append('pageSize', pageSize.toString());
    
    return this.request<TagsResponse>(`/api/tags?${params}`);
  }

  async getTagById(id: string): Promise<TagDto> {
    return this.request<TagDto>(`/api/tags/${id}`);
  }

  async createTag(data: CreateTagRequest): Promise<CreateTagResponse> {
    return this.request<CreateTagResponse>('/api/tags', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  async renameTag(id: string, newName: string): Promise<TagDto> {
    return this.request<TagDto>(`/api/tags/${id}`, {
      method: 'PUT',
      body: JSON.stringify({ id, newName }),
    });
  }

  async deleteTag(id: string): Promise<{ success: boolean }> {
    return this.request<{ success: boolean }>(`/api/tags/${id}`, {
      method: 'DELETE',
    });
  }

  // Item methods
  async getItems(params?: GetItemsParams): Promise<ItemsResponse> {
    const searchParams = new URLSearchParams();
    if (params?.provider) searchParams.append('provider', params.provider);
    if (params?.type) searchParams.append('type', params.type);
    if (params?.untagged !== undefined) searchParams.append('untagged', params.untagged.toString());
    if (params?.tagId) searchParams.append('tagId', params.tagId);
    if (params?.isFavorite !== undefined) searchParams.append('isFavorite', params.isFavorite.toString());
    if (params?.sortBy) searchParams.append('sortBy', params.sortBy);
    if (params?.sortDescending !== undefined) searchParams.append('sortDescending', params.sortDescending.toString());
    searchParams.append('page', (params?.page || 1).toString());
    searchParams.append('pageSize', (params?.pageSize || 50).toString());

    return this.request<ItemsResponse>(`/api/items?${searchParams}`);
  }

  async getItemById(provider: string, id: string): Promise<ItemDetailDto> {
    return this.request<ItemDetailDto>(`/api/items/${provider}/${id}`);
  }

  async addTagToItem(provider: string, itemId: string, tagId: string): Promise<{ success: boolean }> {
    return this.request<{ success: boolean }>(`/api/items/${provider}/${itemId}/tags`, {
      method: 'POST',
      body: JSON.stringify({ tagId }),
    });
  }

  async removeTagFromItem(provider: string, itemId: string, tagId: string): Promise<{ success: boolean }> {
    return this.request<{ success: boolean }>(`/api/items/${provider}/${itemId}/tags/${tagId}`, {
      method: 'DELETE',
    });
  }

  async getItemsByTag(tagId: string, page = 1, pageSize = 50): Promise<ItemsResponse> {
    const params = new URLSearchParams();
    params.append('page', page.toString());
    params.append('pageSize', pageSize.toString());

    return this.request<ItemsResponse>(`/api/tags/${tagId}/items?${params}`);
  }

  async getThumbnail(provider: string, itemId: string): Promise<Blob> {
    return this.requestBinary(`/api/providers/${provider}/items/${itemId}/thumbnail`);
  }

  async getPreview(provider: string, itemId: string): Promise<Blob> {
    return this.requestBinary(`/api/providers/${provider}/items/${itemId}/preview`);
  }

  // Sync methods
  async triggerSync(provider = 'immich'): Promise<TriggerSyncResponse> {
    return this.request<TriggerSyncResponse>('/api/sync', {
      method: 'POST',
      body: JSON.stringify({ provider }),
    });
  }

  async getSyncStatus(jobId?: string, provider = 'immich'): Promise<SyncStatusResponse> {
    const params = new URLSearchParams();
    if (jobId) params.append('jobId', jobId);
    params.append('provider', provider);

    return this.request<SyncStatusResponse>(`/api/sync/status?${params}`);
  }

  async cancelSync(jobId: string): Promise<CancelSyncResponse> {
    return this.request<CancelSyncResponse>(`/api/sync/${jobId}/cancel`, {
      method: 'POST',
    });
  }

  // Collection methods
  async getCollections(page = 1, pageSize = 50): Promise<CollectionsResponse> {
    const params = new URLSearchParams();
    params.append('page', page.toString());
    params.append('pageSize', pageSize.toString());
    
    return this.request<CollectionsResponse>(`/api/collections?${params}`);
  }

  async getCollectionById(id: string): Promise<CollectionDetailDto> {
    return this.request<CollectionDetailDto>(`/api/collections/${id}`);
  }

  async createCollection(data: CreateCollectionRequest): Promise<CreateCollectionResponse> {
    return this.request<CreateCollectionResponse>('/api/collections', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  async deleteCollection(id: string): Promise<{ success: boolean }> {
    return this.request<{ success: boolean }>(`/api/collections/${id}`, {
      method: 'DELETE',
    });
  }

  async addItemToCollection(
    collectionId: string,
    providerName: string,
    providerItemId: string
  ): Promise<AddItemToCollectionResponse> {
    return this.request<AddItemToCollectionResponse>(
      `/api/collections/${collectionId}/items`,
      {
        method: 'POST',
        body: JSON.stringify({ providerName, providerItemId }),
      }
    );
  }

  async removeItemFromCollection(collectionId: string, itemId: string): Promise<{ success: boolean }> {
    return this.request<{ success: boolean }>(
      `/api/collections/${collectionId}/items/${itemId}`,
      {
        method: 'DELETE',
      }
    );
  }

  async updateCollectionItemOrder(
    collectionId: string,
    items: Array<{ itemId: string; newOrder: number }>
  ): Promise<{ success: boolean }> {
    return this.request<{ success: boolean }>(
      `/api/collections/${collectionId}/items/reorder`,
      {
        method: 'PUT',
        body: JSON.stringify({ items }),
      }
    );
  }

  async exportCollectionToShotcut(collectionId: string): Promise<Blob> {
    return this.requestBinary(`/api/collections/${collectionId}/export/shotcut`);
  }
}

// Export singleton instance
export const apiClient = new ApiClient();
