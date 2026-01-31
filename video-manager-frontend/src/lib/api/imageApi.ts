import { Image, Tag, mockImages, mockTags } from './mockData';

// Simulated delay for realistic API behavior
const delay = (ms: number) => new Promise(resolve => setTimeout(resolve, ms));

export const imageApi = {
  // Get all images with optional filtering
  async getImages(params?: { page?: number; pageSize?: number; tag?: string }): Promise<{ images: Image[]; totalCount: number }> {
    await delay(500);
    
    let filteredImages = [...mockImages];
    
    // Filter by tag if provided
    if (params?.tag) {
      filteredImages = filteredImages.filter(img => 
        img.tags.some(t => t.name.toLowerCase() === params.tag?.toLowerCase())
      );
    }
    
    // Pagination
    const page = params?.page || 1;
    const pageSize = params?.pageSize || 20;
    const start = (page - 1) * pageSize;
    const paginatedImages = filteredImages.slice(start, start + pageSize);
    
    return {
      images: paginatedImages,
      totalCount: filteredImages.length,
    };
  },

  // Get a single image by ID
  async getImage(id: string): Promise<Image | undefined> {
    await delay(300);
    return mockImages.find(img => img.id === id);
  },

  // Add tag to image
  async addTagToImage(imageId: string, tag: Tag): Promise<Image> {
    await delay(400);
    const image = mockImages.find(img => img.id === imageId);
    if (!image) {
      throw new Error('Image not found');
    }
    
    // Add tag if not already present
    if (!image.tags.find(t => t.id === tag.id)) {
      image.tags.push(tag);
    }
    
    return image;
  },

  // Remove tag from image
  async removeTagFromImage(imageId: string, tagId: string): Promise<Image> {
    await delay(400);
    const image = mockImages.find(img => img.id === imageId);
    if (!image) {
      throw new Error('Image not found');
    }
    
    image.tags = image.tags.filter(t => t.id !== tagId);
    return image;
  },

  // Get all available tags
  async getTags(): Promise<Tag[]> {
    await delay(300);
    return [...mockTags];
  },

  // Create a new tag
  async createTag(name: string, color?: string): Promise<Tag> {
    await delay(400);
    const newTag: Tag = {
      id: `t${Date.now()}`,
      name,
      color: color || '#6b7280',
    };
    mockTags.push(newTag);
    return newTag;
  },
};
