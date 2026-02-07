import { create } from 'zustand';
import { persist } from 'zustand/middleware';

interface CollectionModeState {
  isActive: boolean;
  activeCollectionId: string | null;
  enterCollectionMode: (collectionId: string) => void;
  exitCollectionMode: () => void;
}

export const useCollectionModeStore = create<CollectionModeState>()(
  persist(
    (set) => ({
      isActive: false,
      activeCollectionId: null,
      enterCollectionMode: (collectionId: string) =>
        set({ isActive: true, activeCollectionId: collectionId }),
      exitCollectionMode: () =>
        set({ isActive: false, activeCollectionId: null }),
    }),
    {
      name: 'collection-mode-storage',
    }
  )
);
