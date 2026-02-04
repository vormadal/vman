# Unified Items Page Implementation Plan

## Objective
Create a unified "Items" page that displays all synced assets (images, videos, audio) from Immich with comprehensive filtering and tagging capabilities.

## Current State
- Backend: Fully functional `/api/items` endpoint with filtering, sorting, pagination
- Frontend: Images page has good implementation, Videos page uses mock data
- Hooks: Complete implementation in `src/lib/hooks/useApi.ts`

## Implementation Steps

### 1. Create Items Page Component
**File**: `video-manager-frontend/src/app/(dashboard)/items/page.tsx`

Features:
- Display all media types in a grid layout
- Use `useItems()` hook with filtering parameters
- Responsive grid (similar to images page)
- Loading states and error handling
- Empty state when no items

### 2. Create Filter Controls Component
**File**: `video-manager-frontend/src/components/items/ItemsFilters.tsx`

Filters to include:
- Media Type toggle (All / Images / Videos / Audio)
- Tag filter dropdown (multi-select or single)
- Show untagged items checkbox
- Sort by (Created Date / Filename)
- Sort direction (Ascending / Descending)

### 3. Create Item Card Component
**File**: `video-manager-frontend/src/components/items/ItemCard.tsx`

Features:
- Display thumbnail/preview
- Show filename and media type icon
- Display associated tags as badges
- Quick actions: Add/Remove tags
- Click to view details (future enhancement)
- Different styling for videos vs images vs audio

### 4. Create Tag Management Component
**File**: `video-manager-frontend/src/components/items/ItemTagManager.tsx`

Features:
- Add tag to item (dropdown of existing tags)
- Remove tag from item (click badge to remove)
- Create new tag inline
- Visual feedback for tag operations

### 5. Add Navigation Link
**File**: `video-manager-frontend/src/components/navigation/*` (find and update)

Add "Items" or "Media" link to the dashboard navigation

### 6. Optional: Update Videos/Images Pages
Add notice that redirects to unified Items page, or keep them as filtered views

## Technical Details

### State Management
- Use React Query via `useItems()` hook
- Filter state in component (using React.useState)
- Automatic refetch after tag mutations

### Pagination
- Implement pagination controls (page size: 20-50 items)
- Show total count and current page
- Next/Previous buttons

### UI Components (shadcn/ui)
- Card for item display
- Badge for tags
- Button for actions
- Select/Combobox for filters
- Checkbox for toggles
- Skeleton for loading states

### Data Flow
```
Items Page
  ↓ (filter params)
useItems(params)
  ↓ (API request)
GET /api/items?type=Video&tagId=xxx&page=1
  ↓ (response)
ItemsResponse { items: [], total: 100 }
  ↓ (render)
Grid of ItemCard components
```

## File Structure
```
video-manager-frontend/src/
├── app/(dashboard)/
│   └── items/
│       └── page.tsx                    # Main page
├── components/
│   └── items/
│       ├── ItemsFilters.tsx           # Filter controls
│       ├── ItemCard.tsx               # Individual item card
│       ├── ItemTagManager.tsx         # Tag add/remove
│       └── ItemsGrid.tsx              # Grid layout wrapper
```

## Success Criteria
- [ ] Page displays all synced items from Immich
- [ ] Can filter by media type (All/Images/Videos/Audio)
- [ ] Can filter by tags
- [ ] Can show only untagged items
- [ ] Can sort by date or filename
- [ ] Can add tags to items
- [ ] Can remove tags from items
- [ ] Pagination works correctly
- [ ] Loading and error states handled
- [ ] Responsive design works on mobile/tablet/desktop

## Future Enhancements (Not in this plan)
- Bulk operations (select multiple items)
- Item detail modal/page
- Image/video preview on hover
- Advanced search by filename
- Export selected items to Shotcut
- Provider selector (when multiple providers added)
