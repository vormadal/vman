import { test } from '@playwright/test';
import { ItemsPage } from './pages/items.page';
import { ensureItemsSynced } from './helpers/sync.helper';

test.describe('Search and filtering', () => {
  test('should filter items by Videos media type', async ({ page }) => {
    await ensureItemsSynced(page);

    const itemsPage = new ItemsPage(page);
    await itemsPage.goto();
    await itemsPage.expectToBeVisible();
    await itemsPage.expectItemsLoaded();

    // Apply Videos filter and verify it takes effect
    await itemsPage.filterByMediaType('Videos');
    await page.waitForLoadState('networkidle');
    await itemsPage.expectToBeVisible();
    await itemsPage.expectActiveMediaTypeFilter();

    // Clearing the filter should restore the full item list
    await itemsPage.clearMediaTypeFilter();
    await page.waitForLoadState('networkidle');
    await itemsPage.expectItemsLoaded();
    await itemsPage.expectNoActiveMediaTypeFilter();
  });

  test('should show matching filter suggestions as you type', async ({ page }) => {
    await ensureItemsSynced(page);

    const itemsPage = new ItemsPage(page);
    await itemsPage.goto();
    await itemsPage.expectToBeVisible();

    // Typing a partial media type name opens the suggestions dropdown
    await itemsPage.filterInput.click();
    await itemsPage.filterInput.fill('vid');
    await itemsPage.expectFilterSuggestion('Videos');

    // Switching the search term updates suggestions
    await itemsPage.filterInput.fill('img');
    await itemsPage.expectFilterSuggestion('Images');

    // Pressing Escape closes the dropdown without applying any filter
    await itemsPage.filterInput.press('Escape');
    await itemsPage.expectNoFilterSuggestion('Videos');
    await itemsPage.expectNoFilterSuggestion('Images');
    await itemsPage.expectNoActiveMediaTypeFilter();
  });
});
