import { Page, Locator, expect } from '@playwright/test';
import { BasePage } from './base.page';

export class TaggingPage extends BasePage {
  readonly heading: Locator;
  readonly previousButton: Locator;
  readonly nextButton: Locator;
  readonly newTagInput: Locator;
  readonly itemCounter: Locator;

  constructor(page: Page) {
    super(page);
    this.heading = page.getByRole('heading', { name: /tagging mode/i });
    this.previousButton = page.getByRole('button', { name: /previous/i });
    this.nextButton = page.getByRole('button', { name: 'Next', exact: true });
    this.newTagInput = page.getByPlaceholder(/enter tag name/i);
    this.itemCounter = page.getByText(/^\d+ of \d+$/);
  }

  async goto() {
    await super.goto('/items/tagging');
  }

  async expectToBeVisible() {
    await expect(this.heading).toBeVisible();
  }

  async createAndAddTag(name: string) {
    await this.newTagInput.fill(name);
    await this.newTagInput.press('Enter');
  }

  async clickNext() {
    await this.nextButton.click();
  }

  async clickPrevious() {
    await this.previousButton.click();
  }

  async removeTagFromOverlay(tagName: string) {
    await this.page.locator('.absolute').getByText(tagName, { exact: true }).click();
  }

  async expectTagInOverlay(tagName: string) {
    await expect(
      this.page.locator('.absolute').getByText(tagName, { exact: true })
    ).toBeVisible({ timeout: 5000 });
  }

  async expectTagInAvailableList(tagName: string) {
    await expect(
      this.page.getByRole('button', { name: new RegExp(`Add tag ${tagName}`, 'i') })
    ).toBeVisible({ timeout: 5000 });
  }

  async expectCounterText(pattern: RegExp) {
    await expect(this.itemCounter.filter({ hasText: pattern })).toBeVisible();
  }
}
