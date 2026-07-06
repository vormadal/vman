import type {
  AuthResponse,
  LoginRequest,
  RegisterRequest,
  CompleteProfileRequest,
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
  UpdateCollectionItemOrderRequest,
  BulkAddFilteredItemsParams,
  BulkAddFilteredItemsResponse,
  UpdateCollectionItemNoteRequest,
  UpdateCollectionItemNoteResponse,
  PersonDto,
  PeopleResponse,
  PersonDetailResponse,
  AdminUsersResponse,
  AdminInvitesResponse,
  CreateInviteResponse,
} from './types';

// In production (unified container), NEXT_PUBLIC_API_URL is unset so requests are relative —
// nginx routes /api/* to the backend. In dev, set via .env to http://localhost:5001.
const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL ?? '';

class ApiClient {
  private baseUrl: string;
  private getAuthToken: (() => string | null) | null = null;

  constructor(baseUrl: string = API_BASE_URL) {
    this.baseUrl = baseUrl;
  }

  setAuthTokenGetter(getter: () => string | null) {
    this.getAuthToken = getter;
  }

  private refreshPromise: Promise<boolean> | null = null;
  private getRefreshToken: (() => string | null) | null = null;
  private onAuthCleared: (() => void) | null = null;

  setRefreshTokenGetter(getter: () => string | null) {
    this.getRefreshToken = getter;
  }

  setOnAuthCleared(callback: () => void) {
    this.onAuthCleared = callback;
  }

  private async tryRefresh(): Promise<boolean> {
    if (this.refreshPromise) return this.refreshPromise;

    this.refreshPromise = (async () => {
      try {
        const refreshToken = this.getRefreshToken?.();
        if (!refreshToken) return false;

        const response = await fetch(`${this.baseUrl}/api/auth/refresh`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ refreshToken }),
        });

        if (!response.ok) {
          this.onAuthCleared?.();
          return false;
        }

        const data = await response.json();
        // Import dynamically to avoid circular dependency at module load time
        const { useAuthStore } = await import('@/lib/store/authStore');
        const state = useAuthStore.getState();
        if (state.user) {
          state.setAuth(state.user, data.accessToken, data.refreshToken, state.isProfileComplete);
        }
        return true;
      } catch {
        this.onAuthCleared?.();
        return false;
      } finally {
        this.refreshPromise = null;
      }
    })();

    return this.refreshPromise;
  }

  private async request<T>(
    endpoint: string,
    options: RequestInit = {},
    isRetry = false
  ): Promise<T> {
    const url = `${this.baseUrl}${endpoint}`;

    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
      ...(options.headers as Record<string, string>),
    };

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

    // Auto-refresh on 401, but not for auth endpoints or retry attempts
    if (response.status === 401 && !isRetry && !endpoint.startsWith('/api/auth/')) {
      const refreshed = await this.tryRefresh();
      if (refreshed) {
        return this.request<T>(endpoint, options, true);
      }
    }

    if (!response.ok) {
      const errorData = await response.json().catch(() => null);

      if (errorData?.detail) {
        throw new Error(errorData.detail);
      } else if (errorData?.error) {
        throw new Error(errorData.error);
      } else {
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

  async acceptInvite(token: string, password: string): Promise<AuthResponse> {
    return this.request<AuthResponse>('/api/auth/accept-invite', {
      method: 'POST',
      body: JSON.stringify({ token, password }),
    });
  }

  async completeProfile(data: CompleteProfileRequest): Promise<AuthResponse> {
    return this.request<AuthResponse>('/api/auth/complete-profile', {
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  async logout(refreshToken: string): Promise<void> {
    await this.request<unknown>('/api/auth/logout', {
      method: 'POST',
      body: JSON.stringify({ refreshToken }),
    }).catch(() => {
      // Best-effort: clear local state even if server call fails
    });
  }

  // Admin methods
  async getAdminUsers(): Promise<AdminUsersResponse> {
    return this.request<AdminUsersResponse>('/api/admin/users');
  }

  async getAdminInvites(): Promise<AdminInvitesResponse> {
    return this.request<AdminInvitesResponse>('/api/admin/invites');
  }

  async blockUser(userId: string): Promise<{ success: boolean }> {
    return this.request<{ success: boolean }>(`/api/admin/users/${userId}/block`, {
      method: 'POST',
    });
  }

  async unblockUser(userId: string): Promise<{ success: boolean }> {
    return this.request<{ success: boolean }>(`/api/admin/users/${userId}/unblock`, {
      method: 'POST',
    });
  }

  async changeUserRole(userId: string, role: string): Promise<{ success: boolean }> {
    return this.request<{ success: boolean }>(`/api/admin/users/${userId}/role`, {
      method: 'PUT',
      body: JSON.stringify({ userId, role }),
    });
  }

  async createInvite(email: string): Promise<CreateInviteResponse> {
    return this.request<CreateInviteResponse>('/api/admin/invites', {
      method: 'POST',
      body: JSON.stringify({ email }),
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
    if (params?.personId) searchParams.append('personId', params.personId);
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
        body: JSON.stringify({ collectionId, providerName, providerItemId }),
      }
    );
  }

  async bulkAddFilteredItemsToCollection(
    collectionId: string,
    params: BulkAddFilteredItemsParams
  ): Promise<BulkAddFilteredItemsResponse> {
    const searchParams = new URLSearchParams();
    if (params.provider) searchParams.append('provider', params.provider);
    if (params.type) searchParams.append('type', params.type);
    if (params.tagId) searchParams.append('tagId', params.tagId);
    if (params.personId) searchParams.append('personId', params.personId);

    return this.request<BulkAddFilteredItemsResponse>(
      `/api/collections/${collectionId}/items/bulk-by-filter?${searchParams}`,
      { method: 'POST' }
    );
  }

  async updateCollectionItemNote(
    collectionId: string,
    itemId: string,
    note: string | null
  ): Promise<UpdateCollectionItemNoteResponse> {
    return this.request<UpdateCollectionItemNoteResponse>(
      `/api/collections/${collectionId}/items/${itemId}/note`,
      {
        method: 'PATCH',
        body: JSON.stringify({ collectionId, itemId, note }),
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
        body: JSON.stringify({ collectionId, items }),
      }
    );
  }

  async exportCollectionToShotcut(collectionId: string): Promise<Blob> {
    return this.requestBinary(`/api/collections/${collectionId}/export/shotcut`);
  }

  // People endpoints
  async getPeople(search?: string, page = 1, pageSize = 50): Promise<PeopleResponse> {
    const params = new URLSearchParams({
      page: page.toString(),
      pageSize: pageSize.toString(),
    });
    
    if (search) {
      params.append('search', search);
    }

    return this.request<PeopleResponse>(`/api/people?${params.toString()}`);
  }

  async getPersonById(id: string): Promise<PersonDetailResponse> {
    return this.request<PersonDetailResponse>(`/api/people/${id}`);
  }
}

// Export singleton instance
export const apiClient = new ApiClient();
