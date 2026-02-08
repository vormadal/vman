import { Page } from '@playwright/test';

/**
 * Ensures items have been synced from the provider.
 * Triggers a sync if no items are found.
 */
export async function ensureItemsSynced(page: Page) {
  await page.goto('/items');
  await page.waitForLoadState('domcontentloaded');

  const hasItems = await page
    .getByText(/showing \d+ of \d+/i)
    .isVisible({ timeout: 5000 })
    .catch(() => false);

  if (hasItems) return;

  // No items found, trigger sync
  await page.goto('/sync');
  await page.getByRole('button', { name: /sync now/i }).click();
  await page.getByText('Sync completed successfully').waitFor({ timeout: 30000 });
}
