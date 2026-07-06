import { Page, Locator, expect } from '@playwright/test';
import { BasePage } from './base.page';

export class ItemsPage extends BasePage {
  readonly heading: Locator;
  readonly taggingModeLink: Locator;
  readonly collectionsLink: Locator;
  readonly filterInput: Locator;
  readonly itemCountText: Locator;

  constructor(page: Page) {
    super(page);
    this.heading = page.getByRole('heading', { name: /media items/i });
    this.taggingModeLink = page.getByRole('link', { name: /tagging mode/i });
    this.collectionsLink = page.getByRole('link', { name: /^Collections$/i });
    this.filterInput = page.getByPlaceholder('Add filter...');
    this.itemCountText = page.getByText(/showing \d+ of \d+/i);
  }

  async goto() {
    await super.goto('/items');
  }

  async expectToBeVisible() {
    await expect(this.heading).toBeVisible();
  }

  async expectItemsLoaded() {
    await expect(this.itemCountText).toBeVisible({ timeout: 10000 });
  }

  async filterByMediaType(type: string) {
    await this.filterInput.click();
    await this.filterInput.fill(type);
    await this.page.getByRole('button', { name: new RegExp(`^${type}$`, 'i') }).click();
  }

  async clearMediaTypeFilter() {
    await this.page.getByRole('button', { name: /remove media type filter/i }).click();
  }

  async clickTaggingMode() {
    await this.taggingModeLink.click();
  }

  async clickCollections() {
    await this.collectionsLink.click();
  }

  async createTag(name: string) {
    await this.filterInput.click();
    await this.filterInput.fill(name);
    await this.page.getByRole('button', { name: `Create tag "${name}"` }).click();
    await this.page.getByRole('button', { name: /^create tag$/i }).click();
    await expect(this.page.getByRole('dialog')).not.toBeVisible({ timeout: 5000 });
  }
}
