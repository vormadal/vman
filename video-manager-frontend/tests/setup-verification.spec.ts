import { test, expect } from '@playwright/test';

test.describe('Playwright Setup Verification', () => {
  test('should load the application', async ({ page }) => {
    await page.goto('/');
    
    // Just verify the page loads without errors
    await expect(page).toBeTruthy();
    await expect(page).toHaveTitle(/Video Manager/);
  });
});
