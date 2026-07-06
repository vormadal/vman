import { test, expect } from '@playwright/test';
import { CollectionsPage } from './pages/collections.page';
import { CollectionDetailPage } from './pages/collection-detail.page';
import { ItemsPage } from './pages/items.page';
import { ensureItemsSynced } from './helpers/sync.helper';

test.describe('Collection management and Shotcut export', () => {
  test('should create a collection, add items via collection mode, and export to Shotcut', async ({ page }) => {
    await ensureItemsSynced(page);

    const collectionsPage = new CollectionsPage(page);
    const itemsPage = new ItemsPage(page);
    const collectionName = `E2E Export ${Date.now()}`;
    let createdCollectionName: string | null = null;

    try {
      // Create the collection
      await collectionsPage.goto();
      await collectionsPage.expectToBeVisible();
      await collectionsPage.createCollection(collectionName, 'E2E test: add items and export');
      await collectionsPage.expectCollectionExists(collectionName);
      createdCollectionName = collectionName;

      // Navigate to detail — collection starts empty
      await collectionsPage.openCollection(collectionName);
      const detailPage = new CollectionDetailPage(page);
      await detailPage.expectToBeVisible(collectionName);
      await detailPage.expectEmptyState();

      // Enter collection mode → redirects to /items with overlay dock bar
      await detailPage.enterCollectionMode();
      await expect(page).toHaveURL(/\/items/);
      await itemsPage.expectCollectionOverlayActive(collectionName);

      // Add the first visible item via its "Add to collection" button
      await itemsPage.addFirstItemToCollection();

      // Overlay badge updates to reflect the added item
      await itemsPage.expectCollectionOverlayItemCount(1);

      // Navigate back to the collection detail via the overlay Manage link
      await itemsPage.navigateToManageCollection();
      await detailPage.expectToBeVisible(collectionName);

      // Detail page shows the item count
      await detailPage.expectItemCount(1);

      // Export to Shotcut — should trigger a zip file download
      const downloadPromise = page.waitForEvent('download');
      await detailPage.exportButton.click();
      const download = await downloadPromise;
      expect(download.suggestedFilename()).toMatch(/\.zip$/);

      // Clean up — delete the collection
      await collectionsPage.goto();
      await collectionsPage.deleteCollection(collectionName);
      await collectionsPage.expectCollectionNotExists(collectionName);
      createdCollectionName = null;
    } finally {
      // Ensure cleanup even if the test fails mid-way
      if (createdCollectionName) {
        try {
          await collectionsPage.goto();
          await collectionsPage.deleteCollection(createdCollectionName);
        } catch {
          // Collection may already be gone
        }
      }
    }
  });
});
