'use client';

import { useState, useMemo, useEffect, useRef, useCallback } from 'react';
import { useSearchParams } from 'next/navigation';
import { useInfiniteItems, useTags, useAddTagToItem, useRemoveTagFromItem, useCreateTag, useCollections, useAddItemToCollection, useBulkAddFilteredItemsToCollection, usePeople } from '@/lib/hooks/useApi';
import { MediaType } from '@/lib/api/types';
import { Card, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog';
import { Label } from '@/components/ui/label';
import { Plus, X, Tag as TagIcon, Image as ImageIcon, Video as VideoIcon, Music as MusicIcon, File as FileIcon, FolderPlus, User as UserIcon, Tags } from 'lucide-react';
import { cn } from '@/lib/utils';
import { AuthenticatedImage } from '@/components/ui/authenticated-image';
import { useWindowVirtualizer } from '@tanstack/react-virtual';
import { useToast } from '@/hooks/use-toast';
import Link from 'next/link';
import { useCollectionModeStore } from '@/lib/store/collectionModeStore';

const MAX_DISPLAYED_BADGES = 10;

export default function ItemsPage() {
  const [selectedMediaType, setSelectedMediaType] = useState<MediaType | undefined>();
  const [selectedTagId, setSelectedTagId] = useState<string | undefined>();
  const [selectedPersonId, setSelectedPersonId] = useState<string | undefined>();
  const [newTagName, setNewTagName] = useState('');
  const [isAddingTag, setIsAddingTag] = useState(false);
  const [openDialogItemId, setOpenDialogItemId] = useState<string | null>(null);
  
  const { isActive: collectionModeActive, activeCollectionId, enterCollectionMode, exitCollectionMode } = useCollectionModeStore();
  const searchParams = useSearchParams();

  // Auto-enter collection mode when arriving via a shared link (?collection=<id>)
  useEffect(() => {
    const collectionParam = searchParams.get('collection');
    if (collectionParam && collectionParam !== activeCollectionId) {
      enterCollectionMode(collectionParam);
    }
  }, [searchParams]);

  const {
    data,
    isLoading,
    error,
    fetchNextPage,
    hasNextPage,
    isFetchingNextPage
  } = useInfiniteItems({
    type: selectedMediaType,
    tagId: selectedTagId,
    personId: selectedPersonId,
  });

  const { data: tagsData } = useTags();
  const { data: peopleData } = usePeople(undefined, 500);
  const addTagMutation = useAddTagToItem();
  const removeTagMutation = useRemoveTagFromItem();
  const createTagMutation = useCreateTag();
  const { data: collectionsData } = useCollections();
  const addToCollectionMutation = useAddItemToCollection();
  const bulkAddFilteredMutation = useBulkAddFilteredItemsToCollection();
  const { toast } = useToast();

  const hasActiveFilter = !!(selectedMediaType || selectedTagId || selectedPersonId);

  const handleBulkAddFiltered = async () => {
    if (!activeCollectionId) return;
    try {
      const result = await bulkAddFilteredMutation.mutateAsync({
        collectionId: activeCollectionId,
        params: {
          type: selectedMediaType,
          tagId: selectedTagId,
          personId: selectedPersonId,
        },
      });
      toast.success(`Added ${result.addedCount} item${result.addedCount !== 1 ? 's' : ''} to collection${result.skippedCount > 0 ? ` (${result.skippedCount} already present)` : ''}`);
    } catch (error) {
      toast.error(error instanceof Error ? error.message : 'Failed to add items to collection');
    }
  };

  const allItems = useMemo(
    () => data?.pages.flatMap(page => page.items) ?? [],
    [data]
  );

  const totalCount = data?.pages[0]?.totalCount ?? 0;

  const getColumnCount = useCallback(() => {
    if (typeof window === 'undefined') return 4;
    const width = window.innerWidth;
    if (width >= 1280) return 4; // xl
    if (width >= 1024) return 3; // lg
    if (width >= 768) return 2;  // md
    return 1;
  }, []);

  const [columnCount, setColumnCount] = useState(getColumnCount);

  useEffect(() => {
    const handleResize = () => setColumnCount(getColumnCount());
    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, [getColumnCount]);

  const rowCount = Math.ceil(allItems.length / columnCount);

  const rowVirtualizer = useWindowVirtualizer({
    count: rowCount,
    estimateSize: () => 240,
    overscan: 3,
  });

  const virtualRows = rowVirtualizer.getVirtualItems();

  const handleAddTag = (provider: string, itemId: string, tagId: string) => {
    addTagMutation.mutate({ provider, itemId, tagId });
  };

  const handleRemoveTag = (provider: string, itemId: string, tagId: string) => {
    removeTagMutation.mutate({ provider, itemId, tagId });
  };

  const handleAddToCollection = async (collectionId: string, providerName: string, providerItemId: string) => {
    try {
      await addToCollectionMutation.mutateAsync({ collectionId, providerName, providerItemId });
      toast.success('Item added to collection');
      setOpenDialogItemId(null); // Close the dialog after successful addition
    } catch (error) {
      toast.error(error instanceof Error ? error.message : 'Failed to add item to collection');
    }
  };

  const handleQuickAddToActiveCollection = async (providerName: string, providerItemId: string) => {
    if (!activeCollectionId) return;
    await handleAddToCollection(activeCollectionId, providerName, providerItemId);
  };

  const handleCreateTag = () => {
    if (!newTagName.trim()) return;

    // Check for exact match (case insensitive) in all tags
    const exactMatch = tagsData?.tags.find(
      tag => tag.name.toLowerCase() === newTagName.trim().toLowerCase()
    );

    if (exactMatch) {
      toast('Tag already exists', {
        description: `A tag named "${exactMatch.name}" already exists.`,
      });
      setNewTagName('');
      setIsAddingTag(false);
      return;
    }

    createTagMutation.mutate(
      { name: newTagName.trim() },
      {
        onSuccess: () => {
          setNewTagName('');
          setIsAddingTag(false);
        },
      }
    );
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

  useEffect(() => {
    const [lastItem] = [...virtualRows].reverse();

    if (!lastItem) return;

    if (
      lastItem.index >= rowCount - 2 &&
      hasNextPage &&
      !isFetchingNextPage
    ) {
      fetchNextPage();
    }
  }, [
    hasNextPage,
    fetchNextPage,
    isFetchingNextPage,
    virtualRows,
    rowCount,
  ]);

  useEffect(() => {
    window.scrollTo(0, 0);
  }, [selectedMediaType, selectedTagId]);

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

  return (
    <div className="container mx-auto px-4 py-4">
        <div className="flex justify-between items-center mb-6">
          <h1 className="text-2xl font-bold">Media Items</h1>
          <div className="flex gap-2">
            <Button variant="outline" asChild>
              <Link href="/items/tagging">
                <Tags className="h-4 w-4 mr-2" />
                Tagging Mode
              </Link>
            </Button>
            <Button variant="outline" asChild>
              <Link href="/collections">
                <FolderPlus className="h-4 w-4 mr-2" />
                Collections
              </Link>
            </Button>
          </div>
        </div>

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
              onClick={() => setSelectedMediaType(undefined)}
            >
              All
            </Badge>
            <Badge
              variant={selectedMediaType === MediaType.Image ? 'default' : 'outline'}
              className="cursor-pointer"
              onClick={() => setSelectedMediaType(selectedMediaType === MediaType.Image ? undefined : MediaType.Image)}
            >
              <ImageIcon className="h-3 w-3 mr-1" />
              Images
            </Badge>
            <Badge
              variant={selectedMediaType === MediaType.Video ? 'default' : 'outline'}
              className="cursor-pointer"
              onClick={() => setSelectedMediaType(selectedMediaType === MediaType.Video ? undefined : MediaType.Video)}
            >
              <VideoIcon className="h-3 w-3 mr-1" />
              Videos
            </Badge>
            <Badge
              variant={selectedMediaType === MediaType.Audio ? 'default' : 'outline'}
              className="cursor-pointer"
              onClick={() => setSelectedMediaType(selectedMediaType === MediaType.Audio ? undefined : MediaType.Audio)}
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
              onClick={() => setSelectedTagId(undefined)}
            >
              All
            </Badge>
            {tagsData?.tags?.map((tag) => (
              <Badge
                key={tag.id}
                variant={selectedTagId === tag.id ? 'default' : 'outline'}
                className="cursor-pointer"
                onClick={() => setSelectedTagId(selectedTagId === tag.id ? undefined : tag.id)}
              >
                {tag.name} ({tag.itemCount})
              </Badge>
            ))}
          </div>
        </div>

        {/* People Filter */}
        <div className="mb-6">
          <div className="flex items-center gap-2 mb-3">
            <UserIcon className="h-5 w-5 text-muted-foreground" />
            <h2 className="text-sm font-semibold text-muted-foreground">Filter by Person</h2>
          </div>

          <div className="flex flex-wrap gap-2">
            <Badge
              variant={selectedPersonId === undefined ? 'default' : 'outline'}
              className="cursor-pointer"
              onClick={() => setSelectedPersonId(undefined)}
            >
              All
            </Badge>
            {peopleData?.people?.filter(p => !p.isHidden).map((person) => (
              <Badge
                key={person.id}
                variant={selectedPersonId === person.id ? 'default' : 'outline'}
                className="cursor-pointer"
                onClick={() => setSelectedPersonId(selectedPersonId === person.id ? undefined : person.id)}
              >
                {person.name} ({person.itemCount})
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

        {/* Bulk add banner — shown in collection mode when a filter is active */}
        {collectionModeActive && activeCollectionId && hasActiveFilter && (
          <div className="mb-4 flex items-center justify-between rounded-lg border border-primary/20 bg-primary/5 px-4 py-3">
            <p className="text-sm text-muted-foreground">
              <span className="font-medium text-foreground">{totalCount}</span> item{totalCount !== 1 ? 's' : ''} match the current filter
            </p>
            <Button
              size="sm"
              onClick={handleBulkAddFiltered}
              disabled={bulkAddFilteredMutation.isPending || totalCount === 0}
            >
              <Plus className="h-4 w-4 mr-1" />
              {bulkAddFilteredMutation.isPending ? 'Adding...' : `Add all ${totalCount} to collection`}
            </Button>
          </div>
        )}

        {/* Items grid */}
        {allItems.length > 0 ? (
          <>
            <p className="text-sm text-muted-foreground mb-4">
              Showing {allItems.length} of {totalCount} {totalCount === 1 ? 'item' : 'items'}
            </p>
            <div
              style={{
                height: `${rowVirtualizer.getTotalSize()}px`,
                width: '100%',
                position: 'relative',
              }}
            >
              {virtualRows.map(virtualRow => {
                const startIndex = virtualRow.index * columnCount;
                const rowItems = allItems.slice(startIndex, startIndex + columnCount);

                return (
                  <div
                    key={virtualRow.key}
                    data-index={virtualRow.index}
                    ref={rowVirtualizer.measureElement}
                    style={{
                      position: 'absolute',
                      top: 0,
                      left: 0,
                      width: '100%',
                      transform: `translateY(${virtualRow.start}px)`,
                    }}
                  >
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-2 pb-2">
                      {rowItems.map(item => (
                        <Card key={`${item.provider}-${item.id}`} className="overflow-hidden hover:shadow-lg transition-shadow p-0 gap-0">
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
                            {/* Name + date overlay */}
                            <div className="absolute bottom-0 left-0 right-0 bg-gradient-to-t from-black/70 to-transparent px-2 pt-4 pb-1.5">
                              <div className="flex items-end justify-between gap-2">
                                <div className="pointer-events-none min-w-0">
                                  <p className="text-white/80 text-xs font-medium truncate leading-tight" title={item.name}>
                                    {item.name}
                                  </p>
                                  <p className="text-white/50 text-xs leading-tight">
                                    {new Date(item.createdAt).toLocaleDateString('en-GB', {
                                      day: '2-digit',
                                      month: '2-digit',
                                      year: 'numeric',
                                    })}
                                  </p>
                                </div>
                                {collectionModeActive && activeCollectionId && (
                                  <button
                                    onClick={() => handleQuickAddToActiveCollection(item.provider, item.id)}
                                    disabled={addToCollectionMutation.isPending}
                                    className="shrink-0 bg-white/20 hover:bg-white/40 disabled:opacity-50 rounded-full p-1.5 transition-colors"
                                    title="Add to collection"
                                  >
                                    <Plus className="h-4 w-4 text-white" />
                                  </button>
                                )}
                              </div>
                            </div>
                          </div>
                          {!collectionModeActive && (
                          <CardContent className="p-0">

                            {/* Item tags and people */}
                            {(() => {
                              const combinedTagsAndPeople = [
                                ...item.tags.map(tag => ({ type: 'tag' as const, data: tag })),
                                ...((item.people || []).map(person => ({ type: 'person' as const, data: person })))
                              ].slice(0, MAX_DISPLAYED_BADGES);

                              return combinedTagsAndPeople.length > 0 ? (
                                <div className="flex flex-wrap gap-1 px-2 pt-1.5 pb-1">
                                  {combinedTagsAndPeople.map((entry) =>
                                    entry.type === 'tag' ? (
                                      <Badge
                                        key={`tag-${entry.data.id}`}
                                        variant="secondary"
                                        className="text-xs"
                                      >
                                        <TagIcon className="h-3 w-3 mr-1" />
                                        {entry.data.name}
                                        <button
                                          onClick={() => handleRemoveTag(item.provider, item.id, entry.data.id)}
                                          className="ml-1 hover:text-destructive"
                                        >
                                          <X className="h-3 w-3" />
                                        </button>
                                      </Badge>
                                    ) : (
                                      <Badge
                                        key={`person-${entry.data.id}`}
                                        variant="outline"
                                        className="text-xs"
                                      >
                                        <UserIcon className="h-3 w-3 mr-1" />
                                        {entry.data.name}
                                      </Badge>
                                    )
                                  )}
                                </div>
                              ) : null;
                            })()}

                            {/* Action buttons */}
                            <div className="flex flex-col sm:flex-row gap-1.5 px-2 py-1.5">
                              {(
                                <>
                                  {/* Universal Add Tag dialog */}
                                  <Dialog
                                    open={openDialogItemId === `tag-${item.provider}-${item.id}`}
                                    onOpenChange={(open) => setOpenDialogItemId(open ? `tag-${item.provider}-${item.id}` : null)}
                                  >
                                    <DialogTrigger asChild>
                                      <Button variant="outline" size="sm" className="flex-1">
                                        <Plus className="h-4 w-4 mr-1" />
                                        Add Tag
                                      </Button>
                                    </DialogTrigger>
                                    <DialogContent>
                                      <DialogHeader>
                                        <DialogTitle>Add Tag</DialogTitle>
                                        <DialogDescription>
                                          Select a tag to add to {item.name}.
                                        </DialogDescription>
                                      </DialogHeader>
                                      <div className="space-y-2 max-h-64 overflow-y-auto">
                                        {tagsData?.tags && tagsData.tags.length > 0 ? (
                                          tagsData.tags
                                            .filter(tag => !item.tags.some(t => t.id === tag.id))
                                            .map((tag) => (
                                              <Button
                                                key={tag.id}
                                                variant="outline"
                                                className="w-full justify-start"
                                                onClick={() => {
                                                  handleAddTag(item.provider, item.id, tag.id);
                                                  setOpenDialogItemId(null);
                                                }}
                                              >
                                                <TagIcon className="h-4 w-4 mr-2" />
                                                {tag.name}
                                                <span className="ml-auto text-xs text-muted-foreground">
                                                  {tag.itemCount} items
                                                </span>
                                              </Button>
                                            ))
                                        ) : (
                                          <div className="text-center py-4">
                                            <p className="text-sm text-muted-foreground mb-3">
                                              No tags available. Create one first.
                                            </p>
                                            <Button variant="outline" size="sm" onClick={() => setIsAddingTag(true)}>
                                              Create Tag
                                            </Button>
                                          </div>
                                        )}
                                      </div>
                                    </DialogContent>
                                  </Dialog>

                                  {/* Universal Add to Collection dialog */}
                                  <Dialog
                                    open={openDialogItemId === `collection-${item.provider}-${item.id}`}
                                    onOpenChange={(open) => setOpenDialogItemId(open ? `collection-${item.provider}-${item.id}` : null)}
                                  >
                                    <DialogTrigger asChild>
                                      <Button variant="outline" size="sm" className="flex-1">
                                        <FolderPlus className="h-4 w-4 mr-1" />
                                        Add to Collection
                                      </Button>
                                    </DialogTrigger>
                                    <DialogContent>
                                      <DialogHeader>
                                        <DialogTitle>Add to Collection</DialogTitle>
                                        <DialogDescription>
                                          Select a collection to add {item.name} to.
                                        </DialogDescription>
                                      </DialogHeader>
                                      <div className="space-y-2 max-h-64 overflow-y-auto">
                                        {collectionsData?.collections && collectionsData.collections.length > 0 ? (
                                          collectionsData.collections.map((collection) => (
                                            <Button
                                              key={collection.id}
                                              variant="outline"
                                              className="w-full justify-start"
                                              onClick={() => handleAddToCollection(collection.id, item.provider, item.id)}
                                            >
                                              <FolderPlus className="h-4 w-4 mr-2" />
                                              {collection.name}
                                              <span className="ml-auto text-xs text-muted-foreground">
                                                {collection.itemCount} items
                                              </span>
                                            </Button>
                                          ))
                                        ) : (
                                          <div className="text-center py-4">
                                            <p className="text-sm text-muted-foreground mb-3">
                                              No collections yet. Create one first.
                                            </p>
                                            <Link href="/collections">
                                              <Button variant="outline" size="sm">
                                                Go to Collections
                                              </Button>
                                            </Link>
                                          </div>
                                        )}
                                      </div>
                                    </DialogContent>
                                  </Dialog>
                                </>
                              )}
                            </div>
                          </CardContent>
                          )}
                        </Card>
                      ))}
                    </div>
                  </div>
                );
              })}
            </div>

            {/* Loading indicator */}
            {isFetchingNextPage && (
              <div className="mt-8 text-center">
                <p className="text-sm text-muted-foreground">Loading more items...</p>
              </div>
            )}

            {!hasNextPage && allItems.length > 0 && (
              <div className="mt-8 text-center">
                <p className="text-sm text-muted-foreground">All items loaded</p>
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
                  }}
                  variant="outline"
                >
                  Clear Filters
                </Button>
              )}
            </CardContent>
          </Card>
        )}
    </div>
  );
}
