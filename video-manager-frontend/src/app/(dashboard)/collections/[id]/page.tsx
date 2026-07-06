'use client';

import { useParams, useRouter } from 'next/navigation';
import { useState, useEffect, useMemo, useRef } from 'react';
import {
  useCollection,
  useAddItemToCollection,
  useRemoveItemFromCollection,
  useUpdateCollectionItemOrder,
  useUpdateCollectionItemNote,
  useExportCollectionToShotcut,
} from '@/lib/hooks/useApi';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Textarea } from '@/components/ui/textarea';
import { ArrowLeft, Download, GripVertical, Trash2, RotateCcw, FolderPlus, MessageSquare, Image as ImageIcon } from 'lucide-react';
import { useToast } from '@/hooks/use-toast';
import Link from 'next/link';
import { useCollectionModeStore } from '@/lib/store/collectionModeStore';
import { AuthenticatedImage } from '@/components/ui/authenticated-image';
import { cn } from '@/lib/utils';

function CollectionItemCard({
  item,
  index,
  collectionId,
  isDraggingOver,
  onRemove,
  onDragStart,
  onDragEnd,
  onDragOver,
  onDragLeave,
  onDrop,
}: {
  item: { id: string; providerName: string; providerItemId: string; order: number; note?: string | null };
  index: number;
  collectionId: string;
  isDraggingOver: boolean;
  onRemove: (id: string) => void;
  onDragStart: (e: React.DragEvent, index: number) => void;
  onDragEnd: () => void;
  onDragOver: (e: React.DragEvent) => void;
  onDragLeave: () => void;
  onDrop: (e: React.DragEvent, index: number) => void;
}) {
  const [noteOpen, setNoteOpen] = useState(false);
  const [noteValue, setNoteValue] = useState(item.note ?? '');
  const [trackedNote, setTrackedNote] = useState(item.note ?? '');
  const noteMutation = useUpdateCollectionItemNote();
  const textareaRef = useRef<HTMLTextAreaElement>(null);

  // Adjusting state during render (React's documented alternative to an effect
  // for syncing state to a changing value): https://react.dev/reference/react/useState#storing-information-from-previous-renders
  if ((item.note ?? '') !== trackedNote) {
    setTrackedNote(item.note ?? '');
    setNoteValue(item.note ?? '');
  }

  useEffect(() => {
    if (noteOpen) textareaRef.current?.focus();
  }, [noteOpen]);

  const saveNote = () => {
    const trimmed = noteValue.trim();
    const current = item.note ?? '';
    if (trimmed === current) return;
    noteMutation.mutate({ collectionId, itemId: item.id, note: trimmed || null });
  };

  const thumbnailUrl = `/api/providers/${item.providerName}/items/${item.providerItemId}/thumbnail`;
  const hasNote = !!(item.note?.trim());

  return (
    <Card
      draggable
      onDragStart={(e) => onDragStart(e, index)}
      onDragEnd={onDragEnd}
      onDragOver={onDragOver}
      onDragLeave={onDragLeave}
      onDrop={(e) => onDrop(e, index)}
      className={cn(
        'overflow-hidden cursor-move transition-all p-0 gap-0',
        isDraggingOver ? 'ring-2 ring-primary scale-[1.02]' : 'hover:shadow-md',
      )}
    >
      {/* Thumbnail */}
      <div className="aspect-video bg-muted">
        <AuthenticatedImage
          src={thumbnailUrl}
          alt={`Collection item ${index + 1}`}
          className="w-full h-full object-cover"
          fallback={
            <div className="w-full h-full flex items-center justify-center text-muted-foreground">
              <ImageIcon className="h-8 w-8" />
            </div>
          }
        />
      </div>

      {/* Controls */}
      <div className="flex items-center gap-1 px-2 py-1.5 border-t">
        <GripVertical className="h-4 w-4 text-muted-foreground shrink-0" />
        <div className="flex-1" />
        <Button
          variant="ghost"
          size="sm"
          className="h-7 w-7 p-0"
          onClick={() => setNoteOpen(v => !v)}
          title={hasNote ? 'Edit note' : 'Add note'}
        >
          <MessageSquare className={cn('h-3.5 w-3.5', hasNote ? 'text-primary fill-primary/20' : 'text-muted-foreground')} />
        </Button>
        <Button
          variant="ghost"
          size="sm"
          className="h-7 w-7 p-0"
          onClick={() => onRemove(item.id)}
        >
          <Trash2 className="h-3.5 w-3.5 text-destructive" />
        </Button>
      </div>

      {/* Note — inline, compact */}
      {noteOpen && (
        <div className="px-2 pb-2">
          <Textarea
            ref={textareaRef}
            value={noteValue}
            onChange={e => setNoteValue(e.target.value)}
            onBlur={saveNote}
            placeholder="Add a note…"
            rows={2}
            className="text-xs resize-none min-h-0"
          />
        </div>
      )}

      {!noteOpen && hasNote && (
        <p
          className="px-2 pb-2 text-xs text-muted-foreground italic cursor-pointer line-clamp-2"
          onClick={() => setNoteOpen(true)}
          title="Click to edit"
        >
          {item.note}
        </p>
      )}
    </Card>
  );
}

function RemovedCollectionItemCard({
  item,
  onRestore,
}: {
  item: { id: string; providerName: string; providerItemId: string; removedAt?: string | null };
  onRestore: (providerName: string, providerItemId: string) => void;
}) {
  const thumbnailUrl = `/api/providers/${item.providerName}/items/${item.providerItemId}/thumbnail`;

  return (
    <Card className="overflow-hidden p-0 gap-0 opacity-75 hover:opacity-100 transition-opacity">
      <div className="aspect-video bg-muted">
        <AuthenticatedImage
          src={thumbnailUrl}
          alt="Removed collection item"
          className="w-full h-full object-cover grayscale"
          fallback={
            <div className="w-full h-full flex items-center justify-center text-muted-foreground">
              <ImageIcon className="h-8 w-8" />
            </div>
          }
        />
      </div>
      <div className="flex items-center gap-1 px-2 py-1.5 border-t">
        <p className="flex-1 text-xs text-muted-foreground truncate">
          {item.removedAt
            ? `Removed ${new Date(item.removedAt).toLocaleDateString('en-GB', { day: '2-digit', month: '2-digit', year: 'numeric' })}`
            : 'Removed'}
        </p>
        <Button
          variant="ghost"
          size="sm"
          className="h-7 w-7 p-0"
          onClick={() => onRestore(item.providerName, item.providerItemId)}
          title="Restore to collection"
        >
          <RotateCcw className="h-3.5 w-3.5 text-primary" />
        </Button>
      </div>
    </Card>
  );
}

export default function CollectionDetailPage() {
  const params = useParams();
  const router = useRouter();
  const collectionId = params.id as string;
  const { toast } = useToast();
  const { enterCollectionMode } = useCollectionModeStore();

  const { data: collection, isLoading, error } = useCollection(collectionId);
  const addMutation = useAddItemToCollection();
  const removeMutation = useRemoveItemFromCollection();
  const reorderMutation = useUpdateCollectionItemOrder();
  const exportMutation = useExportCollectionToShotcut();

  const [view, setView] = useState<'active' | 'removed'>('active');

  // Ref instead of state: avoids stale-closure issues inside handleDrop
  const draggedIndexRef = useRef<number | null>(null);
  const [dragOverIndex, setDragOverIndex] = useState<number | null>(null);

  const activeItems = useMemo(() => collection?.items.filter(i => !i.isRemoved) ?? [], [collection]);
  const removedItems = useMemo(() => collection?.items.filter(i => i.isRemoved) ?? [], [collection]);

  const handleDragStart = (e: React.DragEvent, index: number) => {
    draggedIndexRef.current = index;
    e.dataTransfer.effectAllowed = 'move';
    // Required by Firefox to initiate a drag
    e.dataTransfer.setData('text/plain', String(index));
  };

  const handleDragEnd = () => {
    draggedIndexRef.current = null;
    setDragOverIndex(null);
  };

  const handleDragOver = (e: React.DragEvent, index: number) => {
    e.preventDefault();
    e.dataTransfer.dropEffect = 'move';
    setDragOverIndex(index);
  };

  const handleDragLeave = () => {
    setDragOverIndex(null);
  };

  const handleDrop = async (e: React.DragEvent, dropIndex: number) => {
    e.preventDefault();
    const draggedIndex = draggedIndexRef.current;
    setDragOverIndex(null);
    draggedIndexRef.current = null;

    if (draggedIndex === null || draggedIndex === dropIndex || !collection) return;

    const items = [...activeItems];
    const [draggedItem] = items.splice(draggedIndex, 1);
    items.splice(dropIndex, 0, draggedItem);

    const reorderedItems = items.map((item, idx) => ({ itemId: item.id, newOrder: idx }));

    try {
      await reorderMutation.mutateAsync({ collectionId, items: reorderedItems });
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Failed to reorder items';
      console.error('Reorder failed:', err);
      toast.error(message);
    }
  };

  const handleRemoveItem = async (itemId: string) => {
    try {
      await removeMutation.mutateAsync({ collectionId, itemId });
      toast.success('Item removed from collection');
    } catch (error) {
      toast.error(error instanceof Error ? error.message : 'Failed to remove item');
    }
  };

  const handleRestoreItem = async (providerName: string, providerItemId: string) => {
    try {
      await addMutation.mutateAsync({ collectionId, providerName, providerItemId });
      toast.success('Item restored to collection');
    } catch (error) {
      toast.error(error instanceof Error ? error.message : 'Failed to restore item');
    }
  };

  const handleEnterCollectionMode = () => {
    enterCollectionMode(collectionId);
    router.push('/items');
  };

  const handleExport = async () => {
    try {
      const blob = await exportMutation.mutateAsync(collectionId);
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      const safeCollectionName = collection?.name.replace(/[^a-z0-9]/gi, '_') || 'collection';
      const dateStr = new Date().toISOString().slice(0, 10);
      link.download = `${safeCollectionName}_${dateStr}.zip`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
      toast.success('Collection exported as zip archive with MLT file and assets');
    } catch (error) {
      toast.error(error instanceof Error ? error.message : 'Failed to export collection');
    }
  };

  if (isLoading) {
    return (
      <div className="container mx-auto px-4 py-8 flex items-center justify-center min-h-[400px]">
        <p className="text-muted-foreground">Loading collection...</p>
      </div>
    );
  }

  if (error || !collection) {
    return (
      <div className="container mx-auto px-4 py-8 flex items-center justify-center min-h-[400px]">
        <p className="text-destructive">Collection not found</p>
      </div>
    );
  }

  return (
    <div className="container mx-auto px-4 py-8">
      <div className="mb-6">
        <Link href="/collections">
          <Button variant="ghost" size="sm">
            <ArrowLeft className="mr-2 h-4 w-4" />
            Back to Collections
          </Button>
        </Link>
      </div>

      <div className="flex justify-between items-start mb-8">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">{collection.name}</h1>
          {collection.description && (
            <p className="text-muted-foreground mt-2">{collection.description}</p>
          )}
          <p className="text-sm text-muted-foreground mt-2">
            {activeItems.length} item{activeItems.length !== 1 ? 's' : ''}
          </p>
        </div>

        <div className="flex gap-2">
          <Button variant="outline" onClick={handleEnterCollectionMode}>
            <FolderPlus className="mr-2 h-4 w-4" />
            Collection Mode
          </Button>
          <Button
            onClick={handleExport}
            disabled={activeItems.length === 0 || exportMutation.isPending}
          >
            <Download className="mr-2 h-4 w-4" />
            {exportMutation.isPending ? 'Exporting...' : 'Export to Shotcut'}
          </Button>
        </div>
      </div>

      <div className="flex gap-2 mb-6 border-b">
        <button
          onClick={() => setView('active')}
          className={cn(
            'px-3 py-2 text-sm font-medium border-b-2 -mb-px transition-colors',
            view === 'active'
              ? 'border-primary text-foreground'
              : 'border-transparent text-muted-foreground hover:text-foreground',
          )}
        >
          Items ({activeItems.length})
        </button>
        <button
          onClick={() => setView('removed')}
          className={cn(
            'px-3 py-2 text-sm font-medium border-b-2 -mb-px transition-colors',
            view === 'removed'
              ? 'border-primary text-foreground'
              : 'border-transparent text-muted-foreground hover:text-foreground',
          )}
        >
          Removed ({removedItems.length})
        </button>
      </div>

      {view === 'active' ? (
        activeItems.length === 0 ? (
          <Card>
            <CardContent className="flex flex-col items-center justify-center py-12">
              <p className="text-lg font-medium mb-2">No items in collection</p>
              <p className="text-sm text-muted-foreground mb-4">
                Go to the items page and add videos or images to this collection
              </p>
              <Link href="/items">
                <Button>Browse Items</Button>
              </Link>
            </CardContent>
          </Card>
        ) : (
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 gap-3">
            {activeItems.map((item, index) => (
              <CollectionItemCard
                key={item.id}
                item={item}
                index={index}
                collectionId={collectionId}
                isDraggingOver={dragOverIndex === index}
                onRemove={handleRemoveItem}
                onDragStart={handleDragStart}
                onDragEnd={handleDragEnd}
                onDragOver={(e) => handleDragOver(e, index)}
                onDragLeave={handleDragLeave}
                onDrop={handleDrop}
              />
            ))}
          </div>
        )
      ) : (
        removedItems.length === 0 ? (
          <Card>
            <CardContent className="flex flex-col items-center justify-center py-12">
              <p className="text-lg font-medium mb-2">No removed items</p>
              <p className="text-sm text-muted-foreground">
                Items you remove from this collection will show up here so you can restore them later
              </p>
            </CardContent>
          </Card>
        ) : (
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 gap-3">
            {removedItems.map((item) => (
              <RemovedCollectionItemCard
                key={item.id}
                item={item}
                onRestore={handleRestoreItem}
              />
            ))}
          </div>
        )
      )}
    </div>
  );
}
