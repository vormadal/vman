import { Page, Locator, expect } from '@playwright/test';
import { BasePage } from './base.page';

export class CollectionDetailPage extends BasePage {
  readonly backLink: Locator;
  readonly collectionModeButton: Locator;
  readonly exportButton: Locator;
  readonly emptyState: Locator;

  constructor(page: Page) {
    super(page);
    this.backLink = page.getByRole('link', { name: /back to collections/i });
    this.collectionModeButton = page.getByRole('button', { name: /collection mode/i });
    this.exportButton = page.getByRole('button', { name: /export to shotcut/i });
    this.emptyState = page.getByText(/no items in collection/i);
  }

  async expectToBeVisible(collectionName: string) {
    await expect(this.page.getByRole('heading', { name: collectionName })).toBeVisible();
  }

  async expectEmptyState() {
    await expect(this.emptyState).toBeVisible();
  }

  async goBack() {
    await this.backLink.click();
  }

  async enterCollectionMode() {
    await this.collectionModeButton.click();
  }

  async expectItemCount(count: number) {
    // The detail page renders "1 item" (singular) or "N items" (plural)
    await expect(this.page.getByText(new RegExp(`^${count} items?$`))).toBeVisible({ timeout: 5000 });
  }
}
