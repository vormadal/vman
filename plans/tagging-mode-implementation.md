# Tagging Mode Implementation Plan

## Overview
Implement a dedicated tagging mode for efficient media tagging workflow. This mode should allow users to quickly tag items one at a time with minimal clicks.

## Requirements
1. **Single item display**: Show one image at a time in full focus
2. **Tag sidebar**: Display existing tags with quick add/remove functionality
3. **Recent tags first**: Most recently used tags should appear at the top
4. **Inline tag creation**: Allow creating new tags from a textbox without additional dialogs/clicks
5. **Keyboard navigation**: Support arrow keys (← →) to move between items
6. **Button navigation**: Provide discrete arrow buttons for mouse-based navigation

## Implementation Strategy

### 1. Create New Route
- Create `/app/(dashboard)/items/tagging/page.tsx`
- This will be accessible at `/items/tagging`

### 2. UI Layout
```
┌─────────────────────────────────────────────┐
│ [← Prev]    Tagging Mode    [Next →]        │
├─────────────────────────────┬───────────────┤
│                             │               │
│                             │  Tag Sidebar  │
│      Large Image            │  ┌─────────┐  │
│         Display             │  │ New Tag │  │
│                             │  └─────────┘  │
│                             │               │
│                             │  [Recent]     │
│                             │  □ Tag 1      │
│                             │  □ Tag 2      │
│                             │               │
│                             │  [All Tags]   │
│                             │  □ Tag 3      │
│                             │  □ Tag 4      │
└─────────────────────────────┴───────────────┘
```

### 3. Features
- **Item Navigation**: Fetch items in sequence, allow prev/next navigation
- **Tag Display**: Show all tags with checkboxes/badges
- **Quick Actions**: Toggle tags with single click
- **Create Tag**: Input field that creates tag on Enter key
- **Keyboard Shortcuts**: 
  - ArrowLeft/ArrowRight for navigation
  - Enter in tag input to create
- **State Management**: Use React Query for data, local state for current index

### 4. Technical Details
- Use existing `useInfiniteItems` hook for fetching items
- Use existing `useAddTagToItem`, `useRemoveTagFromItem` hooks
- Use existing `useCreateTag` hook
- Maintain current item index in component state
- Sort tags by recent usage (track in component or use updatedAt)

## Files to Create
1. `/video-manager-frontend/src/app/(dashboard)/items/tagging/page.tsx` - Main tagging mode page

## Files to Modify
None (pure addition)

## Testing
1. Navigate to `/items/tagging`
2. Verify single item display
3. Test tag add/remove from sidebar
4. Test inline tag creation
5. Test keyboard navigation (arrow keys)
6. Test button navigation
7. Verify tag ordering (recent first)
