import { test, expect } from '@playwright/test';
import { LoginPage } from './pages/login.page';
import { RegisterPage } from './pages/register.page';

test.describe('Authentication', () => {
  // Test user from environment variables (seeded in backend)
  const testUser = {
    email: process.env.TEST_USER_EMAIL || 'not-set',
    password: process.env.TEST_USER_PASSWORD || 'not-set',
  };
  
  // For registration tests, use unique email
  const timestamp = Date.now();
  const newUser = {
    firstName: 'New',
    lastName: 'User',
    email: `new.user.${timestamp}@example.com`,
    password: 'NewPassword123!',
  };

  test('should display login page', async ({ page }) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.expectToBeVisible();
  });

  test('should show validation errors for empty login form', async ({ page }) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.clickSignIn();
    
    // Check for validation error messages
    await loginPage.expectValidationError(/email is required|invalid email/i);
  });

  test('should navigate between login and register pages', async ({ page }) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.goToRegister();
    
    const registerPage = new RegisterPage(page);
    await registerPage.expectUrl(/.*register/);
    
    // Navigate back to login
    await registerPage.goToLogin();
    await loginPage.expectUrl(/.*login/);
  });

  test('should register new user with auto-login', async ({ page }) => {
    const registerPage = new RegisterPage(page);
    await registerPage.goto();
    await registerPage.expectToBeVisible();

    // Fill registration form with unique user
    await registerPage.register(
      `${newUser.firstName} ${newUser.lastName}`,
      newUser.email,
      newUser.password
    );

    // Should auto-login and redirect to /videos
    await page.waitForURL(/.*\/videos/, { timeout: 10000 });
    
    // Verify we're authenticated by checking the page content
    await expect(page).toHaveURL(/.*\/videos/);
    
    // Optional: Check for user-specific elements (adjust based on your UI)
    // await expect(page.getByText(newUser.firstName)).toBeVisible();
  });

  test('should login with registered credentials', async ({ page }) => {
    // Use the seeded test user for login test
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.login(testUser.email, testUser.password);
    await loginPage.login(testUser.email, testUser.password);

    // Should redirect to /videos
    await page.waitForURL(/.*\/videos/, { timeout: 10000 });
    await expect(page).toHaveURL(/.*\/videos/);
  });

  test('should show error for invalid credentials', async ({ page }) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    
    // Try to login with invalid credentials
    await loginPage.login('invalid@example.com', 'wrongpassword');
    
    // Should stay on login page and show error
    await expect(page).toHaveURL(/.*login/);
    
    // Check for error message in toast (using alert role for Sonner toast)
    const errorMessage = page.getByRole('alert').filter({ hasText: /invalid email or password/i });
    await expect(errorMessage).toBeVisible({ timeout: 5000 });
  });

  test('should prevent duplicate email registration', async ({ page }) => {
    const registerPage = new RegisterPage(page);
    
    // Try to register with the already seeded test user email
    await registerPage.goto();
    await registerPage.register(
      'Duplicate User',
      testUser.email, // Email already exists in DB from seeding
      'DifferentPassword123!'
    );
    
    // Should stay on register page and show error
    await expect(page).toHaveURL(/.*register/);
    
    // Check for error message in toast about duplicate email
    const errorMessage = page.getByRole('alert').filter({ hasText: /email already in use/i });
    await expect(errorMessage).toBeVisible({ timeout: 5000 });
  });

  test('should validate password requirements', async ({ page }) => {
    const registerPage = new RegisterPage(page);
    await registerPage.goto();
    
    // Try to register with weak password
    await registerPage.register(
      `${newUser.firstName} ${newUser.lastName}`,
      `weak.${timestamp}@example.com`,
      '123' // Weak password
    );
    
    // Should show validation error
    await expect(page).toHaveURL(/.*register/);
    
    // Check for password validation message (adjust based on your validation rules)
    const errorMessage = page.getByText(/password.*must.*characters|password.*too.*short/i);
    await expect(errorMessage).toBeVisible({ timeout: 5000 });
  });
});
