'use client';

import { useState } from 'react';
import { useImages, useTags, useAddTagToImage, useRemoveTagFromImage, useCreateTag } from '@/lib/hooks/useImages';
import { Card, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog';
import { Label } from '@/components/ui/label';
import { Tag } from '@/lib/api/mockData';
import { Plus, X, Tag as TagIcon } from 'lucide-react';

export default function ImagesPage() {
  const [selectedTag, setSelectedTag] = useState<string | undefined>();
  const [selectedImage, setSelectedImage] = useState<string | null>(null);
  const [newTagName, setNewTagName] = useState('');
  const [isAddingTag, setIsAddingTag] = useState(false);
  
  const { data, isLoading, error } = useImages({ tag: selectedTag });
  const { data: tags } = useTags();
  const addTagMutation = useAddTagToImage();
  const removeTagMutation = useRemoveTagFromImage();
  const createTagMutation = useCreateTag();

  const handleAddTag = (imageId: string, tag: Tag) => {
    addTagMutation.mutate({ imageId, tag });
  };

  const handleRemoveTag = (imageId: string, tagId: string) => {
    removeTagMutation.mutate({ imageId, tagId });
  };

  const handleCreateTag = () => {
    if (newTagName.trim()) {
      createTagMutation.mutate(
        { name: newTagName.trim() },
        {
          onSuccess: () => {
            setNewTagName('');
            setIsAddingTag(false);
          },
        }
      );
    }
  };

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <p>Loading images...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <p className="text-destructive">Error loading images</p>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-background">
      <header className="border-b">
        <div className="container mx-auto px-4 py-4">
          <h1 className="text-2xl font-bold">Image Manager</h1>
        </div>
      </header>

      <main className="container mx-auto px-4 py-8">
        {/* Filter by tags */}
        <div className="mb-6">
          <div className="flex items-center gap-2 mb-3">
            <TagIcon className="h-5 w-5 text-muted-foreground" />
            <h2 className="text-sm font-semibold text-muted-foreground">Filter by Tag</h2>
            <Button
              variant="ghost"
              size="sm"
              onClick={() => setIsAddingTag(true)}
              className="ml-auto"
            >
              <Plus className="h-4 w-4 mr-1" />
              New Tag
            </Button>
          </div>
          
          <div className="flex flex-wrap gap-2">
            <Badge
              variant={selectedTag === undefined ? 'default' : 'outline'}
              className="cursor-pointer"
              onClick={() => setSelectedTag(undefined)}
            >
              All
            </Badge>
            {tags?.map((tag) => (
              <Badge
                key={tag.id}
                variant={selectedTag === tag.name ? 'default' : 'outline'}
                className="cursor-pointer"
                style={
                  selectedTag === tag.name && tag.color
                    ? { backgroundColor: tag.color, borderColor: tag.color }
                    : {}
                }
                onClick={() => setSelectedTag(selectedTag === tag.name ? undefined : tag.name)}
              >
                {tag.name}
              </Badge>
            ))}
          </div>
        </div>

        {/* Create new tag dialog */}
        <Dialog open={isAddingTag} onOpenChange={setIsAddingTag}>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Create New Tag</DialogTitle>
              <DialogDescription>
                Add a new tag to organize your images.
              </DialogDescription>
            </DialogHeader>
            <div className="space-y-4">
              <div>
                <Label htmlFor="tag-name">Tag Name</Label>
                <Input
                  id="tag-name"
                  value={newTagName}
                  onChange={(e) => setNewTagName(e.target.value)}
                  placeholder="Enter tag name"
                  onKeyDown={(e) => {
                    if (e.key === 'Enter') {
                      handleCreateTag();
                    }
                  }}
                />
              </div>
              <Button 
                onClick={handleCreateTag} 
                disabled={!newTagName.trim() || createTagMutation.isPending}
              >
                {createTagMutation.isPending ? 'Creating...' : 'Create Tag'}
              </Button>
            </div>
          </DialogContent>
        </Dialog>

        {/* Images grid */}
        {data?.images && data.images.length > 0 ? (
          <>
            <p className="text-sm text-muted-foreground mb-4">
              {data.totalCount} {data.totalCount === 1 ? 'image' : 'images'} found
            </p>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
              {data.images.map((image) => (
                <Card key={image.id} className="overflow-hidden hover:shadow-lg transition-shadow">
                  <div className="relative aspect-video bg-muted">
                    <img
                      src={image.thumbnailUrl || image.url}
                      alt={image.title}
                      className="w-full h-full object-cover"
                    />
                  </div>
                  <CardContent className="p-4">
                    <h3 className="font-semibold mb-1 truncate">{image.title}</h3>
                    {image.description && (
                      <p className="text-sm text-muted-foreground mb-3 line-clamp-2">
                        {image.description}
                      </p>
                    )}
                    
                    {/* Image tags */}
                    <div className="flex flex-wrap gap-1 mb-3">
                      {image.tags.map((tag) => (
                        <Badge
                          key={tag.id}
                          variant="secondary"
                          className="text-xs"
                          style={tag.color ? { backgroundColor: tag.color + '20', color: tag.color } : {}}
                        >
                          {tag.name}
                          <button
                            onClick={() => handleRemoveTag(image.id, tag.id)}
                            className="ml-1 hover:text-destructive"
                          >
                            <X className="h-3 w-3" />
                          </button>
                        </Badge>
                      ))}
                    </div>

                    {/* Add tag dialog */}
                    <Dialog>
                      <DialogTrigger asChild>
                        <Button variant="outline" size="sm" className="w-full">
                          <Plus className="h-4 w-4 mr-1" />
                          Add Tag
                        </Button>
                      </DialogTrigger>
                      <DialogContent>
                        <DialogHeader>
                          <DialogTitle>Add Tag to {image.title}</DialogTitle>
                          <DialogDescription>
                            Select a tag to add to this image.
                          </DialogDescription>
                        </DialogHeader>
                        <div className="flex flex-wrap gap-2 max-h-64 overflow-y-auto">
                          {tags
                            ?.filter((tag) => !image.tags.find((t) => t.id === tag.id))
                            .map((tag) => (
                              <Badge
                                key={tag.id}
                                variant="outline"
                                className="cursor-pointer hover:bg-accent"
                                style={tag.color ? { borderColor: tag.color } : {}}
                                onClick={() => handleAddTag(image.id, tag)}
                              >
                                {tag.name}
                              </Badge>
                            ))}
                          {tags?.every((tag) => image.tags.find((t) => t.id === tag.id)) && (
                            <p className="text-sm text-muted-foreground">
                              All tags are already added to this image.
                            </p>
                          )}
                        </div>
                      </DialogContent>
                    </Dialog>
                  </CardContent>
                </Card>
              ))}
            </div>
          </>
        ) : (
          <Card>
            <CardContent className="py-12 text-center">
              <p className="text-muted-foreground mb-4">
                {selectedTag ? `No images found with tag "${selectedTag}"` : 'No images found'}
              </p>
              {selectedTag && (
                <Button onClick={() => setSelectedTag(undefined)} variant="outline">
                  Clear Filter
                </Button>
              )}
            </CardContent>
          </Card>
        )}
      </main>
    </div>
  );
}
