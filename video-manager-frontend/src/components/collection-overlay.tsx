'use client';

import { useState, useEffect } from 'react';
import { useCollections, useCollection, useAddItemToCollection, useRemoveItemFromCollection } from '@/lib/hooks/useApi';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { X, ChevronDown, ChevronUp, FolderOpen, Download, Trash2 } from 'lucide-react';
import Link from 'next/link';
import { cn } from '@/lib/utils';

interface CollectionOverlayProps {
  activeCollectionId: string | null;
  onClose: () => void;
  onSelectCollection: (id: string | null) => void;
}

export function CollectionOverlay({ activeCollectionId, onClose, onSelectCollection }: CollectionOverlayProps) {
  const [isMinimized, setIsMinimized] = useState(false);
  const [position, setPosition] = useState({ x: 20, y: window.innerHeight - 400 });
  const [isDragging, setIsDragging] = useState(false);
  const [dragOffset, setDragOffset] = useState({ x: 0, y: 0 });
  
  const { data: collectionsData } = useCollections();
  const { data: activeCollection } = useCollection(activeCollectionId || '');

  const isMobile = typeof window !== 'undefined' && window.innerWidth < 768;

  useEffect(() => {
    if (isMobile) {
      setPosition({ x: 0, y: window.innerHeight - 200 });
    }
  }, [isMobile]);

  const handleMouseDown = (e: React.MouseEvent) => {
    if (isMobile) return;
    setIsDragging(true);
    setDragOffset({
      x: e.clientX - position.x,
      y: e.clientY - position.y,
    });
  };

  useEffect(() => {
    const handleMouseMove = (e: MouseEvent) => {
      if (!isDragging || isMobile) return;
      
      const newX = Math.max(0, Math.min(window.innerWidth - 350, e.clientX - dragOffset.x));
      const newY = Math.max(0, Math.min(window.innerHeight - 100, e.clientY - dragOffset.y));
      
      setPosition({ x: newX, y: newY });
    };

    const handleMouseUp = () => {
      setIsDragging(false);
    };

    if (isDragging) {
      document.addEventListener('mousemove', handleMouseMove);
      document.addEventListener('mouseup', handleMouseUp);
    }

    return () => {
      document.removeEventListener('mousemove', handleMouseMove);
      document.removeEventListener('mouseup', handleMouseUp);
    };
  }, [isDragging, dragOffset, isMobile]);

  if (!activeCollectionId && !collectionsData?.collections.length) {
    return null;
  }

  return (
    <div
      className={cn(
        "fixed z-50 bg-background border rounded-lg shadow-2xl",
        isMobile 
          ? "left-0 right-0 bottom-0" 
          : "w-[350px]"
      )}
      style={
        isMobile
          ? undefined
          : {
              left: `${position.x}px`,
              top: `${position.y}px`,
            }
      }
    >
      <CardHeader
        className={cn(
          "flex flex-row items-center justify-between space-y-0 pb-2 cursor-move select-none",
          isMobile && "cursor-default"
        )}
        onMouseDown={handleMouseDown}
      >
        <CardTitle className="text-sm font-medium">
          Collection Mode
        </CardTitle>
        <div className="flex items-center gap-2">
          <Button
            variant="ghost"
            size="sm"
            onClick={() => setIsMinimized(!isMinimized)}
          >
            {isMinimized ? <ChevronUp className="h-4 w-4" /> : <ChevronDown className="h-4 w-4" />}
          </Button>
          <Button
            variant="ghost"
            size="sm"
            onClick={onClose}
          >
            <X className="h-4 w-4" />
          </Button>
        </div>
      </CardHeader>

      {!isMinimized && (
        <CardContent className="space-y-3">
          {/* Collection Selector */}
          <div className="space-y-2">
            <label className="text-xs font-medium text-muted-foreground">
              Active Collection
            </label>
            {collectionsData?.collections && collectionsData.collections.length > 0 ? (
              <select
                className="w-full px-3 py-2 text-sm border rounded-md bg-background"
                value={activeCollectionId || ''}
                onChange={(e) => onSelectCollection(e.target.value || null)}
              >
                <option value="">Select a collection...</option>
                {collectionsData.collections.map((collection) => (
                  <option key={collection.id} value={collection.id}>
                    {collection.name} ({collection.itemCount} items)
                  </option>
                ))}
              </select>
            ) : (
              <div className="text-center py-4">
                <p className="text-xs text-muted-foreground mb-2">
                  No collections yet
                </p>
                <Link href="/collections">
                  <Button variant="outline" size="sm">
                    Create Collection
                  </Button>
                </Link>
              </div>
            )}
          </div>

          {/* Active Collection Details */}
          {activeCollection && (
            <div className="space-y-2">
              <div className="flex items-center justify-between">
                <span className="text-xs font-medium text-muted-foreground">
                  {activeCollection.name}
                </span>
                <Badge variant="secondary" className="text-xs">
                  {activeCollection.items.length} items
                </Badge>
              </div>

              {/* Items List */}
              <div className="max-h-48 overflow-y-auto space-y-1">
                {activeCollection.items.length === 0 ? (
                  <p className="text-xs text-muted-foreground text-center py-4">
                    No items yet. Browse items and add them to this collection.
                  </p>
                ) : (
                  activeCollection.items.map((item, index) => (
                    <div
                      key={item.id}
                      className="flex items-center justify-between text-xs p-2 bg-muted/50 rounded"
                    >
                      <span className="truncate flex-1">
                        {index + 1}. {item.providerItemId.substring(0, 20)}...
                      </span>
                    </div>
                  ))
                )}
              </div>

              {/* Actions */}
              <div className="flex gap-2 pt-2 border-t">
                <Link href={`/collections/${activeCollectionId}`} className="flex-1">
                  <Button variant="outline" size="sm" className="w-full">
                    <FolderOpen className="h-4 w-4 mr-2" />
                    Manage
                  </Button>
                </Link>
              </div>
            </div>
          )}
        </CardContent>
      )}
    </div>
  );
}
