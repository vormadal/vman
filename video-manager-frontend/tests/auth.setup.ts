import { test as setup, expect } from '@playwright/test';
import { RegisterPage } from './pages/register.page';
import { LoginPage } from './pages/login.page';
import path from 'path';

const authFile = path.join(__dirname, '../playwright/.auth/user.json');

// Test user credentials
const testUser = {
  firstName: 'Test',
  lastName: 'User',
  email: 'test.user@example.com',
  password: 'TestPassword123!',
};

setup('authenticate', async ({ page }) => {
  const loginPage = new LoginPage(page);
  const registerPage = new RegisterPage(page);
  
  // Try to login first
  await loginPage.goto();
  await loginPage.login(testUser.email, testUser.password);
  
  // Check if login was successful
  const isLoginSuccessful = await page.waitForURL(/.*\/(videos|dashboard|images)/, { timeout: 5000 })
    .then(() => true)
    .catch(() => false);
  
  if (!isLoginSuccessful) {
    // User doesn't exist, register new user
    console.log('Test user not found, creating new user...');
    await registerPage.goto();
    await registerPage.register(
      `${testUser.firstName} ${testUser.lastName}`,
      testUser.email,
      testUser.password
    );
    
    // Wait for redirect after registration
    await page.waitForURL(/.*\/(videos|dashboard|images)/, { timeout: 10000 });
  }
  
  // Verify we're authenticated
  await expect(page).toHaveURL(/.*\/(videos|dashboard|images)/);
  
  // Save authenticated state
  await page.context().storageState({ path: authFile });
  
  console.log('✅ Authentication state saved to:', authFile);
});
