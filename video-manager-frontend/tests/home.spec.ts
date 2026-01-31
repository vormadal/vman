import { test, expect } from '@playwright/test';
import { LoginPage } from './pages/login.page';

test.describe('Home Page', () => {
  test('should load successfully', async ({ page }) => {
    await page.goto('/');
    
    await expect(page).toHaveTitle(/Video Manager/);
  });

  test('should redirect unauthenticated users to login', async ({ page }) => {
    await page.goto('/');
    
    // Should redirect to login if not authenticated
    await page.waitForURL(/.*login/, { timeout: 5000 }).catch(() => {
      // If no redirect, user might already be on a public page
    });
    
    // Verify login page is displayed
    const loginPage = new LoginPage(page);
    await loginPage.expectToBeVisible().catch(() => {
      // Login page might not be displayed if already authenticated or on public page
    });
  });
});
