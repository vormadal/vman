import { test } from '@playwright/test';
import { SyncPage } from './pages/sync.page';

test.describe('Sync', () => {
  test('should trigger sync and complete successfully', async ({ page }) => {
    const syncPage = new SyncPage(page);
    await syncPage.goto();
    await syncPage.expectToBeVisible();

    // Trigger sync
    await syncPage.clickSyncNow();

    // Wait for sync to complete (stub data is small, should be fast)
    await syncPage.expectSyncCompleted();
  });
});
