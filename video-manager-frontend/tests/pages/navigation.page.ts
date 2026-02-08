import { Page, Locator, expect } from '@playwright/test';
import { BasePage } from './base.page';

export class NavigationPage extends BasePage {
  readonly menuButton: Locator;
  readonly itemsLink: Locator;
  readonly collectionsLink: Locator;
  readonly tagsLink: Locator;
  readonly syncLink: Locator;
  readonly logoutButton: Locator;

  constructor(page: Page) {
    super(page);
    this.menuButton = page.getByRole('button', { name: /toggle navigation menu/i });
    this.itemsLink = page.getByRole('link', { name: /Items.*Browse all media/i });
    this.collectionsLink = page.getByRole('link', { name: /Collections.*Manage collections/i });
    this.tagsLink = page.getByRole('link', { name: /Tags.*Manage tags/i });
    this.syncLink = page.getByRole('link', { name: /Sync.*Sync with providers/i });
    this.logoutButton = page.getByRole('button', { name: /logout/i });
  }

  async openDrawer() {
    await this.menuButton.click();
  }

  async navigateTo(link: Locator) {
    await this.openDrawer();
    await link.click();
    await this.page.waitForLoadState('domcontentloaded');
  }

  async navigateToItems() {
    await this.navigateTo(this.itemsLink);
  }

  async navigateToCollections() {
    await this.navigateTo(this.collectionsLink);
  }

  async navigateToTags() {
    await this.navigateTo(this.tagsLink);
  }

  async navigateToSync() {
    await this.navigateTo(this.syncLink);
  }

  async logout() {
    await this.openDrawer();
    await this.logoutButton.click();
  }
}
