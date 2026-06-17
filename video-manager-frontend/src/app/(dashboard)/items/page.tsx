'use client';

import { useState, useMemo, useEffect, useCallback, useRef } from 'react';
import { useSearchParams } from 'next/navigation';
import { useInfiniteItems, useTags, useRemoveTagFromItem, useCreateTag, useAddItemToCollection, useBulkAddFilteredItemsToCollection, usePeople } from '@/lib/hooks/useApi';
import { MediaType } from '@/lib/api/types';
import { Card, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Label } from '@/components/ui/label';
import { Plus, X, Tag as TagIcon, Image as ImageIcon, Video as VideoIcon, Music as MusicIcon, File as FileIcon, FolderPlus, User as UserIcon, Tags, Search, Check } from 'lucide-react';
import { cn } from '@/lib/utils';
import { AuthenticatedImage } from '@/components/ui/authenticated-image';
import { useWindowVirtualizer } from '@tanstack/react-virtual';
import { useToast } from '@/hooks/use-toast';
import Link from 'next/link';
import { useCollectionModeStore } from '@/lib/store/collectionModeStore';

const MAX_DISPLAYED_BADGES = 10;

const MEDIA_TYPE_OPTIONS = [
  { type: MediaType.Image, label: 'Images' },
  { type: MediaType.Video, label: 'Videos' },
  { type: MediaType.Audio, label: 'Audio' },
];

export default function ItemsPage() {
  const [selectedMediaType, setSelectedMediaType] = useState<MediaType | undefined>();
  const [selectedTagId, setSelectedTagId] = useState<string | undefined>();
  const [selectedPersonId, setSelectedPersonId] = useState<string | undefined>();
  const [newTagName, setNewTagName] = useState('');
  const [isAddingTag, setIsAddingTag] = useState(false);
  const [filterSearch, setFilterSearch] = useState('');
  const [filterDropdownOpen, setFilterDropdownOpen] = useState(false);
  const filterInputRef = useRef<HTMLInputElement>(null);
  const filterContainerRef = useRef<HTMLDivElement>(null);

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
  const removeTagMutation = useRemoveTagFromItem();
  const createTagMutation = useCreateTag();
  const addToCollectionMutation = useAddItemToCollection();
  const bulkAddFilteredMutation = useBulkAddFilteredItemsToCollection();
  const { toast } = useToast();

  const hasActiveFilter = !!(selectedMediaType || selectedTagId || selectedPersonId);

  const filteredMediaTypes = MEDIA_TYPE_OPTIONS.filter(opt =>
    !filterSearch || opt.label.toLowerCase().includes(filterSearch.toLowerCase())
  );

  const filteredTags = (tagsData?.tags ?? [])
    .filter(tag => !filterSearch || tag.name.toLowerCase().includes(filterSearch.toLowerCase()))
    .sort((a, b) => b.itemCount - a.itemCount)
    .slice(0, filterSearch ? 10 : 3);

  const filteredPeople = (peopleData?.people ?? [])
    .filter(p => !p.isHidden)
    .filter(p => !filterSearch || (p.name ?? '').toLowerCase().includes(filterSearch.toLowerCase()))
    .sort((a, b) => b.itemCount - a.itemCount)
    .slice(0, filterSearch ? 10 : 3);

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

  const handleRemoveTag = (provider: string, itemId: string, tagId: string) => {
    removeTagMutation.mutate({ provider, itemId, tagId });
  };

  const handleAddToCollection = async (collectionId: string, providerName: string, providerItemId: string) => {
    try {
      await addToCollectionMutation.mutateAsync({ collectionId, providerName, providerItemId });
      toast.success('Item added to collection');
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

        {/* Filter bar */}
        <div
          className="mb-6"
          ref={filterContainerRef}
          onBlurCapture={(e) => {
            if (!e.currentTarget.contains(e.relatedTarget as Node | null)) {
              setFilterDropdownOpen(false);
              setFilterSearch('');
            }
          }}
        >
          <div className="flex flex-wrap items-center gap-2">
            {/* Active filter badges */}
            {selectedMediaType && (
              <Badge className="gap-1 pl-2 pr-1 h-7">
                {getMediaTypeIcon(selectedMediaType)}
                <span className="ml-1">{MEDIA_TYPE_OPTIONS.find(o => o.type === selectedMediaType)?.label ?? selectedMediaType}</span>
                <button
                  onClick={() => setSelectedMediaType(undefined)}
                  className="ml-1 hover:opacity-70"
                  aria-label="Remove media type filter"
                >
                  <X className="h-3 w-3" />
                </button>
              </Badge>
            )}
            {selectedTagId && (() => {
              const tag = tagsData?.tags.find(t => t.id === selectedTagId);
              return tag ? (
                <Badge className="gap-1 pl-2 pr-1 h-7">
                  <TagIcon className="h-3 w-3" />
                  <span className="ml-1">{tag.name}</span>
                  <button
                    onClick={() => setSelectedTagId(undefined)}
                    className="ml-1 hover:opacity-70"
                    aria-label="Remove tag filter"
                  >
                    <X className="h-3 w-3" />
                  </button>
                </Badge>
              ) : null;
            })()}
            {selectedPersonId && (() => {
              const person = peopleData?.people?.find(p => p.id === selectedPersonId);
              return person ? (
                <Badge className="gap-1 pl-2 pr-1 h-7">
                  <UserIcon className="h-3 w-3" />
                  <span className="ml-1">{person.name}</span>
                  <button
                    onClick={() => setSelectedPersonId(undefined)}
                    className="ml-1 hover:opacity-70"
                    aria-label="Remove person filter"
                  >
                    <X className="h-3 w-3" />
                  </button>
                </Badge>
              ) : null;
            })()}

            {/* Search input with suggestions dropdown */}
            <div className="relative">
              <div className="relative flex items-center">
                <Search className="absolute left-2.5 h-3.5 w-3.5 text-muted-foreground pointer-events-none" />
                <Input
                  ref={filterInputRef}
                  value={filterSearch}
                  onChange={(e) => setFilterSearch(e.target.value)}
                  onFocus={() => setFilterDropdownOpen(true)}
                  onBlur={(e) => {
                    if (!filterContainerRef.current?.contains(e.relatedTarget as Node)) {
                      setFilterDropdownOpen(false);
                      setFilterSearch('');
                    }
                  }}
                  onKeyDown={(e) => {
                    if (e.key === 'Escape') {
                      setFilterDropdownOpen(false);
                      setFilterSearch('');
                      filterInputRef.current?.blur();
                    }
                  }}
                  placeholder="Add filter..."
                  className="pl-8 h-7 w-44 text-sm"
                />
              </div>

              {/* Suggestions dropdown */}
              {filterDropdownOpen && (
                <div className="absolute top-full left-0 mt-1 w-64 bg-popover border rounded-md shadow-md z-50 py-1 text-popover-foreground">
                  {filteredMediaTypes.length > 0 && (
                    <>
                      <p className="px-2 py-1 text-xs font-semibold text-muted-foreground uppercase tracking-wide">Media Type</p>
                      {filteredMediaTypes.map(opt => (
                        <button
                          key={opt.type}
                          className={cn(
                            'w-full flex items-center gap-2 px-3 py-1.5 text-sm hover:bg-accent hover:text-accent-foreground',
                            selectedMediaType === opt.type && 'bg-accent text-accent-foreground'
                          )}
                          onMouseDown={(e) => e.preventDefault()}
                          onClick={() => {
                            setSelectedMediaType(selectedMediaType === opt.type ? undefined : opt.type);
                            setFilterSearch('');
                            setFilterDropdownOpen(false);
                            filterInputRef.current?.blur();
                          }}
                        >
                          {getMediaTypeIcon(opt.type)}
                          <span>{opt.label}</span>
                          {selectedMediaType === opt.type && <Check className="h-3 w-3 ml-auto" />}
                        </button>
                      ))}
                    </>
                  )}

                  {filteredTags.length > 0 && (
                    <>
                      <p className="px-2 py-1 text-xs font-semibold text-muted-foreground uppercase tracking-wide">Tags</p>
                      {filteredTags.map(tag => (
                        <button
                          key={tag.id}
                          className={cn(
                            'w-full flex items-center gap-2 px-3 py-1.5 text-sm hover:bg-accent hover:text-accent-foreground',
                            selectedTagId === tag.id && 'bg-accent text-accent-foreground'
                          )}
                          onMouseDown={(e) => e.preventDefault()}
                          onClick={() => {
                            setSelectedTagId(selectedTagId === tag.id ? undefined : tag.id);
                            setFilterSearch('');
                            setFilterDropdownOpen(false);
                            filterInputRef.current?.blur();
                          }}
                        >
                          <TagIcon className="h-3 w-3 shrink-0" />
                          <span className="truncate">{tag.name}</span>
                          <span className="ml-auto text-xs text-muted-foreground shrink-0">{tag.itemCount}</span>
                          {selectedTagId === tag.id && <Check className="h-3 w-3 shrink-0" />}
                        </button>
                      ))}
                    </>
                  )}

                  {filteredPeople.length > 0 && (
                    <>
                      <p className="px-2 py-1 text-xs font-semibold text-muted-foreground uppercase tracking-wide">People</p>
                      {filteredPeople.map(person => (
                        <button
                          key={person.id}
                          className={cn(
                            'w-full flex items-center gap-2 px-3 py-1.5 text-sm hover:bg-accent hover:text-accent-foreground',
                            selectedPersonId === person.id && 'bg-accent text-accent-foreground'
                          )}
                          onMouseDown={(e) => e.preventDefault()}
                          onClick={() => {
                            setSelectedPersonId(selectedPersonId === person.id ? undefined : person.id);
                            setFilterSearch('');
                            setFilterDropdownOpen(false);
                            filterInputRef.current?.blur();
                          }}
                        >
                          <UserIcon className="h-3 w-3 shrink-0" />
                          <span className="truncate">{person.name}</span>
                          <span className="ml-auto text-xs text-muted-foreground shrink-0">{person.itemCount}</span>
                          {selectedPersonId === person.id && <Check className="h-3 w-3 shrink-0" />}
                        </button>
                      ))}
                    </>
                  )}

                  {filteredMediaTypes.length === 0 && filteredTags.length === 0 && filteredPeople.length === 0 && (
                    <p className="px-3 py-2 text-sm text-muted-foreground">No filters found</p>
                  )}

                  {filterSearch.trim() && tagsData && !tagsData.tags.some(t => t.name.toLowerCase() === filterSearch.trim().toLowerCase()) && (
                    <>
                      <div className="border-t my-1" />
                      <button
                        className="w-full flex items-center gap-2 px-3 py-1.5 text-sm hover:bg-accent hover:text-accent-foreground"
                        onMouseDown={(e) => e.preventDefault()}
                        onClick={() => {
                          setNewTagName(filterSearch.trim());
                          setIsAddingTag(true);
                          setFilterSearch('');
                          setFilterDropdownOpen(false);
                          filterInputRef.current?.blur();
                        }}
                      >
                        <Plus className="h-3 w-3" />
                        <span>Create tag &quot;{filterSearch.trim()}&quot;</span>
                      </button>
                    </>
                  )}
                </div>
              )}
            </div>
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
                {hasActiveFilter ? 'No items found with the selected filters' : 'No items found'}
              </p>
              {hasActiveFilter && (
                <Button
                  onClick={() => {
                    setSelectedMediaType(undefined);
                    setSelectedTagId(undefined);
                    setSelectedPersonId(undefined);
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
