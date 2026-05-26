'use client';

import { useState } from 'react';
import { useCollection } from '@/lib/hooks/useApi';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { X, ChevronDown, ChevronUp, ExternalLink, Link as LinkIcon, Check } from 'lucide-react';
import Link from 'next/link';
import { cn } from '@/lib/utils';

interface CollectionOverlayProps {
  activeCollectionId: string | null;
  onClose: () => void;
}

export function CollectionOverlay({ activeCollectionId, onClose }: CollectionOverlayProps) {
  const [isExpanded, setIsExpanded] = useState(false);
  const [copied, setCopied] = useState(false);
  const { data: activeCollection } = useCollection(activeCollectionId || '');

  const copyLink = () => {
    const url = `${window.location.origin}/items?collection=${activeCollectionId}`;
    navigator.clipboard.writeText(url).then(() => {
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    });
  };

  if (!activeCollectionId) return null;

  return (
    <div className="fixed bottom-0 left-0 right-0 z-50 border-t bg-background shadow-[0_-4px_24px_rgba(0,0,0,0.12)]">
      {/* Expanded item list */}
      {isExpanded && (
        <div className="max-h-48 overflow-y-auto border-b">
          {activeCollection ? (
            activeCollection.items.length === 0 ? (
              <p className="px-4 py-3 text-xs text-muted-foreground">
                No items yet — browse and add items to this collection.
              </p>
            ) : (
              <ul className="divide-y">
                {activeCollection.items.map((item, index) => (
                  <li key={item.id} className="px-4 py-2 text-xs text-muted-foreground">
                    {index + 1}. {item.providerItemId.substring(0, 40)}
                  </li>
                ))}
              </ul>
            )
          ) : (
            <p className="px-4 py-3 text-xs text-muted-foreground">Loading…</p>
          )}
        </div>
      )}

      {/* Dock bar */}
      <div className="flex items-center gap-3 px-4 py-3">
        <span className="font-medium text-sm truncate flex-1">
          {activeCollection?.name ?? '…'}
        </span>

        <Badge variant="secondary" className="shrink-0 text-xs">
          {activeCollection?.items.length ?? 0} items
        </Badge>

        <Button
          variant="ghost"
          size="sm"
          className="gap-1.5 shrink-0"
          onClick={copyLink}
          title="Copy sharable link"
        >
          {copied ? <Check className="h-4 w-4 text-green-500" /> : <LinkIcon className="h-4 w-4" />}
          <span className="hidden sm:inline text-xs">{copied ? 'Copied' : 'Copy link'}</span>
        </Button>

        <Link href={`/collections/${activeCollectionId}`}>
          <Button variant="ghost" size="sm" className="gap-1.5 shrink-0">
            <ExternalLink className="h-4 w-4" />
            <span className="hidden sm:inline text-xs">Manage</span>
          </Button>
        </Link>

        <Button
          variant="ghost"
          size="sm"
          onClick={() => setIsExpanded(v => !v)}
          className="shrink-0"
          aria-label={isExpanded ? 'Collapse' : 'Expand'}
        >
          {isExpanded ? <ChevronDown className="h-4 w-4" /> : <ChevronUp className="h-4 w-4" />}
        </Button>

        <Button
          variant="ghost"
          size="sm"
          onClick={onClose}
          className="shrink-0"
          aria-label="Exit collection mode"
        >
          <X className="h-4 w-4" />
        </Button>
      </div>
    </div>
  );
}
