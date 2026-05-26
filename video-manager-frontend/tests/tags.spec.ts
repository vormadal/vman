import { test } from '@playwright/test';
import { TagsPage } from './pages/tags.page';

test.describe('Tags Management', () => {
  test('should create, rename, and delete a tag', async ({ page }) => {
    const tagsPage = new TagsPage(page);
    const tagName = `E2E Tag ${Date.now()}`;
    const renamedTag = `E2E Renamed ${Date.now()}`;
    let createdTagName: string | null = null;

    try {
      await tagsPage.goto();
      await tagsPage.expectToBeVisible();

      // Create tag
      await tagsPage.createTag(tagName);
      await tagsPage.expectTagExists(tagName);
      createdTagName = tagName;

      // Rename tag
      await tagsPage.renameTag(tagName, renamedTag);
      await tagsPage.expectTagExists(renamedTag);
      await tagsPage.expectTagNotExists(tagName);
      createdTagName = renamedTag;

      // Delete tag
      await tagsPage.deleteTag(renamedTag);
      await tagsPage.expectTagNotExists(renamedTag);
      createdTagName = null;
    } finally {
      // Cleanup: ensure tag is deleted even if test fails
      if (createdTagName) {
        try {
          await tagsPage.goto();
          await tagsPage.deleteTag(createdTagName);
        } catch {
          // Tag might already be deleted, ignore errors
        }
      }
    }
  });
});
