import { Page, Locator, expect } from '@playwright/test';
import { BasePage } from './base.page';

export class CollectionsPage extends BasePage {
  readonly heading: Locator;
  readonly newCollectionButton: Locator;

  constructor(page: Page) {
    super(page);
    this.heading = page.getByRole('heading', { name: /^collections$/i });
    this.newCollectionButton = page.getByRole('button', { name: /new collection/i });
  }

  async goto() {
    await super.goto('/collections');
  }

  async expectToBeVisible() {
    await expect(this.heading).toBeVisible();
  }

  async createCollection(name: string, description?: string) {
    await this.newCollectionButton.click();
    await this.page.getByLabel('Name').fill(name);
    if (description) {
      await this.page.getByLabel(/description/i).fill(description);
    }
    await this.page.getByRole('button', { name: 'Create', exact: true }).click();
    await expect(this.page.getByRole('dialog')).not.toBeVisible({ timeout: 5000 });
  }

  async openCollection(name: string) {
    await this.page.getByRole('link', { name }).click();
  }

  async deleteCollection(name: string) {
    const heading = this.page.getByRole('heading', { name, exact: true });
    // Delete button is inside the CardTitle (h3) alongside the link
    await heading.getByRole('button').click();
    // Confirm deletion in alert dialog
    await this.page.getByRole('alertdialog').getByRole('button', { name: 'Delete' }).click();
  }

  async expectCollectionExists(name: string) {
    await expect(this.page.getByRole('link', { name })).toBeVisible({ timeout: 5000 });
  }

  async expectCollectionNotExists(name: string) {
    await expect(this.page.getByRole('link', { name })).not.toBeVisible({ timeout: 5000 });
  }
}
