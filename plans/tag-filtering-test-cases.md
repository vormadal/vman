# Tag Filtering - Manual Verification Guide

## Test Cases

### Test Case 1: Tag Filtering in Tagging Mode
**Location**: `/items/tagging` page

**Steps**:
1. Navigate to the tagging mode
2. In the "Enter tag name..." input, type a partial tag name (e.g., "nat")
3. Observe the "Available Tags" list

**Expected Results**:
- The available tags list should filter to show only tags containing "nat" (case insensitive)
- Tags like "nature", "national", "Natural" should be visible
- Tags not matching the filter should be hidden
- The helper text should say "Press Enter to add or create tag"

**Pass/Fail**: ___

---

### Test Case 2: Create New Tag When No Match
**Location**: `/items/tagging` page

**Steps**:
1. In the "Enter tag name..." input, type a unique tag name (e.g., "newuniquetag123")
2. Verify no tags match in the filtered list
3. Press Enter

**Expected Results**:
- A new tag "newuniquetag123" should be created
- The tag should be automatically added to the current item
- Toast should show "Tag created" with description mentioning the new tag
- Input should be cleared
- The new tag should appear in the item's tag list

**Pass/Fail**: ___

---

### Test Case 3: Use Existing Tag on Exact Match (Same Case)
**Location**: `/items/tagging` page

**Prerequisites**: A tag named "nature" exists

**Steps**:
1. In the "Enter tag name..." input, type "nature"
2. Press Enter

**Expected Results**:
- No new tag should be created
- The existing "nature" tag should be added to the item
- Toast should show "Tag added" (NOT "Tag created")
- Input should be cleared

**Pass/Fail**: ___

---

### Test Case 4: Use Existing Tag on Exact Match (Different Case)
**Location**: `/items/tagging` page

**Prerequisites**: A tag named "nature" exists

**Steps**:
1. In the "Enter tag name..." input, type "NATURE" (all caps)
2. Press Enter

**Expected Results**:
- No new tag should be created
- The existing "nature" tag should be added to the item (with original casing)
- Toast should show "Tag added" with the original tag name "nature"
- No duplicate tag "NATURE" should exist
- Input should be cleared

**Pass/Fail**: ___

---

### Test Case 5: Prevent Duplicate in Tags Management
**Location**: `/tags` page

**Prerequisites**: A tag named "vacation" exists

**Steps**:
1. Click "Create Tag" button to open the dialog
2. Enter "Vacation" (different case)
3. Press Enter or click Create

**Expected Results**:
- No new tag should be created
- Toast should show "Tag already exists" with message mentioning "vacation"
- Dialog should close
- Only one "vacation" tag should exist in the list

**Pass/Fail**: ___

---

### Test Case 6: Prevent Duplicate in Items Page
**Location**: `/items` page

**Prerequisites**: A tag named "family" exists

**Steps**:
1. Click "Add Tag" button to open the dialog
2. Enter "FAMILY" (all caps)
3. Press Enter or click Create Tag

**Expected Results**:
- No new tag should be created
- Toast should show "Tag already exists" with message mentioning "family"
- Dialog should close
- No duplicate tag "FAMILY" should exist

**Pass/Fail**: ___

---

### Test Case 7: Empty State Messages
**Location**: `/items/tagging` page

**Steps**:
1. Clear the tag input
2. Observe the "Available Tags" section when:
   - Current item has no tags and no tags exist in the system
   - Current item has no tags but tags exist in the system
   - All available tags have been added to the current item
3. Type a search term that matches no tags

**Expected Results**:
- When no tags exist: "No tags available. Create one above!"
- When tags exist but all added: "All tags have been added to this item."
- When search has no matches: "No matching tags found. Press Enter to create a new tag."

**Pass/Fail**: ___

---

### Test Case 8: Filter Responsiveness
**Location**: `/items/tagging` page

**Steps**:
1. Type "a" in the tag input
2. Observe the filtered list
3. Type "ni" (making it "ani")
4. Observe the filtered list
5. Delete characters back to "a"

**Expected Results**:
- Filtering should update in real-time as you type
- Tags should appear/disappear based on whether they contain the search text
- The filter should be case insensitive
- Performance should be smooth with no lag

**Pass/Fail**: ___

---

### Test Case 9: Click to Add Filtered Tag
**Location**: `/items/tagging` page

**Steps**:
1. Type "nat" in the tag input to filter tags
2. Click on one of the filtered tags (e.g., "nature")

**Expected Results**:
- The clicked tag should be added to the current item
- The tag should disappear from the "Available Tags" list (since it's now on the item)
- The filter text should remain in the input
- The filtered list should update to exclude the newly added tag

**Pass/Fail**: ___

---

## Summary

Total Test Cases: 9
Passed: ___
Failed: ___

## Notes
(Add any observations or issues found during testing)
