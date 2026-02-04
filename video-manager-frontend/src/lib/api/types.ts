// Auto-generated API types from backend
export interface UserDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
}

export interface AuthResponse {
  user: UserDto;
  accessToken: string;
  refreshToken: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface ErrorResponse {
  error: string;
}

// Tag types
export interface TagDto {
  id: string;
  name: string;
  itemCount: number;
  createdAt: string;
  updatedAt: string;
}

export interface TagsResponse {
  tags: TagDto[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface CreateTagRequest {
  name: string;
}

export interface CreateTagResponse {
  id: string;
  name: string;
  createdAt: string;
}

export interface RenameTagRequest {
  id: string;
  newName: string;
}

// Item types
export enum MediaType {
  Image = 'Image',
  Video = 'Video',
  Audio = 'Audio',
  Other = 'Other'
}

export interface ItemTagDto {
  id: string;
  name: string;
}

export interface ItemDto {
  provider: string;
  id: string;
  name: string;
  type: MediaType;
  createdAt: string;
  thumbnailUrl?: string;
  previewUrl?: string;
  isFavorite: boolean;
  tags: ItemTagDto[];
}

export interface ItemDetailDto extends ItemDto {
  updatedAt?: string;
  description?: string;
  fileSizeBytes?: number;
  duration?: string;
  width?: number;
  height?: number;
}

export interface ItemsResponse {
  items: ItemDto[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface GetItemsParams {
  provider?: string;
  type?: MediaType;
  untagged?: boolean;
  tagId?: string;
  isFavorite?: boolean;
  sortBy?: string;
  sortDescending?: boolean;
  page?: number;
  pageSize?: number;
}

export interface AddTagToItemRequest {
  tagId: string;
}

// Sync types
export interface TriggerSyncRequest {
  provider?: string;
}

export interface TriggerSyncResponse {
  jobId: string;
  status: string;
  message: string;
}

export interface SyncStatusResponse {
  jobId: string;
  providerName: string;
  status: 'Pending' | 'InProgress' | 'Completed' | 'Failed' | 'Cancelled';
  startedAt: string;
  completedAt?: string;
  totalItems: number;
  processedItems: number;
  errorMessage?: string;
}

export interface CancelSyncResponse {
  jobId: string;
  status: string;
  message: string;
}

