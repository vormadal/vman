# Collection Removed-Items Tracking

## Overview
Currently, removing an item from a collection (`RemoveItemFromCollection`) hard-deletes the
`CollectionItem` join row. This means there's no memory that the item was ever removed, so a
later bulk-add-by-filter (or manual re-add) can silently pull it right back in. We want to
soft-delete instead, expose a "Removed" view on the collection detail page, and let the user
deliberately re-add an item from that view.

## Requirements
1. Removing an item from a collection keeps a record of the removal (soft delete), not a hard delete.
2. Bulk-add-by-filter must **not** resurrect previously-removed items automatically — it should skip them.
3. The collection detail page gets a way to view removed items (separate view/toggle), each with a way to explicitly re-add it.
4. Explicitly re-adding a single item (from the removed view, or by picking it again in the Items page) reactivates the same row instead of erroring on the unique constraint.

## Backend Changes

### 1. `CollectionItem` model (`Common/Models/CollectionItem.cs`)
Add:
```csharp
public bool IsRemoved { get; set; }
public DateTime? RemovedAt { get; set; }
```

### 2. `ApplicationDbContext` fluent config
In the `CollectionItem` entity block, add:
```csharp
entity.Property(e => e.IsRemoved).IsRequired().HasDefaultValue(false);
```
(mirrors the `Person.IsHidden` precedent). The existing unique index on
`(CollectionId, ProviderName, ProviderItemId)` stays as-is and un-partial — since we never
hard-insert a duplicate row for the same item again, only flip `IsRemoved`, one row per
collection+item is still correct.

### 3. Migration
`dotnet ef migrations add AddIsRemovedToCollectionItem` (from `VManBackend/`). Do **not** run
`database update` — Aspire applies it on restart.

### 4. `RemoveItemFromCollection.cs`
- Replace `db.CollectionItems.Remove(collectionItem)` with:
  ```csharp
  collectionItem.IsRemoved = true;
  collectionItem.RemovedAt = DateTime.UtcNow;
  ```
- The order-compaction query must exclude already-removed items:
  `ci.CollectionId == request.CollectionId && ci.Order > removedOrder && !ci.IsRemoved`.

### 5. `AddItemToCollection.cs`
- The existing lookup (`FirstOrDefaultAsync` by CollectionId+ProviderName+ProviderItemId) already
  finds soft-deleted rows too since it has no `IsRemoved` filter.
- If found and **not** removed → throw "Item already exists in collection" (unchanged).
- If found and **removed** → reactivate in place: `IsRemoved = false`, `RemovedAt = null`,
  `Order = maxOrder + 1` (append to end, its old slot is gone), bump `collection.UpdatedAt`,
  save, return the same response shape (treat as success either way).
- If not found → insert new row (unchanged).
- `maxOrder` computation should filter `!ci.IsRemoved` so stale removed rows don't skew ordering.

### 6. `BulkAddFilteredItemsToCollection.cs`
- Fetch **all** existing rows (active + removed) for the collection/provider, not just active,
  so we never attempt a duplicate insert (would violate the unique index).
- Split into `existingIds` (all) and `removedIds` (subset with `IsRemoved`).
- `newItems` = filtered items not in `existingIds` (i.e. genuinely new — removed items are
  excluded here, matching requirement #2).
- `maxOrder` computation filters `!ci.IsRemoved`.
- Extend `Response` with a count so the UI can tell the user some matches were skipped because
  they'd been removed before:
  ```csharp
  public record Response(int AddedCount, int SkippedCount, int SkippedRemovedCount);
  ```

### 7. `GetCollectionById.cs`
- Return **all** items (active + removed) in one response; add `IsRemoved`/`RemovedAt` to the DTO:
  ```csharp
  public record CollectionItemDto(Guid Id, string ProviderName, string ProviderItemId, int Order, string? Note, bool IsRemoved, DateTime? RemovedAt, DateTime CreatedAt);
  ```
- Ordering: active items by `Order` asc, followed by removed items by `RemovedAt` desc.
- No new endpoint/query param needed — the frontend splits the single list client-side. Keeps
  this in line with the existing unpaginated, single-fetch pattern for collections.

## Frontend Changes

### 1. `src/lib/api/types.ts`
- `CollectionItemDto`: add `isRemoved: boolean; removedAt?: string | null;`
- `BulkAddFilteredItemsResponse` (wherever defined near `BulkAddFilteredItemsParams`): add `skippedRemovedCount: number`.

### 2. `src/lib/api/client.ts`
No new endpoints — existing `addItemToCollection`/`removeItemFromCollection` are reused as-is;
just response typing changes flow through automatically.

### 3. Collection detail page (`src/app/(dashboard)/collections/[id]/page.tsx`)
- Split `collection.items` into `activeItems` / `removedItems` via `useMemo`.
- Add a small local toggle (two buttons, matching the existing lightweight filter-UI style used
  on the Items page — no new shadcn primitive needed since Tabs isn't used anywhere else in the
  app): "Items (N)" / "Removed (M)".
- Header item count reflects `activeItems.length` (unchanged behavior for the main count).
- Active view: unchanged grid, drag-reorder, notes, remove button.
- Removed view: same thumbnail grid, but each card shows "Removed on {date}" instead of the note
  control, and a "Restore" button (`RotateCcw` icon) instead of the trash icon, wired to
  `useAddItemToCollection` with the item's `providerName`/`providerItemId` (backend reactivates
  it). No drag-and-drop in this view.
- Empty states differ per view ("No items in collection" vs "No removed items").

### 4. `src/lib/hooks/useApi.ts`
No new hooks required — `useAddItemToCollection` already handles reactivation transparently since
the backend treats it as an upsert now.

## Order of Implementation
1. Backend model + migration + `ApplicationDbContext` config.
2. `RemoveItemFromCollection` soft-delete + reorder fix.
3. `AddItemToCollection` reactivation logic.
4. `BulkAddFilteredItemsToCollection` exclude-removed logic + response field.
5. `GetCollectionById` DTO + all-items response.
6. Frontend types.
7. Collection detail page UI (toggle + removed view + restore action).
8. Manual verification via Aspire (restart to pick up migration), exercise: remove → verify item
   disappears from active grid and appears in removed view → bulk-add-by-filter matching that
   item confirms it's skipped → restore from removed view → item reappears in active grid.
