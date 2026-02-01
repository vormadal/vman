import { test as setup, expect } from '@playwright/test';
import { LoginPage } from './pages/login.page';
import path from 'path';

const authFile = path.join(__dirname, '../playwright/.auth/user.json');

// Test user credentials from environment variables
const testUser = {
  email: process.env.TEST_USER_EMAIL || 'test.user@example.com',
  password: process.env.TEST_USER_PASSWORD || 'TestPassword123!',
};

setup('authenticate', async ({ page }) => {
  const loginPage = new LoginPage(page);
  
  // Login with test user (user is seeded in backend on startup)
  await loginPage.goto();
  await loginPage.login(testUser.email, testUser.password);
  
  // Wait for successful login redirect
  await page.waitForURL(/.*\/(videos|dashboard|images)/, { timeout: 10000 });
  
  // Verify we're authenticated
  await expect(page).toHaveURL(/.*\/(videos|dashboard|images)/);
  
  // Save authenticated state
  await page.context().storageState({ path: authFile });
  
  console.log('✅ Authentication state saved to:', authFile);
  console.log(`   Test user: ${testUser.email}`);
});

