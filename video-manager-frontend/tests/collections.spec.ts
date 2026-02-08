import { test, expect } from '@playwright/test';
import { CollectionsPage } from './pages/collections.page';
import { CollectionDetailPage } from './pages/collection-detail.page';
import { ensureItemsSynced } from './helpers/sync.helper';

test.describe('Collections', () => {
  test('should create, view detail, enter collection mode, and delete', async ({ page }) => {
    await ensureItemsSynced(page);

    const collectionsPage = new CollectionsPage(page);
    const collectionName = `E2E Collection ${Date.now()}`;
    let createdCollectionName: string | null = null;

    try {
      // Create collection
      await collectionsPage.goto();
      await collectionsPage.expectToBeVisible();
      await collectionsPage.createCollection(collectionName, 'Test description');
      await collectionsPage.expectCollectionExists(collectionName);
      createdCollectionName = collectionName;

      // Navigate to collection detail
      await collectionsPage.openCollection(collectionName);
      const detailPage = new CollectionDetailPage(page);
      await detailPage.expectToBeVisible(collectionName);
      await detailPage.expectEmptyState();

      // Enter collection mode - redirects to /items with overlay
      await detailPage.enterCollectionMode();
      await expect(page).toHaveURL(/\/items/);
      await expect(page.getByText('Collection Mode')).toBeVisible();
      await expect(page.getByText(collectionName)).toBeVisible();

      // Go back to collections and delete
      await collectionsPage.goto();
      await collectionsPage.expectToBeVisible();
      await collectionsPage.deleteCollection(collectionName);
      await collectionsPage.expectCollectionNotExists(collectionName);
      createdCollectionName = null;
    } finally {
      // Cleanup: ensure collection is deleted even if test fails
      if (createdCollectionName) {
        try {
          await collectionsPage.goto();
          await collectionsPage.deleteCollection(createdCollectionName);
        } catch {
          // Collection might already be deleted, ignore errors
        }
      }
    }
  });
});
