import { test } from '@playwright/test';
import { TaggingPage } from './pages/tagging.page';
import { ensureItemsSynced } from './helpers/sync.helper';

test.describe('Tagging Mode', () => {
  test('should navigate items and add/remove tags', async ({ page }) => {
    await ensureItemsSynced(page);

    const taggingPage = new TaggingPage(page);
    await taggingPage.goto();
    await taggingPage.expectToBeVisible();

    // Verify counter shows first item
    await taggingPage.expectCounterText(/^1 of \d+$/);

    // Create and add a tag to current item
    const tagName = `E2E Tagging ${Date.now()}`;
    await taggingPage.createAndAddTag(tagName);

    // Verify tag appears on the item overlay
    await taggingPage.expectTagInOverlay(tagName);

    // Remove the tag from overlay
    await taggingPage.removeTagFromOverlay(tagName);

    // Verify tag moved to available tags list
    await taggingPage.expectTagInAvailableList(tagName);

    // Navigate to next item
    await taggingPage.clickNext();
    await taggingPage.expectCounterText(/^2 of \d+$/);
  });
});
