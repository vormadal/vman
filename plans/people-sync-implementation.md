# People Sync Implementation

## Overview
This implementation adds support for syncing people from Immich and displaying them in the Video Manager application. Users can now filter items by person and see which people appear in each media item.

## Backend Changes

### Database Schema
Created two new tables:
- **People**: Stores person information from Immich
  - Id (GUID, PK)
  - ProviderName (string, indexed)
  - ProviderItemId (string, indexed with ProviderName)
  - Name (string, required)
  - BirthDate (DateOnly, nullable)
  - ThumbnailPath (string, nullable)
  - IsFavorite (bool)
  - IsHidden (bool)
  - UpdatedAt (DateTimeOffset)
  - LastSyncedAt (DateTimeOffset)

- **ItemPeople**: Junction table linking items to people (many-to-many)
  - Id (GUID, PK)
  - PersonId (GUID, FK to People, indexed)
  - ProviderName + ProviderItemId (composite key to Items, indexed)
  - CreatedAt (DateTimeOffset)

### Immich Service
Added methods to `IImmichService`:
- `GetPeopleAsync(bool withHidden)`: Retrieves all people from Immich
- `GetPersonAsync(Guid personId)`: Gets a single person by ID
- `GetAssetsForPersonAsync(Guid personId)`: Streams all assets for a specific person

These methods use the existing Kiota-generated Immich API client.

### Sync Process
Updated `ImmichSyncProcessor` to sync people after assets:
1. Fetches all people from Immich (including hidden)
2. Creates or updates person records in the database
3. For each person, fetches their associated assets from Immich
4. Creates ItemPerson relationships for assets that exist in our database
5. Removes stale relationships that no longer exist in Immich

### API Endpoints
Added new endpoints in `/api/people`:
- `GET /api/people`: List all people with pagination and search
  - Query params: `search`, `page`, `pageSize`
  - Returns: People list with item counts

- `GET /api/people/{id}`: Get a single person by ID
  - Returns: Person details with item count

### Items Filtering
Updated `GetItems` handler to support person filtering:
- Added `PersonId` parameter to filter items by person
- Included people data in item DTOs alongside tags
- Batch loads people relationships to avoid N+1 queries

## Frontend Changes

### Type Definitions
Added new TypeScript types in `types.ts`:
- `ItemPersonDto`: Person reference on an item
- `PersonDto`: Full person details with item count
- `PeopleResponse`: Paginated people list response
- `PersonDetailResponse`: Single person response

Updated `ItemDto` to include `people: ItemPersonDto[]` field.
Updated `GetItemsParams` to include `personId?: string` filter.

### API Client
Added methods to `apiClient`:
- `getPeople(search?, page, pageSize)`: Fetch people list
- `getPersonById(id)`: Fetch single person details

### React Hooks
Added React Query hooks in `useApi.ts`:
- `usePeople(search?)`: Query hook for fetching people
- `usePerson(id)`: Query hook for fetching a single person

### UI Updates
Modified `/items` page to:
1. Add a "Filter by Person" section similar to tag filtering
2. Display person badges showing:
   - Person name
   - Item count
   - Selected/unselected state
3. Show people on item cards as badges with user icon
4. Filter hidden people from the UI

Person badges are displayed below tag badges on item cards with a user icon to distinguish them from tags.

## Database Migration
Migration file: `20260207130217_AddPeopleAndItemPeople.cs`
- Creates `People` table with indexes
- Creates `ItemPeople` table with composite unique constraint
- Adds navigation properties to `Items` table

## Usage

### Running a Sync
1. Navigate to the Sync page in the UI
2. Trigger a sync (existing functionality)
3. The sync will now automatically sync people after syncing assets
4. People and their relationships will appear in the database

### Filtering by Person
1. Navigate to the Items page
2. Click on a person badge in the "Filter by Person" section
3. Items will be filtered to show only those containing that person
4. Click the person badge again or "All" to clear the filter

### Viewing People on Items
- People appear as outline badges on item cards
- Each badge shows the person's name with a user icon
- People badges are read-only (no remove button)

## Technical Notes

### Date Handling
- Immich's `Date` type (from Kiota) is converted to `DateOnly` using individual Year/Month/Day properties
- This avoids timezone issues and matches the semantic meaning of a birth date

### Sync Efficiency
- People sync happens after asset sync to ensure all items exist in the database
- Batch operations minimize database round-trips
- Existing relationships are preserved when they still exist in Immich
- Stale relationships are removed to keep data in sync

### Frontend State Management
- People data is fetched via React Query with caching
- Filters are local component state
- Item data includes embedded people information to avoid additional requests

## Future Enhancements
Potential improvements for future iterations:
1. Add ability to manually tag people on items
2. Person detail page showing all items for that person
3. Person thumbnail display
4. Support for favoriting people
5. Batch operations on people (hide/unhide, merge)
6. Face detection integration
