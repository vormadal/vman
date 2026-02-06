'use client';

import { useState, useMemo, useEffect, useCallback } from 'react';
import { useInfiniteItems, useTags, useAddTagToItem, useRemoveTagFromItem, useCreateTag } from '@/lib/hooks/useApi';
import { Card, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { ChevronLeft, ChevronRight, Tag as TagIcon, X, Plus, ArrowLeft, Filter } from 'lucide-react';
import { AuthenticatedImage } from '@/components/ui/authenticated-image';
import { useToast } from '@/hooks/use-toast';
import Link from 'next/link';

const PREFETCH_THRESHOLD = 5; // Trigger next page load when this many items from the end

export default function TaggingModePage() {
  const [currentIndex, setCurrentIndex] = useState(0);
  const [newTagName, setNewTagName] = useState('');
  const [tagSearch, setTagSearch] = useState('');
  const [showOnlyUntagged, setShowOnlyUntagged] = useState(true);

  const {
    data,
    isLoading,
    error,
    fetchNextPage,
    hasNextPage,
  } = useInfiniteItems({
    untagged: showOnlyUntagged ? true : undefined,
    sortBy: showOnlyUntagged ? undefined : 'tagCount',
    sortDescending: false,
  });

  const { data: tagsData } = useTags();
  const addTagMutation = useAddTagToItem();
  const removeTagMutation = useRemoveTagFromItem();
  const createTagMutation = useCreateTag();
  const { toast } = useToast();

  const allItems = useMemo(
    () => data?.pages.flatMap(page => page.items) ?? [],
    [data]
  );

  const totalCount = data?.pages[0]?.totalCount ?? 0;
  const currentItem = allItems[currentIndex];

  // Sort tags by most recent first
  const sortedTags = useMemo(() => {
    if (!tagsData?.tags) return [];
    return [...tagsData.tags].sort((a, b) => 
      new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
    );
  }, [tagsData]);

  // Filter tags based on search
  const filteredTags = useMemo(() => {
    if (!tagSearch.trim()) return sortedTags;
    return sortedTags.filter(tag => 
      tag.name.toLowerCase().includes(tagSearch.toLowerCase())
    );
  }, [sortedTags, tagSearch]);

  const handlePrevious = useCallback(() => {
    if (currentIndex > 0) {
      setCurrentIndex(prev => prev - 1);
    }
  }, [currentIndex]);

  const handleNext = useCallback(() => {
    setCurrentIndex(prevIndex => {
      const lastLoadedIndex = allItems.length - 1;

      // If we still have a next loaded item, advance to it
      if (prevIndex < lastLoadedIndex) {
        const nextIndex = prevIndex + 1;

        // Fetch next page when approaching the end of loaded items
        if (nextIndex >= allItems.length - PREFETCH_THRESHOLD && hasNextPage) {
          fetchNextPage();
        }

        return nextIndex;
      }

      // We're at the last loaded item; if more pages are available, trigger a fetch
      if (hasNextPage) {
        fetchNextPage();
      }

      // Stay on the current index until new items have been loaded
      return prevIndex;
    });
  }, [allItems.length, hasNextPage, fetchNextPage]);

  const handleAddTag = (tagId: string) => {
    if (!currentItem) return;
    
    // Check if tag is already added
    if (currentItem.tags.some(t => t.id === tagId)) {
      toast.error("Tag already added");
      return;
    }
    
    addTagMutation.mutate(
      { provider: currentItem.provider, itemId: currentItem.id, tagId },
      {
        onSuccess: () => {
          toast.success("Tag added");
        },
      }
    );
  };

  const handleRemoveTag = (tagId: string) => {
    if (!currentItem) return;
    removeTagMutation.mutate({ 
      provider: currentItem.provider, 
      itemId: currentItem.id, 
      tagId 
    });
  };

  const handleCreateTag = () => {
    if (!newTagName.trim()) return;
    
    // Capture current item details to avoid race condition
    const targetProvider = currentItem?.provider;
    const targetItemId = currentItem?.id;
    
    createTagMutation.mutate(
      { name: newTagName.trim() },
      {
        onSuccess: (tag) => {
          setNewTagName('');
          toast.success("Tag created");
          // Auto-add the newly created tag to the captured item
          if (targetProvider && targetItemId) {
            addTagMutation.mutate({
              provider: targetProvider,
              itemId: targetItemId,
              tagId: tag.id,
            });
          }
        },
      }
    );
  };

  // Keyboard navigation
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      // Don't trigger navigation if user is typing in an input
      if (e.target instanceof HTMLInputElement || e.target instanceof HTMLTextAreaElement) {
        return;
      }

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
  }, [handlePrevious, handleNext]);

  // Reset to first item when filter changes
  useEffect(() => {
    setCurrentIndex(0);
  }, [showOnlyUntagged]);

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

  if (allItems.length === 0) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <Card className="max-w-md">
          <CardContent className="py-12 text-center">
            <p className="text-muted-foreground mb-4">
              No items available for tagging
            </p>
            <Link href="/items">
              <Button variant="outline">
                Go to Items
              </Button>
            </Link>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-background flex flex-col">
      {/* Header */}
      <header className="border-b">
        <div className="container mx-auto px-4 py-4">
          <div className="flex justify-between items-center">
            <div className="flex items-center gap-4">
              <Link href="/items">
                <Button variant="ghost" size="sm">
                  <ArrowLeft className="h-4 w-4 mr-2" />
                  Back to Items
                </Button>
              </Link>
              <h1 className="text-2xl font-bold">Tagging Mode</h1>
            </div>
            <div className="flex items-center gap-4">
              <Button
                variant={showOnlyUntagged ? "default" : "outline"}
                size="sm"
                onClick={() => setShowOnlyUntagged(!showOnlyUntagged)}
              >
                <Filter className="h-4 w-4 mr-2" />
                {showOnlyUntagged ? 'Untagged Only' : 'All Items'}
              </Button>
              <div className="text-sm text-muted-foreground">
                {currentIndex + 1} of {totalCount}
              </div>
            </div>
          </div>
        </div>
      </header>

      {/* Main content */}
      <main className="flex-1 flex overflow-hidden">
        {/* Image viewer */}
        <div className="flex-1 flex flex-col p-4">
          <div className="flex-1 flex items-center justify-center relative">
            {currentItem && (
              <div className="relative w-full h-full flex items-center justify-center">
                {currentItem.thumbnailUrl ? (
                  <AuthenticatedImage
                    src={currentItem.previewUrl || currentItem.thumbnailUrl}
                    alt={currentItem.name}
                    className="max-w-full max-h-full object-contain"
                    fallback={
                      <div className="w-full h-full flex items-center justify-center text-muted-foreground">
                        No preview available
                      </div>
                    }
                  />
                ) : (
                  <div className="text-muted-foreground">
                    No preview available
                  </div>
                )}

                {/* Navigation arrows */}
                <Button
                  variant="outline"
                  size="icon"
                  className="absolute left-4 top-1/2 -translate-y-1/2 h-12 w-12"
                  onClick={handlePrevious}
                  disabled={currentIndex === 0}
                  aria-label="Previous item"
                >
                  <ChevronLeft className="h-6 w-6" />
                </Button>
                <Button
                  variant="outline"
                  size="icon"
                  className="absolute right-4 top-1/2 -translate-y-1/2 h-12 w-12"
                  onClick={handleNext}
                  disabled={currentIndex >= allItems.length - 1 && !hasNextPage}
                  aria-label="Next item"
                >
                  <ChevronRight className="h-6 w-6" />
                </Button>
              </div>
            )}
          </div>

          {/* Item info */}
          {currentItem && (
            <div className="mt-4 text-center">
              <h2 className="text-lg font-semibold">{currentItem.name}</h2>
              <p className="text-sm text-muted-foreground">
                {new Date(currentItem.createdAt).toLocaleDateString('en-GB', {
                  day: '2-digit',
                  month: '2-digit',
                  year: 'numeric'
                })}
                {' • '}
                {currentItem.type}
              </p>

              {/* Current tags */}
              <div className="flex flex-wrap gap-2 justify-center mt-3">
                {currentItem.tags.map((tag) => (
                  <Badge
                    key={tag.id}
                    variant="secondary"
                    className="text-sm"
                  >
                    {tag.name}
                    <button
                      onClick={() => handleRemoveTag(tag.id)}
                      className="ml-1 hover:text-destructive"
                      aria-label={`Remove ${tag.name} tag`}
                    >
                      <X className="h-3 w-3" />
                    </button>
                  </Badge>
                ))}
              </div>
            </div>
          )}
        </div>

        {/* Sidebar with tags */}
        <aside className="w-80 border-l bg-muted/30 p-4 overflow-y-auto">
          <div className="space-y-4">
            <div>
              <h3 className="text-sm font-semibold mb-2 flex items-center gap-2">
                <TagIcon className="h-4 w-4" />
                Add Tags
              </h3>
              
              {/* Create new tag */}
              <div className="mb-3">
                <Input
                  placeholder="Create new tag..."
                  value={newTagName}
                  onChange={(e) => setNewTagName(e.target.value)}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter') {
                      handleCreateTag();
                    }
                  }}
                  disabled={createTagMutation.isPending}
                />
                {newTagName.trim() && (
                  <Button
                    size="sm"
                    className="w-full mt-2"
                    onClick={handleCreateTag}
                    disabled={createTagMutation.isPending}
                  >
                    <Plus className="h-3 w-3 mr-1" />
                    Create &amp; Add &quot;{newTagName}&quot;
                  </Button>
                )}
              </div>

              {/* Search tags */}
              <Input
                placeholder="Search tags..."
                value={tagSearch}
                onChange={(e) => setTagSearch(e.target.value)}
                className="mb-3"
              />

              {/* Tags list */}
              <div className="space-y-1">
                {filteredTags.length > 0 ? (
                  filteredTags.map((tag) => {
                    const isAdded = currentItem?.tags.some(t => t.id === tag.id);
                    return (
                      <Button
                        key={tag.id}
                        variant={isAdded ? "default" : "outline"}
                        size="sm"
                        className="w-full justify-between"
                        onClick={() => handleAddTag(tag.id)}
                        disabled={isAdded || addTagMutation.isPending}
                      >
                        <span className="truncate">{tag.name}</span>
                        <span className="text-xs ml-2">
                          {tag.itemCount}
                        </span>
                      </Button>
                    );
                  })
                ) : (
                  <p className="text-sm text-muted-foreground text-center py-4">
                    {tagSearch ? 'No tags found' : 'No tags available'}
                  </p>
                )}
              </div>
            </div>
          </div>
        </aside>
      </main>
    </div>
  );
}
