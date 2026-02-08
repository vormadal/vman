import { test, expect } from '@playwright/test';
import { ItemsPage } from './pages/items.page';
import { ensureItemsSynced } from './helpers/sync.helper';

test.describe('Items', () => {
  test('should display items and support filtering', async ({ page }) => {
    await ensureItemsSynced(page);

    const itemsPage = new ItemsPage(page);
    await itemsPage.goto();
    await itemsPage.expectToBeVisible();
    await itemsPage.expectItemsLoaded();

    // Filter by Images
    await itemsPage.filterByMediaType('Images');
    await page.waitForLoadState('networkidle');
    await itemsPage.expectToBeVisible();

    // Reset filter
    await itemsPage.filterByMediaType('All');
    await page.waitForLoadState('networkidle');
    await itemsPage.expectItemsLoaded();

    // Navigate to tagging mode
    await itemsPage.clickTaggingMode();
    await expect(page).toHaveURL(/\/items\/tagging/);
  });
});
