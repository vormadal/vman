import { Page, Locator, expect } from '@playwright/test';
import { BasePage } from './base.page';

export class TagsPage extends BasePage {
  readonly heading: Locator;
  readonly searchInput: Locator;
  readonly createTagButton: Locator;

  constructor(page: Page) {
    super(page);
    this.heading = page.getByRole('heading', { name: /^tags$/i });
    this.searchInput = page.getByPlaceholder(/search tags/i);
    this.createTagButton = page.getByRole('button', { name: 'Create Tag' });
  }

  async goto() {
    await super.goto('/tags');
  }

  async expectToBeVisible() {
    await expect(this.heading).toBeVisible();
  }

  async createTag(name: string) {
    await this.createTagButton.click();
    await this.page.locator('#tagName').fill(name);
    await this.page.getByRole('button', { name: 'Create', exact: true }).click();
    await expect(this.page.getByRole('dialog')).not.toBeVisible({ timeout: 5000 });
  }

  async renameTag(currentName: string, newName: string) {
    await this.page.getByRole('button', { name: `Edit tag ${currentName}`, exact: true }).click();

    await this.page.locator('#editTagName').fill(newName);
    await this.page.getByRole('button', { name: 'Rename' }).click();
    await expect(this.page.getByRole('dialog')).not.toBeVisible({ timeout: 5000 });
  }

  async deleteTag(name: string) {
    await this.page.getByRole('button', { name: `Delete tag ${name}`, exact: true }).click();

    await this.page.getByRole('button', { name: 'Delete', exact: true }).click();
    await expect(this.page.getByRole('dialog')).not.toBeVisible({ timeout: 5000 });
  }

  async expectTagExists(name: string) {
    await expect(this.page.getByRole('heading', { name, exact: true })).toBeVisible({ timeout: 5000 });
  }

  async expectTagNotExists(name: string) {
    await expect(this.page.getByRole('heading', { name, exact: true })).not.toBeVisible({ timeout: 5000 });
  }
}
