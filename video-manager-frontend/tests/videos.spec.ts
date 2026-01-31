import { test, expect } from './fixtures/authenticated';
import { VideosPage } from './pages/videos.page';

test.describe('Videos Page (Authenticated)', () => {
  test.use({ storageState: 'playwright/.auth/user.json' });

  test('should display videos page', async ({ page }) => {
    const videosPage = new VideosPage(page);
    await videosPage.goto();
    await videosPage.expectUrl(/.*videos/);
    await videosPage.expectToBeVisible();
  });

  test('should have video controls', async ({ page }) => {
    const videosPage = new VideosPage(page);
    await videosPage.goto();
    
    // Check for video page controls
    await videosPage.expectControlsToBeVisible();
  });
});
