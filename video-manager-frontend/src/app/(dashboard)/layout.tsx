'use client';

import { NavigationDrawer } from '@/components/navigation-drawer';
import { CollectionOverlay } from '@/components/collection-overlay';
import { useCollectionModeStore } from '@/lib/store/collectionModeStore';
import { cn } from '@/lib/utils';

export default function DashboardLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const { isActive, activeCollectionId, exitCollectionMode } = useCollectionModeStore();

  return (
    <div className="min-h-screen flex flex-col">
      <header className="sticky top-0 z-40 border-b bg-background">
        <div className="flex h-16 items-center px-4 gap-4">
          <NavigationDrawer />
          <div className="flex-1">
            <h1 className="text-xl font-semibold">Video Manager</h1>
          </div>
        </div>
      </header>
      <main className={cn('flex-1', isActive && 'pb-16')}>
        {children}
      </main>
      {isActive && (
        <CollectionOverlay
          activeCollectionId={activeCollectionId}
          onClose={exitCollectionMode}
        />
      )}
    </div>
  );
}
