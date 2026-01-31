import { Page, expect } from '@playwright/test';

/**
 * Base Page Object class with common functionality
 * All page objects should extend this class
 */
export class BasePage {
  readonly page: Page;

  constructor(page: Page) {
    this.page = page;
  }

  /**
   * Navigate to a specific path
   */
  async goto(path: string) {
    await this.page.goto(path);
  }

  /**
   * Wait for the page to load completely
   */
  async waitForLoad() {
    await this.page.waitForLoadState('domcontentloaded');
  }

  /**
   * Wait for network to be idle
   */
  async waitForNetworkIdle() {
    await this.page.waitForLoadState('networkidle');
  }

  /**
   * Get the current URL
   */
  async getCurrentUrl(): Promise<string> {
    return this.page.url();
  }

  /**
   * Verify the current URL matches a pattern
   */
  async expectUrl(pattern: RegExp) {
    await expect(this.page).toHaveURL(pattern);
  }

  /**
   * Verify the page title
   */
  async expectTitle(pattern: RegExp) {
    await expect(this.page).toHaveTitle(pattern);
  }
}
