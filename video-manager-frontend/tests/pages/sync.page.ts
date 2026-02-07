import { Page, Locator, expect } from '@playwright/test';
import { BasePage } from './base.page';

/**
 * Page Object Model for the Sync page
 */
export class SyncPage extends BasePage {
  readonly heading: Locator;
  readonly syncNowButton: Locator;
  readonly cancelButton: Locator;
  readonly statusBadge: Locator;
  readonly successMessage: Locator;
  readonly itemsSyncedValue: Locator;

  constructor(page: Page) {
    super(page);

    this.heading = page.getByRole('heading', { name: /sync/i }).first();
    this.syncNowButton = page.getByRole('button', { name: /sync now/i });
    this.cancelButton = page.getByRole('button', { name: /cancel/i });
    this.statusBadge = page.locator('.inline-flex').filter({ hasText: /Completed|Failed|InProgress|Pending|Cancelled/ });
    this.successMessage = page.getByText('Sync completed successfully');
    this.itemsSyncedValue = page.getByText(/items have been synced/);
  }

  async goto() {
    await super.goto('/sync');
  }

  async expectToBeVisible() {
    await expect(this.heading).toBeVisible();
  }

  async clickSyncNow() {
    await this.syncNowButton.click();
  }

  async expectSyncCompleted() {
    await expect(this.successMessage).toBeVisible({ timeout: 30000 });
  }

  async expectStatusBadge(status: string) {
    await expect(this.page.getByText(status).first()).toBeVisible({ timeout: 30000 });
  }
}
