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
}

export function CollectionOverlay({ activeCollectionId, onClose }: CollectionOverlayProps) {
  const [isMinimized, setIsMinimized] = useState(false);
  const DEFAULT_Y_POSITION = 20;
  const [position, setPosition] = useState({ x: 20, y: DEFAULT_Y_POSITION });
  const [isDragging, setIsDragging] = useState(false);
  const [dragOffset, setDragOffset] = useState({ x: 0, y: 0 });
  
  const { data: activeCollection } = useCollection(activeCollectionId || '');

  const isMobile = typeof window !== 'undefined' && window.innerWidth < 768;

  useEffect(() => {
    if (isMobile) {
      setPosition({ x: 0, y: DEFAULT_Y_POSITION });
    }
  }, [isMobile, DEFAULT_Y_POSITION]);

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

  if (!activeCollectionId) {
    return null;
  }

  return (
    <div
      className={cn(
        "fixed z-50 bg-background border rounded-lg shadow-2xl",
        isMobile 
          ? "left-0 right-0 top-0" 
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
          {/* Active Collection Details */}
          {activeCollection ? (
            <div className="space-y-2">
              <div className="flex items-center justify-between">
                <span className="text-sm font-medium">
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
            </div>
          ) : (
            <div className="text-center py-4">
              <p className="text-sm text-muted-foreground">
                Loading collection...
              </p>
            </div>
          )}
        </CardContent>
      )}
    </div>
  );
}
