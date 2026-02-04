'use client';

import { useState } from 'react';
import { useItems, useTags, useAddTagToItem, useRemoveTagFromItem, useCreateTag } from '@/lib/hooks/useApi';
import { MediaType } from '@/lib/api/types';
import { Card, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog';
import { Label } from '@/components/ui/label';
import { Plus, X, Tag as TagIcon, Image as ImageIcon, Video as VideoIcon, Music as MusicIcon, File as FileIcon } from 'lucide-react';
import { cn } from '@/lib/utils';
import { AuthenticatedImage } from '@/components/ui/authenticated-image';

export default function ItemsPage() {
  const [selectedMediaType, setSelectedMediaType] = useState<MediaType | undefined>();
  const [selectedTagId, setSelectedTagId] = useState<string | undefined>();
  const [newTagName, setNewTagName] = useState('');
  const [isAddingTag, setIsAddingTag] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);

  const { data, isLoading, error } = useItems({
    type: selectedMediaType,
    tagId: selectedTagId,
    page: currentPage,
    pageSize: 50
  });

  const { data: tagsData } = useTags();
  const addTagMutation = useAddTagToItem();
  const removeTagMutation = useRemoveTagFromItem();
  const createTagMutation = useCreateTag();

  const handleAddTag = (provider: string, itemId: string, tagId: string) => {
    addTagMutation.mutate({ provider, itemId, tagId });
  };

  const handleRemoveTag = (provider: string, itemId: string, tagId: string) => {
    removeTagMutation.mutate({ provider, itemId, tagId });
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

  const getMediaTypeIcon = (type: MediaType) => {
    switch (type) {
      case MediaType.Image:
        return <ImageIcon className="h-4 w-4" />;
      case MediaType.Video:
        return <VideoIcon className="h-4 w-4" />;
      case MediaType.Audio:
        return <MusicIcon className="h-4 w-4" />;
      default:
        return <FileIcon className="h-4 w-4" />;
    }
  };

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <p>Loading items...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <p className="text-destructive">Error loading items</p>
      </div>
    );
  }

  const totalPages = data ? Math.ceil(data.totalCount / 50) : 0;

  return (
    <div className="min-h-screen bg-background">
      <header className="border-b">
        <div className="container mx-auto px-4 py-4">
          <h1 className="text-2xl font-bold">Media Items</h1>
        </div>
      </header>

      <main className="container mx-auto px-4 py-8">
        {/* Media Type Filter */}
        <div className="mb-6">
          <div className="flex items-center gap-2 mb-3">
            <FileIcon className="h-5 w-5 text-muted-foreground" />
            <h2 className="text-sm font-semibold text-muted-foreground">Media Type</h2>
          </div>

          <div className="flex flex-wrap gap-2">
            <Badge
              variant={selectedMediaType === undefined ? 'default' : 'outline'}
              className="cursor-pointer"
              onClick={() => {
                setSelectedMediaType(undefined);
                setCurrentPage(1);
              }}
            >
              All
            </Badge>
            <Badge
              variant={selectedMediaType === MediaType.Image ? 'default' : 'outline'}
              className="cursor-pointer"
              onClick={() => {
                setSelectedMediaType(selectedMediaType === MediaType.Image ? undefined : MediaType.Image);
                setCurrentPage(1);
              }}
            >
              <ImageIcon className="h-3 w-3 mr-1" />
              Images
            </Badge>
            <Badge
              variant={selectedMediaType === MediaType.Video ? 'default' : 'outline'}
              className="cursor-pointer"
              onClick={() => {
                setSelectedMediaType(selectedMediaType === MediaType.Video ? undefined : MediaType.Video);
                setCurrentPage(1);
              }}
            >
              <VideoIcon className="h-3 w-3 mr-1" />
              Videos
            </Badge>
            <Badge
              variant={selectedMediaType === MediaType.Audio ? 'default' : 'outline'}
              className="cursor-pointer"
              onClick={() => {
                setSelectedMediaType(selectedMediaType === MediaType.Audio ? undefined : MediaType.Audio);
                setCurrentPage(1);
              }}
            >
              <MusicIcon className="h-3 w-3 mr-1" />
              Audio
            </Badge>
          </div>
        </div>

        {/* Tag Filter */}
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
              variant={selectedTagId === undefined ? 'default' : 'outline'}
              className="cursor-pointer"
              onClick={() => {
                setSelectedTagId(undefined);
                setCurrentPage(1);
              }}
            >
              All
            </Badge>
            {tagsData?.tags?.map((tag) => (
              <Badge
                key={tag.id}
                variant={selectedTagId === tag.id ? 'default' : 'outline'}
                className="cursor-pointer"
                onClick={() => {
                  setSelectedTagId(selectedTagId === tag.id ? undefined : tag.id);
                  setCurrentPage(1);
                }}
              >
                {tag.name} ({tag.itemCount})
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
                Add a new tag to organize your items.
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

        {/* Items grid */}
        {data?.items && data.items.length > 0 ? (
          <>
            <p className="text-sm text-muted-foreground mb-4">
              {data.totalCount} {data.totalCount === 1 ? 'item' : 'items'} found
            </p>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
              {data.items.map((item) => (
                <Card key={`${item.provider}-${item.id}`} className="overflow-hidden hover:shadow-lg transition-shadow">
                  <div className="relative aspect-video bg-muted">
                    {item.thumbnailUrl ? (
                      <AuthenticatedImage
                        src={item.thumbnailUrl}
                        alt={item.name}
                        className="w-full h-full object-cover"
                        fallback={
                          <div className="w-full h-full flex items-center justify-center text-muted-foreground">
                            {getMediaTypeIcon(item.type)}
                          </div>
                        }
                      />
                    ) : (
                      <div className="w-full h-full flex items-center justify-center text-muted-foreground">
                        {getMediaTypeIcon(item.type)}
                      </div>
                    )}
                    <Badge
                      variant="secondary"
                      className="absolute top-2 right-2"
                    >
                      {getMediaTypeIcon(item.type)}
                      <span className="ml-1">{item.type}</span>
                    </Badge>
                  </div>
                  <CardContent className="p-4">
                    <h3 className="font-semibold mb-1 truncate" title={item.name}>
                      {item.name}
                    </h3>
                    <p className="text-xs text-muted-foreground mb-3">
                      {new Date(item.createdAt).toLocaleDateString('en-GB', {
                        day: '2-digit',
                        month: '2-digit',
                        year: 'numeric'
                      })}
                    </p>

                    {/* Item tags */}
                    <div className="flex flex-wrap gap-1 mb-3 min-h-[24px]">
                      {item.tags.map((tag) => (
                        <Badge
                          key={tag.id}
                          variant="secondary"
                          className="text-xs"
                        >
                          {tag.name}
                          <button
                            onClick={() => handleRemoveTag(item.provider, item.id, tag.id)}
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
                          <DialogTitle>Add Tag to {item.name}</DialogTitle>
                          <DialogDescription>
                            Select a tag to add to this item.
                          </DialogDescription>
                        </DialogHeader>
                        <div className="flex flex-wrap gap-2 max-h-64 overflow-y-auto">
                          {tagsData?.tags
                            ?.filter((tag) => !item.tags.find((t) => t.id === tag.id))
                            .map((tag) => (
                              <Badge
                                key={tag.id}
                                variant="outline"
                                className="cursor-pointer hover:bg-accent"
                                onClick={() => handleAddTag(item.provider, item.id, tag.id)}
                              >
                                {tag.name}
                              </Badge>
                            ))}
                          {tagsData?.tags?.every((tag) => item.tags.find((t) => t.id === tag.id)) && (
                            <p className="text-sm text-muted-foreground">
                              All tags are already added to this item.
                            </p>
                          )}
                        </div>
                      </DialogContent>
                    </Dialog>
                  </CardContent>
                </Card>
              ))}
            </div>

            {/* Pagination */}
            {totalPages > 1 && (
              <div className="mt-8 flex items-center justify-center gap-2">
                <Button
                  variant="outline"
                  onClick={() => setCurrentPage(p => Math.max(1, p - 1))}
                  disabled={currentPage === 1}
                >
                  Previous
                </Button>
                <span className="text-sm text-muted-foreground">
                  Page {currentPage} of {totalPages}
                </span>
                <Button
                  variant="outline"
                  onClick={() => setCurrentPage(p => Math.min(totalPages, p + 1))}
                  disabled={currentPage === totalPages}
                >
                  Next
                </Button>
              </div>
            )}
          </>
        ) : (
          <Card>
            <CardContent className="py-12 text-center">
              <p className="text-muted-foreground mb-4">
                {selectedMediaType || selectedTagId ? 'No items found with the selected filters' : 'No items found'}
              </p>
              {(selectedMediaType || selectedTagId) && (
                <Button
                  onClick={() => {
                    setSelectedMediaType(undefined);
                    setSelectedTagId(undefined);
                    setCurrentPage(1);
                  }}
                  variant="outline"
                >
                  Clear Filters
                </Button>
              )}
            </CardContent>
          </Card>
        )}
      </main>
    </div>
  );
}
