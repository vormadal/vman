import { test, expect } from '@playwright/test';
import { LoginPage } from './pages/login.page';
import { RegisterPage } from './pages/register.page';

test.describe('Authentication', () => {
  // Generate unique test user email for each test run
  const timestamp = Date.now();
  const testUser = {
    firstName: 'Test',
    lastName: 'User',
    email: `test.user.${timestamp}@example.com`,
    password: 'TestPassword123!',
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

    // Fill registration form
    await registerPage.register(
      `${testUser.firstName} ${testUser.lastName}`,
      testUser.email,
      testUser.password
    );

    // Should auto-login and redirect to /videos
    await page.waitForURL(/.*\/videos/, { timeout: 10000 });
    
    // Verify we're authenticated by checking the page content
    await expect(page).toHaveURL(/.*\/videos/);
    
    // Optional: Check for user-specific elements (adjust based on your UI)
    // await expect(page.getByText(testUser.firstName)).toBeVisible();
  });

  test('should login with registered credentials', async ({ page }) => {
    // First register a user
    const registerPage = new RegisterPage(page);
    await registerPage.goto();
    await registerPage.register(
      `${testUser.firstName} ${testUser.lastName}`,
      testUser.email,
      testUser.password
    );
    
    // Wait for auto-login redirect
    await page.waitForURL(/.*\/videos/, { timeout: 10000 });

    // Logout by clearing storage (simulating logout)
    await page.context().clearCookies();
    await page.evaluate(() => localStorage.clear());
    
    // Now test login
    const loginPage = new LoginPage(page);
    await loginPage.goto();
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
    
    // Check for error message (adjust selector based on your error display)
    const errorMessage = page.getByText(/invalid credentials|email or password/i);
    await expect(errorMessage).toBeVisible({ timeout: 5000 });
  });

  test('should prevent duplicate email registration', async ({ page }) => {
    const registerPage = new RegisterPage(page);
    
    // Register user first time
    await registerPage.goto();
    await registerPage.register(
      `${testUser.firstName} ${testUser.lastName}`,
      testUser.email,
      testUser.password
    );
    await page.waitForURL(/.*\/videos/, { timeout: 10000 });
    
    // Logout
    await page.context().clearCookies();
    await page.evaluate(() => localStorage.clear());
    
    // Try to register again with same email
    await registerPage.goto();
    await registerPage.register(
      'Another User',
      testUser.email, // Same email
      'DifferentPassword123!'
    );
    
    // Should stay on register page and show error
    await expect(page).toHaveURL(/.*register/);
    
    // Check for error message about duplicate email
    const errorMessage = page.getByText(/email.*already.*exists|email.*taken/i);
    await expect(errorMessage).toBeVisible({ timeout: 5000 });
  });

  test('should validate password requirements', async ({ page }) => {
    const registerPage = new RegisterPage(page);
    await registerPage.goto();
    
    // Try to register with weak password
    await registerPage.register(
      `${testUser.firstName} ${testUser.lastName}`,
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
