import { test } from '@playwright/test';
import { TagsPage } from './pages/tags.page';

test.describe('Tags Management', () => {
  test('should create, rename, and delete a tag', async ({ page }) => {
    const tagsPage = new TagsPage(page);
    const tagName = `E2E Tag ${Date.now()}`;
    const renamedTag = `E2E Renamed ${Date.now()}`;

    await tagsPage.goto();
    await tagsPage.expectToBeVisible();

    // Create tag
    await tagsPage.createTag(tagName);
    await tagsPage.expectTagExists(tagName);

    // Rename tag
    await tagsPage.renameTag(tagName, renamedTag);
    await tagsPage.expectTagExists(renamedTag);
    await tagsPage.expectTagNotExists(tagName);

    // Delete tag
    await tagsPage.deleteTag(renamedTag);
    await tagsPage.expectTagNotExists(renamedTag);
  });
});
