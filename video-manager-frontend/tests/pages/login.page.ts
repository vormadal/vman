import { Page, Locator, expect } from '@playwright/test';
import { BasePage } from './base.page';

/**
 * Page Object Model for the Login page
 */
export class LoginPage extends BasePage {
  // Locators
  readonly emailInput: Locator;
  readonly passwordInput: Locator;
  readonly signInButton: Locator;
  readonly heading: Locator;
  readonly registerLink: Locator;
  readonly errorMessage: Locator;

  constructor(page: Page) {
    super(page);
    
    // Initialize locators
    this.emailInput = page.getByLabel(/email/i);
    this.passwordInput = page.getByLabel(/password/i);
    this.signInButton = page.getByRole('button', { name: /login|sign in/i });
    this.heading = page.getByRole('heading', { name: /login|sign in/i });
    this.registerLink = page.getByRole('link', { name: /register|sign up/i });
    this.errorMessage = page.getByRole('alert').or(page.getByText(/invalid|error/i));
  }

  /**
   * Navigate to the login page
   */
  async goto() {
    await super.goto('/login');
  }

  /**
   * Perform login with email and password
   */
  async login(email: string, password: string) {
    await this.emailInput.fill(email);
    await this.passwordInput.fill(password);
    await this.signInButton.click();
  }

  /**
   * Click the sign in button
   */
  async clickSignIn() {
    await this.signInButton.click();
  }

  /**
   * Navigate to the register page
   */
  async goToRegister() {
    await this.registerLink.click();
  }

  /**
   * Verify the login page is visible
   */
  async expectToBeVisible() {
    await expect(this.heading).toBeVisible();
    await expect(this.emailInput).toBeVisible();
    await expect(this.passwordInput).toBeVisible();
    await expect(this.signInButton).toBeVisible();
  }

  /**
   * Verify an error message is displayed
   */
  async expectErrorMessage() {
    await expect(this.errorMessage).toBeVisible();
  }

  /**
   * Verify validation error for a specific field
   */
  async expectValidationError(message: string | RegExp) {
    const error = this.page.getByText(message);
    await expect(error).toBeVisible();
  }
}
