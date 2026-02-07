'use client';

import { useState, useMemo, useEffect, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import { useInfiniteItems, useTags, useAddTagToItem, useRemoveTagFromItem, useCreateTag } from '@/lib/hooks/useApi';
import { MediaType } from '@/lib/api/types';
import { Card, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { ChevronLeft, ChevronRight, Tag as TagIcon, Image as ImageIcon, Video as VideoIcon, Music as MusicIcon, File as FileIcon } from 'lucide-react';
import { cn } from '@/lib/utils';
import { AuthenticatedImage } from '@/components/ui/authenticated-image';
import { useToast } from '@/hooks/use-toast';

export default function TaggingModePage() {
  const [currentIndex, setCurrentIndex] = useState(0);
  const [newTagName, setNewTagName] = useState('');
  const { toast } = useToast();
  const router = useRouter();

  const {
    data,
    isLoading,
    error,
    fetchNextPage,
    hasNextPage,
  } = useInfiniteItems({
    type: MediaType.Image, // Focus on images for tagging mode
  });

  const { data: tagsData } = useTags();
  const addTagMutation = useAddTagToItem();
  const removeTagMutation = useRemoveTagFromItem();
  const createTagMutation = useCreateTag();

  const allItems = useMemo(
    () => data?.pages.flatMap(page => page.items) ?? [],
    [data]
  );

  const currentItem = allItems[currentIndex];
  const totalCount = data?.pages[0]?.totalCount ?? 0;

  // Sort tags by most recently used (based on item tags first, then all tags)
  const sortedTags = useMemo(() => {
    if (!tagsData?.tags) return [];
    
    const currentItemTagIds = new Set(currentItem?.tags.map(t => t.id) ?? []);
    const tagged = tagsData.tags.filter(tag => currentItemTagIds.has(tag.id));
    const untagged = tagsData.tags.filter(tag => !currentItemTagIds.has(tag.id));
    
    return [...tagged, ...untagged];
  }, [tagsData, currentItem]);

  const handlePrevious = useCallback(() => {
    if (currentIndex > 0) {
      setCurrentIndex(currentIndex - 1);
    }
  }, [currentIndex]);

  const handleNext = useCallback(() => {
    if (currentIndex < allItems.length - 1) {
      setCurrentIndex(currentIndex + 1);
      
      // Prefetch next page if approaching the end
      if (currentIndex >= allItems.length - 5 && hasNextPage) {
        fetchNextPage();
      }
    }
  }, [currentIndex, allItems.length, hasNextPage, fetchNextPage]);

  const handleToggleTag = async (tagId: string) => {
    if (!currentItem) return;
    
    const hasTag = currentItem.tags.some(t => t.id === tagId);
    
    try {
      if (hasTag) {
        await removeTagMutation.mutateAsync({ 
          provider: currentItem.provider, 
          itemId: currentItem.id, 
          tagId 
        });
      } else {
        await addTagMutation.mutateAsync({ 
          provider: currentItem.provider, 
          itemId: currentItem.id, 
          tagId 
        });
      }
    } catch (error) {
      toast(hasTag ? 'Failed to remove tag' : 'Failed to add tag', {
        description: error instanceof Error ? error.message : 'An error occurred while updating the tag',
      });
    }
  };

  const handleCreateTag = async () => {
    if (!newTagName.trim()) return;
    
    try {
      const result = await createTagMutation.mutateAsync({ name: newTagName.trim() });
      setNewTagName('');
      
      // Auto-add the newly created tag to current item
      if (currentItem && result?.id) {
        await addTagMutation.mutateAsync({
          provider: currentItem.provider,
          itemId: currentItem.id,
          tagId: result.id,
        });
      }
      
      toast('Tag created', {
        description: `"${newTagName}" has been created and added.`,
      });
    } catch (error) {
      toast('Failed to create tag', {
        description: error instanceof Error ? error.message : 'An error occurred while creating the tag',
      });
    }
  };

  const getMediaTypeIcon = (type: MediaType) => {
    switch (type) {
      case MediaType.Image:
        return <ImageIcon className="h-5 w-5" />;
      case MediaType.Video:
        return <VideoIcon className="h-5 w-5" />;
      case MediaType.Audio:
        return <MusicIcon className="h-5 w-5" />;
      default:
        return <FileIcon className="h-5 w-5" />;
    }
  };

  // Keyboard navigation
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.target instanceof HTMLInputElement) return; // Don't interfere with input fields
      
      if (e.key === 'ArrowLeft') {
        e.preventDefault();
        handlePrevious();
      } else if (e.key === 'ArrowRight') {
        e.preventDefault();
        handleNext();
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [handleNext, handlePrevious]);

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

  if (!currentItem || allItems.length === 0) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center">
          <p className="text-muted-foreground mb-4">No items found for tagging</p>
          <Button onClick={() => router.push('/items')}>
            Go to Items
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-background">
      {/* Header */}
      <div className="border-b bg-card">
        <div className="container mx-auto px-4 py-4">
          <div className="flex items-center justify-between">
            <h1 className="text-2xl font-bold">Tagging Mode</h1>
            <div className="flex items-center gap-4">
              <span className="text-sm text-muted-foreground">
                {currentIndex + 1} of {totalCount}
              </span>
              <div className="flex gap-2">
                <Button
                  variant="outline"
                  size="sm"
                  onClick={handlePrevious}
                  disabled={currentIndex === 0}
                >
                  <ChevronLeft className="h-4 w-4 mr-1" />
                  Previous
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={handleNext}
                  disabled={currentIndex >= allItems.length - 1}
                >
                  Next
                  <ChevronRight className="h-4 w-4 ml-1" />
                </Button>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Main Content */}
      <div className="container mx-auto px-4 py-6">
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Image Display - Takes 2/3 on large screens */}
          <div className="lg:col-span-2">
            <Card className="overflow-hidden">
              <CardContent className="p-0">
                <div className="relative aspect-video bg-muted">
                  {currentItem.thumbnailUrl ? (
                    <AuthenticatedImage
                      src={currentItem.thumbnailUrl}
                      alt={currentItem.name}
                      className="w-full h-full object-contain"
                      fallback={
                        <div className="w-full h-full flex items-center justify-center text-muted-foreground">
                          {getMediaTypeIcon(currentItem.type)}
                        </div>
                      }
                    />
                  ) : (
                    <div className="w-full h-full flex items-center justify-center text-muted-foreground">
                      {getMediaTypeIcon(currentItem.type)}
                    </div>
                  )}
                </div>
                <div className="p-4">
                  <div className="flex items-center justify-between mb-2">
                    <h2 className="text-lg font-semibold">{currentItem.name}</h2>
                    <Badge variant="secondary">
                      {getMediaTypeIcon(currentItem.type)}
                      <span className="ml-1">{currentItem.type}</span>
                    </Badge>
                  </div>
                  <p className="text-sm text-muted-foreground">
                    Created: {new Date(currentItem.createdAt).toLocaleDateString('en-GB', {
                      day: '2-digit',
                      month: '2-digit',
                      year: 'numeric',
                      hour: '2-digit',
                      minute: '2-digit',
                    })}
                  </p>
                </div>
              </CardContent>
            </Card>
          </div>

          {/* Tag Sidebar - Takes 1/3 on large screens */}
          <div className="lg:col-span-1">
            <Card>
              <CardContent className="p-4">
                <div className="space-y-4">
                  {/* Create New Tag */}
                  <div>
                    <Label htmlFor="new-tag" className="text-sm font-semibold mb-2 flex items-center gap-2">
                      <TagIcon className="h-4 w-4" />
                      Create New Tag
                    </Label>
                    <div className="flex gap-2 mt-2">
                      <Input
                        id="new-tag"
                        placeholder="Enter tag name..."
                        value={newTagName}
                        onChange={(e) => setNewTagName(e.target.value)}
                        onKeyDown={(e) => {
                          if (e.key === 'Enter') {
                            handleCreateTag();
                          }
                        }}
                        className="flex-1"
                      />
                    </div>
                    <p className="text-xs text-muted-foreground mt-1">
                      Press Enter to create and add tag
                    </p>
                  </div>

                  {/* Current Item Tags */}
                  {currentItem.tags.length > 0 && (
                    <div>
                      <Label className="text-sm font-semibold mb-2 block">
                        Current Tags
                      </Label>
                      <div className="flex flex-wrap gap-2">
                        {currentItem.tags.map((tag) => (
                          <Badge
                            key={tag.id}
                            variant="default"
                            className="cursor-pointer"
                            onClick={() => handleToggleTag(tag.id)}
                          >
                            {tag.name}
                          </Badge>
                        ))}
                      </div>
                    </div>
                  )}

                  {/* All Available Tags */}
                  <div>
                    <Label className="text-sm font-semibold mb-2 block">
                      Available Tags
                    </Label>
                    <div className="space-y-2 max-h-96 overflow-y-auto">
                      {sortedTags.map((tag) => {
                        const isSelected = currentItem.tags.some(t => t.id === tag.id);
                        return (
                          <div
                            key={tag.id}
                            className={cn(
                              "flex items-center gap-2 p-2 rounded-md cursor-pointer transition-colors",
                              isSelected 
                                ? "bg-primary text-primary-foreground hover:bg-primary/90" 
                                : "bg-muted hover:bg-muted/80"
                            )}
                            onClick={() => handleToggleTag(tag.id)}
                          >
                            <div className={cn(
                              "w-4 h-4 rounded border-2 flex items-center justify-center",
                              isSelected 
                                ? "border-primary-foreground bg-primary-foreground" 
                                : "border-muted-foreground"
                            )}>
                              {isSelected && (
                                <div className="w-2 h-2 bg-primary rounded-sm" />
                              )}
                            </div>
                            <span className="flex-1 text-sm">{tag.name}</span>
                            <Badge variant="outline" className={cn(
                              "text-xs",
                              isSelected && "border-primary-foreground text-primary-foreground"
                            )}>
                              {tag.itemCount}
                            </Badge>
                          </div>
                        );
                      })}
                      {sortedTags.length === 0 && (
                        <p className="text-sm text-muted-foreground text-center py-4">
                          No tags available. Create one above!
                        </p>
                      )}
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>
        </div>

        {/* Navigation Hint */}
        <div className="mt-6 text-center">
          <p className="text-sm text-muted-foreground">
            Use ← → arrow keys to navigate between items
          </p>
        </div>
      </div>
    </div>
  );
}
