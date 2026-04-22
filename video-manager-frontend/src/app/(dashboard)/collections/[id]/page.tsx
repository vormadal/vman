'use client';

import { useParams, useRouter } from 'next/navigation';
import { useState, useEffect, useRef } from 'react';
import {
  useCollection,
  useRemoveItemFromCollection,
  useUpdateCollectionItemOrder,
  useUpdateCollectionItemNote,
  useExportCollectionToShotcut,
} from '@/lib/hooks/useApi';
import { Card, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Textarea } from '@/components/ui/textarea';
import { ArrowLeft, Download, GripVertical, Trash2, FolderPlus, MessageSquare, Image as ImageIcon } from 'lucide-react';
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
  const noteMutation = useUpdateCollectionItemNote();
  const textareaRef = useRef<HTMLTextAreaElement>(null);

  useEffect(() => {
    setNoteValue(item.note ?? '');
  }, [item.note]);

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

export default function CollectionDetailPage() {
  const params = useParams();
  const router = useRouter();
  const collectionId = params.id as string;
  const { toast } = useToast();
  const { enterCollectionMode } = useCollectionModeStore();

  const { data: collection, isLoading, error } = useCollection(collectionId);
  const removeMutation = useRemoveItemFromCollection();
  const reorderMutation = useUpdateCollectionItemOrder();
  const exportMutation = useExportCollectionToShotcut();

  // Ref instead of state: avoids stale-closure issues inside handleDrop
  const draggedIndexRef = useRef<number | null>(null);
  const [dragOverIndex, setDragOverIndex] = useState<number | null>(null);

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

    const items = [...collection.items];
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
            {collection.items.length} item{collection.items.length !== 1 ? 's' : ''}
          </p>
        </div>

        <div className="flex gap-2">
          <Button variant="outline" onClick={handleEnterCollectionMode}>
            <FolderPlus className="mr-2 h-4 w-4" />
            Collection Mode
          </Button>
          <Button
            onClick={handleExport}
            disabled={collection.items.length === 0 || exportMutation.isPending}
          >
            <Download className="mr-2 h-4 w-4" />
            {exportMutation.isPending ? 'Exporting...' : 'Export to Shotcut'}
          </Button>
        </div>
      </div>

      {collection.items.length === 0 ? (
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
          {collection.items.map((item, index) => (
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
      )}
    </div>
  );
}
