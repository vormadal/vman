'use client';

import { useState } from 'react';
import { useCollections, useCreateCollection, useDeleteCollection } from '@/lib/hooks/useApi';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';
import { Plus, Trash2, Film, Calendar } from 'lucide-react';
import { useToast } from '@/hooks/use-toast';
import Link from 'next/link';

export default function CollectionsPage() {
  const [page, setPage] = useState(1);
  const [newCollectionName, setNewCollectionName] = useState('');
  const [newCollectionDescription, setNewCollectionDescription] = useState('');
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false);
  const [collectionToDelete, setCollectionToDelete] = useState<string | null>(null);

  const { data: collectionsData, isLoading, error } = useCollections(page);
  const createMutation = useCreateCollection();
  const deleteMutation = useDeleteCollection();
  const { toast } = useToast();

  const handleCreateCollection = async () => {
    if (!newCollectionName.trim()) {
      toast.error('Collection name is required');
      return;
    }

    try {
      await createMutation.mutateAsync({
        name: newCollectionName.trim(),
        description: newCollectionDescription.trim() || undefined,
      });
      
      toast.success('Collection created successfully');
      
      setNewCollectionName('');
      setNewCollectionDescription('');
      setIsCreateDialogOpen(false);
    } catch (error) {
      toast.error(error instanceof Error ? error.message : 'Failed to create collection');
    }
  };

  const handleDeleteCollection = async (id: string) => {
    try {
      await deleteMutation.mutateAsync(id);
      toast.success('Collection deleted successfully');
      setCollectionToDelete(null);
    } catch (error) {
      toast.error(error instanceof Error ? error.message : 'Failed to delete collection');
    }
  };

  if (isLoading) {
    return (
      <div className="container mx-auto py-8">
        <div className="flex items-center justify-center min-h-[400px]">
          <p className="text-muted-foreground">Loading collections...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="container mx-auto py-8">
        <div className="flex items-center justify-center min-h-[400px]">
          <p className="text-destructive">Error loading collections</p>
        </div>
      </div>
    );
  }

  const collections = collectionsData?.collections || [];

  return (
    <div className="container mx-auto py-8">
      <div className="flex justify-between items-center mb-8">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Collections</h1>
          <p className="text-muted-foreground mt-2">
            Create and manage video collections for Shotcut export
          </p>
        </div>
        
        <Dialog open={isCreateDialogOpen} onOpenChange={setIsCreateDialogOpen}>
          <DialogTrigger asChild>
            <Button>
              <Plus className="mr-2 h-4 w-4" />
              New Collection
            </Button>
          </DialogTrigger>
          <DialogContent>
            <DialogHeader>
              <DialogTitle>Create New Collection</DialogTitle>
              <DialogDescription>
                Create a collection to organize videos and images for export
              </DialogDescription>
            </DialogHeader>
            <div className="space-y-4 py-4">
              <div className="space-y-2">
                <Label htmlFor="name">Name</Label>
                <Input
                  id="name"
                  placeholder="My Video Collection"
                  value={newCollectionName}
                  onChange={(e) => setNewCollectionName(e.target.value)}
                />
              </div>
              <div className="space-y-2">
                <Label htmlFor="description">Description (optional)</Label>
                <Textarea
                  id="description"
                  placeholder="Description of this collection..."
                  value={newCollectionDescription}
                  onChange={(e) => setNewCollectionDescription(e.target.value)}
                  rows={3}
                />
              </div>
            </div>
            <div className="flex justify-end gap-2">
              <Button
                variant="outline"
                onClick={() => setIsCreateDialogOpen(false)}
              >
                Cancel
              </Button>
              <Button
                onClick={handleCreateCollection}
                disabled={createMutation.isPending}
              >
                {createMutation.isPending ? 'Creating...' : 'Create'}
              </Button>
            </div>
          </DialogContent>
        </Dialog>
      </div>

      {collections.length === 0 ? (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-12">
            <Film className="h-12 w-12 text-muted-foreground mb-4" />
            <p className="text-lg font-medium mb-2">No collections yet</p>
            <p className="text-sm text-muted-foreground mb-4">
              Create your first collection to start organizing videos
            </p>
            <Button onClick={() => setIsCreateDialogOpen(true)}>
              <Plus className="mr-2 h-4 w-4" />
              Create Collection
            </Button>
          </CardContent>
        </Card>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {collections.map((collection) => (
            <Card key={collection.id} className="hover:shadow-lg transition-shadow">
              <CardHeader>
                <CardTitle className="flex items-center justify-between">
                  <Link
                    href={`/collections/${collection.id}`}
                    className="hover:underline flex-1"
                  >
                    {collection.name}
                  </Link>
                  <Button
                    variant="ghost"
                    size="sm"
                    onClick={() => setCollectionToDelete(collection.id)}
                    disabled={deleteMutation.isPending}
                  >
                    <Trash2 className="h-4 w-4 text-destructive" />
                  </Button>
                </CardTitle>
              </CardHeader>
              <CardContent>
                {collection.description && (
                  <p className="text-sm text-muted-foreground mb-3 line-clamp-2">
                    {collection.description}
                  </p>
                )}
                <div className="flex items-center gap-4 text-sm text-muted-foreground">
                  <div className="flex items-center gap-1">
                    <Film className="h-4 w-4" />
                    <span>{collection.itemCount} items</span>
                  </div>
                  <div className="flex items-center gap-1">
                    <Calendar className="h-4 w-4" />
                    <span>{new Date(collection.updatedAt).toLocaleDateString()}</span>
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      {collectionsData && collectionsData.totalCount > collectionsData.pageSize && (
        <div className="flex justify-center gap-2 mt-8">
          <Button
            variant="outline"
            onClick={() => setPage((p) => Math.max(1, p - 1))}
            disabled={page === 1}
          >
            Previous
          </Button>
          <Button
            variant="outline"
            onClick={() => setPage((p) => p + 1)}
            disabled={page >= Math.ceil(collectionsData.totalCount / collectionsData.pageSize)}
          >
            Next
          </Button>
        </div>
      )}

      {/* Delete Confirmation Dialog */}
      <AlertDialog open={collectionToDelete !== null} onOpenChange={(open: boolean) => !open && setCollectionToDelete(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete Collection?</AlertDialogTitle>
            <AlertDialogDescription>
              This action cannot be undone. This will permanently delete the collection and all its items.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction
              onClick={() => collectionToDelete && handleDeleteCollection(collectionToDelete)}
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              Delete
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}
