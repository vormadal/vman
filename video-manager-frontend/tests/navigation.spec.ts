import { test, expect } from '@playwright/test';
import { NavigationPage } from './pages/navigation.page';

test.describe('Navigation', () => {
  test('should navigate to all major pages via drawer', async ({ page }) => {
    const nav = new NavigationPage(page);

    await page.goto('/items');
    await page.waitForLoadState('domcontentloaded');

    // Navigate to Collections
    await nav.navigateToCollections();
    await expect(page).toHaveURL(/\/collections/);

    // Navigate to Tags
    await nav.navigateToTags();
    await expect(page).toHaveURL(/\/tags/);

    // Navigate to Sync
    await nav.navigateToSync();
    await expect(page).toHaveURL(/\/sync/);

    // Navigate back to Items
    await nav.navigateToItems();
    await expect(page).toHaveURL(/\/items/);
  });
});
