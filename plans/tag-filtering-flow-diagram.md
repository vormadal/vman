# Tag Filtering - User Flow Diagram

## Flow 1: Filtering Tags (Tagging Mode)

```
User Action: Types in tag input
     |
     v
Input: "nat"
     |
     v
System: Filter tags (case insensitive)
     |
     v
Display: Available Tags List
     |
     +-- "nature" (5 items)
     +-- "national" (3 items)
     +-- "Natural Beauty" (12 items)
```

## Flow 2: Creating New Tag (No Match)

```
User Input: "uniquetag123"
     |
     v
Check: Any exact match? (case insensitive)
     |
     v
Result: No match found
     |
     v
Action: Create new tag "uniquetag123"
     |
     v
Auto-add: Add to current item
     |
     v
Toast: "Tag created - 'uniquetag123' has been created and added."
```

## Flow 3: Using Existing Tag (Exact Match)

```
User Input: "NATURE"
     |
     v
Check: Any exact match? (case insensitive)
     |
     v
Result: Match found → "nature" exists
     |
     v
Action: Use existing tag (NO new tag created)
     |
     v
Add: Add "nature" to current item
     |
     v
Toast: "Tag added - 'nature' has been added to the item."
```

## Flow 4: Duplicate Prevention (Tag Dialog)

```
User: Click "Create Tag" button
     |
     v
Dialog: Opens with input field
     |
     v
User Input: "Vacation"
     |
     v
User: Presses Enter or clicks Create
     |
     v
Check: Any exact match? (case insensitive)
     |
     v
Result: Match found → "vacation" exists
     |
     v
Action: PREVENT duplicate creation
     |
     v
Toast: "Tag already exists - A tag named 'vacation' already exists."
     |
     v
Dialog: Closes
```

## Component Interaction

```
┌─────────────────────────────────────────────────────────┐
│ Tagging Mode Page                                       │
│                                                          │
│  ┌────────────────────────────────────────────┐         │
│  │ Input: "Enter tag name..."                 │         │
│  │ Value: newTagName (state)                  │         │
│  └────────────────────────────────────────────┘         │
│           │                                              │
│           │ onChange                                     │
│           v                                              │
│  ┌────────────────────────────────────────────┐         │
│  │ useMemo: filteredTags                      │         │
│  │ - Filter by newTagName (case insensitive)  │         │
│  └────────────────────────────────────────────┘         │
│           │                                              │
│           │ render                                       │
│           v                                              │
│  ┌────────────────────────────────────────────┐         │
│  │ Available Tags List                        │         │
│  │ - Shows filteredTags                       │         │
│  │ - Click to add tag                         │         │
│  └────────────────────────────────────────────┘         │
│                                                          │
│  ┌────────────────────────────────────────────┐         │
│  │ Helper Text (context-aware)                │         │
│  │ - getInputHelperText()                     │         │
│  └────────────────────────────────────────────┘         │
│                                                          │
│  User presses Enter → handleCreateTag()                 │
│           │                                              │
│           v                                              │
│  ┌────────────────────────────────────────────┐         │
│  │ Check for exact match (case insensitive)   │         │
│  └────────────────────────────────────────────┘         │
│           │                                              │
│           ├─ Match found                                 │
│           │      │                                       │
│           │      v                                       │
│           │  Add existing tag to item                    │
│           │  Toast: "Tag added"                          │
│           │                                              │
│           └─ No match                                    │
│                  │                                       │
│                  v                                       │
│              Create new tag                              │
│              Add to item                                 │
│              Toast: "Tag created"                        │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

## Data Flow

```
tagsData (from API)
     │
     │ useTags() React Query
     v
All tags in system
     │
     │ Filter: Remove tags already on item
     v
sortedTags (available tags)
     │
     │ Filter: By search text (newTagName)
     v
filteredTags (displayed tags)
     │
     │ User interaction
     v
Selected/Created tag
     │
     │ addTagMutation / createTagMutation
     v
Updated item with tag
     │
     │ React Query invalidation
     v
UI updates automatically
```

## State Dependencies

```
newTagName (user input)
     |
     +──> filteredTags (memoized)
     |         |
     |         +──> Rendered tag list
     |
     +──> getInputHelperText() (helper function)
     |         |
     |         +──> Helper text display
     |
     +──> getEmptyStateMessage() (helper function)
     |         |
     |         +──> Empty state message
     |
     +──> handleCreateTag()
               |
               +──> exactMatch check
                         |
                         +──> Add existing OR create new
```

## Key Benefits of This Implementation

1. **Single Source of Truth**: `newTagName` drives all UI states
2. **Memoization**: `filteredTags` only recalculates when needed
3. **Separation of Concerns**: Helper functions keep JSX clean
4. **Consistency**: Same duplicate detection logic in all 3 locations
5. **User Feedback**: Clear messages at every step
6. **Performance**: Efficient filtering with memoization
