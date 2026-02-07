'use client';

import { useParams, useRouter } from 'next/navigation';
import { useState } from 'react';
import { useCollection, useRemoveItemFromCollection, useUpdateCollectionItemOrder, useExportCollectionToShotcut } from '@/lib/hooks/useApi';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { ArrowLeft, Download, GripVertical, Trash2, FolderPlus } from 'lucide-react';
import { useToast } from '@/hooks/use-toast';
import Link from 'next/link';
import { useCollectionModeStore } from '@/lib/store/collectionModeStore';

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

  const [draggedIndex, setDraggedIndex] = useState<number | null>(null);

  const handleRemoveItem = async (itemId: string) => {
    try {
      await removeMutation.mutateAsync({ collectionId, itemId });
      toast.success('Item removed from collection');
    } catch (error) {
      toast.error(error instanceof Error ? error.message : 'Failed to remove item');
    }
  };

  const handleDragStart = (index: number) => {
    setDraggedIndex(index);
  };

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
  };

  const handleDrop = async (dropIndex: number) => {
    if (draggedIndex === null || !collection) return;
    
    const items = [...collection.items];
    const draggedItem = items[draggedIndex];
    
    items.splice(draggedIndex, 1);
    items.splice(dropIndex, 0, draggedItem);
    
    const reorderedItems = items.map((item, idx) => ({
      itemId: item.id,
      newOrder: idx,
    }));
    
    try {
      await reorderMutation.mutateAsync({ collectionId, items: reorderedItems });
      setDraggedIndex(null);
    } catch (error) {
      toast.error('Failed to reorder items');
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
      
      // Create filename from collection name (will be .zip now)
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
      <div className="container mx-auto px-4 py-8">
        <div className="flex items-center justify-center min-h-[400px]">
          <p className="text-muted-foreground">Loading collection...</p>
        </div>
      </div>
    );
  }

  if (error || !collection) {
    return (
      <div className="container mx-auto px-4 py-8">
        <div className="flex items-center justify-center min-h-[400px]">
          <p className="text-destructive">Collection not found</p>
        </div>
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
            {collection.items.length} items in collection
          </p>
        </div>
        
        <div className="flex gap-2">
          <Button
            variant="outline"
            onClick={handleEnterCollectionMode}
          >
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
        <div className="space-y-2">
          {collection.items.map((item, index) => (
            <Card
              key={item.id}
              draggable
              onDragStart={() => handleDragStart(index)}
              onDragOver={handleDragOver}
              onDrop={() => handleDrop(index)}
              className="cursor-move hover:shadow-md transition-shadow"
            >
              <CardContent className="flex items-center justify-between p-4">
                <div className="flex items-center gap-3">
                  <GripVertical className="h-5 w-5 text-muted-foreground" />
                  <div>
                    <p className="font-medium">
                      {item.providerName} - {item.providerItemId}
                    </p>
                    <p className="text-sm text-muted-foreground">
                      Position: {item.order + 1}
                    </p>
                  </div>
                </div>
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => handleRemoveItem(item.id)}
                  disabled={removeMutation.isPending}
                >
                  <Trash2 className="h-4 w-4 text-destructive" />
                </Button>
              </CardContent>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
