export interface Tag {
  id: string;
  name: string;
  color?: string;
}

export interface Image {
  id: string;
  title: string;
  description?: string;
  url: string;
  thumbnailUrl?: string;
  tags: Tag[];
  createdAt: string;
  width?: number;
  height?: number;
}

// Mock images data
export const mockImages: Image[] = [
  {
    id: '1',
    title: 'Sunset Beach',
    description: 'Beautiful sunset over the ocean',
    url: 'https://images.unsplash.com/photo-1507525428034-b723cf961d3e?w=800',
    thumbnailUrl: 'https://images.unsplash.com/photo-1507525428034-b723cf961d3e?w=400',
    tags: [
      { id: 't1', name: 'nature', color: '#10b981' },
      { id: 't2', name: 'sunset', color: '#f97316' },
      { id: 't3', name: 'beach', color: '#3b82f6' },
    ],
    createdAt: '2026-01-15T10:30:00Z',
    width: 1920,
    height: 1080,
  },
  {
    id: '2',
    title: 'Mountain Peak',
    description: 'Snow-covered mountain peak',
    url: 'https://images.unsplash.com/photo-1506905925346-21bda4d32df4?w=800',
    thumbnailUrl: 'https://images.unsplash.com/photo-1506905925346-21bda4d32df4?w=400',
    tags: [
      { id: 't1', name: 'nature', color: '#10b981' },
      { id: 't4', name: 'mountain', color: '#8b5cf6' },
      { id: 't5', name: 'winter', color: '#06b6d4' },
    ],
    createdAt: '2026-01-16T14:20:00Z',
    width: 1920,
    height: 1280,
  },
  {
    id: '3',
    title: 'City Lights',
    description: 'Night view of a modern city',
    url: 'https://images.unsplash.com/photo-1514565131-fce0801e5785?w=800',
    thumbnailUrl: 'https://images.unsplash.com/photo-1514565131-fce0801e5785?w=400',
    tags: [
      { id: 't6', name: 'city', color: '#eab308' },
      { id: 't7', name: 'night', color: '#1f2937' },
      { id: 't8', name: 'urban', color: '#6366f1' },
    ],
    createdAt: '2026-01-17T18:45:00Z',
    width: 1920,
    height: 1080,
  },
  {
    id: '4',
    title: 'Forest Path',
    description: 'Peaceful path through a green forest',
    url: 'https://images.unsplash.com/photo-1441974231531-c6227db76b6e?w=800',
    thumbnailUrl: 'https://images.unsplash.com/photo-1441974231531-c6227db76b6e?w=400',
    tags: [
      { id: 't1', name: 'nature', color: '#10b981' },
      { id: 't9', name: 'forest', color: '#059669' },
      { id: 't10', name: 'path', color: '#84cc16' },
    ],
    createdAt: '2026-01-18T09:15:00Z',
    width: 1920,
    height: 1280,
  },
  {
    id: '5',
    title: 'Desert Dunes',
    description: 'Sand dunes in the desert',
    url: 'https://images.unsplash.com/photo-1509316785289-025f5b846b35?w=800',
    thumbnailUrl: 'https://images.unsplash.com/photo-1509316785289-025f5b846b35?w=400',
    tags: [
      { id: 't11', name: 'desert', color: '#d97706' },
      { id: 't12', name: 'sand', color: '#fbbf24' },
    ],
    createdAt: '2026-01-19T12:00:00Z',
    width: 1920,
    height: 1080,
  },
  {
    id: '6',
    title: 'Ocean Waves',
    description: 'Waves crashing on the shore',
    url: 'https://images.unsplash.com/photo-1505142468610-359e7d316be0?w=800',
    thumbnailUrl: 'https://images.unsplash.com/photo-1505142468610-359e7d316be0?w=400',
    tags: [
      { id: 't3', name: 'beach', color: '#3b82f6' },
      { id: 't13', name: 'ocean', color: '#0ea5e9' },
      { id: 't14', name: 'waves', color: '#06b6d4' },
    ],
    createdAt: '2026-01-20T07:30:00Z',
    width: 1920,
    height: 1080,
  },
];

// Mock tags data
export const mockTags: Tag[] = [
  { id: 't1', name: 'nature', color: '#10b981' },
  { id: 't2', name: 'sunset', color: '#f97316' },
  { id: 't3', name: 'beach', color: '#3b82f6' },
  { id: 't4', name: 'mountain', color: '#8b5cf6' },
  { id: 't5', name: 'winter', color: '#06b6d4' },
  { id: 't6', name: 'city', color: '#eab308' },
  { id: 't7', name: 'night', color: '#1f2937' },
  { id: 't8', name: 'urban', color: '#6366f1' },
  { id: 't9', name: 'forest', color: '#059669' },
  { id: 't10', name: 'path', color: '#84cc16' },
  { id: 't11', name: 'desert', color: '#d97706' },
  { id: 't12', name: 'sand', color: '#fbbf24' },
  { id: 't13', name: 'ocean', color: '#0ea5e9' },
  { id: 't14', name: 'waves', color: '#06b6d4' },
];
