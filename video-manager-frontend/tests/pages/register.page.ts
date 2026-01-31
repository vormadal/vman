import { Page, Locator, expect } from '@playwright/test';
import { BasePage } from './base.page';

/**
 * Page Object Model for the Register page
 */
export class RegisterPage extends BasePage {
  // Locators
  readonly firstNameInput: Locator;
  readonly lastNameInput: Locator;
  readonly emailInput: Locator;
  readonly passwordInput: Locator;
  readonly confirmPasswordInput: Locator;
  readonly signUpButton: Locator;
  readonly heading: Locator;
  readonly loginLink: Locator;

  constructor(page: Page) {
    super(page);
    
    // Initialize locators
    this.firstNameInput = page.getByLabel(/first name/i);
    this.lastNameInput = page.getByLabel(/last name/i);
    this.emailInput = page.getByLabel(/email/i);
    this.passwordInput = page.getByLabel(/^password$/i);
    this.confirmPasswordInput = page.getByLabel(/confirm password|password confirmation/i);
    this.signUpButton = page.getByRole('button', { name: /sign up|register/i });
    this.heading = page.getByRole('heading', { name: /sign up|register/i });
    this.loginLink = page.getByRole('link', { name: /sign in|login/i });
  }

  /**
   * Navigate to the register page
   */
  async goto() {
    await super.goto('/register');
  }

  /**
   * Perform registration
   * @param name - Full name (will be split into first and last name)
   * @param email - Email address
   * @param password - Password
   * @param confirmPassword - Password confirmation (defaults to password if not provided)
   */
  async register(name: string, email: string, password: string, confirmPassword?: string) {
    // Split name into first and last name
    const nameParts = name.trim().split(/\s+/);
    const firstName = nameParts[0] || '';
    const lastName = nameParts.slice(1).join(' ') || nameParts[0] || '';
    
    await this.firstNameInput.fill(firstName);
    await this.lastNameInput.fill(lastName);
    await this.emailInput.fill(email);
    await this.passwordInput.fill(password);
    
    if (confirmPassword !== undefined) {
      await this.confirmPasswordInput.fill(confirmPassword);
    } else {
      await this.confirmPasswordInput.fill(password);
    }
    
    await this.signUpButton.click();
  }

  /**
   * Navigate to the login page
   */
  async goToLogin() {
    await this.loginLink.click();
  }

  /**
   * Verify the register page is visible
   */
  async expectToBeVisible() {
    await expect(this.heading).toBeVisible();
    await expect(this.firstNameInput).toBeVisible();
    await expect(this.lastNameInput).toBeVisible();
    await expect(this.emailInput).toBeVisible();
    await expect(this.passwordInput).toBeVisible();
    await expect(this.signUpButton).toBeVisible();
  }
}
