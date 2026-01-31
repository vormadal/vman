import { Page, Locator, expect } from '@playwright/test';
import { BasePage } from './base.page';

/**
 * Page Object Model for the Videos page
 */
export class VideosPage extends BasePage {
  // Locators
  readonly heading: Locator;
  readonly searchInput: Locator;
  readonly addButton: Locator;
  readonly uploadButton: Locator;
  readonly videoGrid: Locator;
  readonly videoCards: Locator;

  constructor(page: Page) {
    super(page);
    
    // Initialize locators
    this.heading = page.getByRole('heading', { name: /videos/i });
    this.searchInput = page.getByPlaceholder(/search/i);
    this.addButton = page.getByRole('button', { name: /add|new/i });
    this.uploadButton = page.getByRole('button', { name: /upload/i });
    this.videoGrid = page.locator('[data-testid="video-grid"]').or(
      page.locator('.video-grid, .videos-container')
    );
    this.videoCards = page.locator('[data-testid="video-card"]').or(
      page.locator('.video-card')
    );
  }

  /**
   * Navigate to the videos page
   */
  async goto() {
    await super.goto('/videos');
  }

  /**
   * Search for videos
   */
  async search(query: string) {
    await this.searchInput.fill(query);
    await this.searchInput.press('Enter');
  }

  /**
   * Click the add/new video button
   */
  async clickAdd() {
    await this.addButton.click();
  }

  /**
   * Click the upload button
   */
  async clickUpload() {
    await this.uploadButton.click();
  }

  /**
   * Get a video card by index
   */
  getVideoCard(index: number): Locator {
    return this.videoCards.nth(index);
  }

  /**
   * Click on a video card
   */
  async clickVideoCard(index: number) {
    await this.getVideoCard(index).click();
  }

  /**
   * Verify the videos page is visible
   */
  async expectToBeVisible() {
    await expect(this.heading).toBeVisible();
  }

  /**
   * Verify videos are displayed
   */
  async expectVideosToBeVisible() {
    await expect(this.videoCards.first()).toBeVisible();
  }

  /**
   * Verify the number of visible videos
   */
  async expectVideoCount(count: number) {
    await expect(this.videoCards).toHaveCount(count);
  }

  /**
   * Verify the page has video controls (add, search, etc.)
   */
  async expectControlsToBeVisible() {
    const controls = this.addButton.or(this.uploadButton).or(this.searchInput);
    await expect(controls).toBeVisible();
  }
}
