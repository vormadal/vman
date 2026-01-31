# End-to-End Testing Guide

## Overview
This project uses Playwright for end-to-end testing. Tests are located in the `tests/` directory.

## Prerequisites

### 1. Backend Must Be Running
The E2E tests require the backend API to be running. Start it using Aspire:

```powershell
cd VideoManager\VideoManager.AppHost
aspire run
```

The backend should be available at `https://localhost:7213` (or the port specified in `.env.local`).

### 2. Frontend Auto-Starts
The Playwright config is set up to automatically start the Next.js dev server when running tests. No manual start needed!

Alternatively, you can run it manually:
```powershell
cd video-manager-frontend
npm run dev
```

The frontend should be available at `http://localhost:3000`.

## Running Tests

### Run All Tests
```powershell
npm run test:e2e
```

### Run Tests in UI Mode (Recommended for Development)
```powershell
npm run test:e2e:ui
```

### Run Tests in Debug Mode
```powershell
npm run test:e2e:debug
```

### Run Specific Test File
```powershell
npx playwright test tests/auth.spec.ts
```

### Run Tests in Headed Mode (See Browser)
```powershell
npx playwright test --headed
```

### Run Specific Browser
```powershell
npx playwright test --project=chromium
```

### List All Tests
```powershell
npx playwright test --list
```

### View Test Report
```powershell
npm run test:e2e:report
```

## Test Structure

```
tests/
├── auth.spec.ts           # Authentication tests (login, register)
├── auth.setup.ts          # Setup authenticated state for other tests
├── videos.spec.ts         # Video management tests
├── home.spec.ts           # Home page tests
├── pages/                 # Page Object Models
│   ├── base.page.ts
│   ├── login.page.ts
│   ├── register.page.ts
│   └── videos.page.ts
└── fixtures/              # Custom fixtures and helpers
    └── authenticated.ts   # Re-export test/expect (for future extensions)
```

## Authentication Tests

The `auth.spec.ts` file contains comprehensive tests for:

1. **Page Display**: Verifies login/register pages load correctly
2. **Validation**: Tests form validation errors
3. **Navigation**: Tests navigation between login and register
4. **Registration**: Tests successful user registration with auto-login
5. **Login**: Tests login with valid credentials
6. **Error Handling**: Tests invalid credentials and duplicate emails
7. **Password Validation**: Tests password requirements

### Test User Generation
Tests generate unique users using timestamps to avoid conflicts:
```typescript
const timestamp = Date.now();
const testUser = {
  firstName: 'Test',
  lastName: 'User',
  email: `test.user.${timestamp}@example.com`,
  password: 'TestPassword123!',
};
```

## Writing New Tests

### Using Page Objects (REQUIRED)
Page objects encapsulate page interactions and locators:

```typescript
import { test, expect } from '@playwright/test';
import { LoginPage } from './pages/login.page';

test('my test', async ({ page }) => {
  const loginPage = new LoginPage(page);
  await loginPage.goto();
  await loginPage.login('user@example.com', 'password');
  await expect(page).toHaveURL(/.*\/videos/);
});
```

### Using Authenticated Context
For tests that require authentication, use `test.use()`:

```typescript
import { test, expect } from '@playwright/test';

test.describe('Authenticated Tests', () => {
  // Load authenticated state for this test suite
  test.use({ storageState: 'playwright/.auth/user.json' });

  test('authenticated test', async ({ page }) => {
    // User is already logged in
    await page.goto('/videos');
    // ... test authenticated features
  });
});
```

**Note:** Run `auth.setup.ts` first to create the authenticated state file.

## Browser Configuration

Tests run on multiple browsers by default:
- **Chromium** (Desktop Chrome)
- **Firefox**
- **WebKit** (Safari)
- **Mobile Chrome** (Pixel 5)
- **Mobile Safari** (iPhone 12)

**Total:** 8 tests × 5 browsers = **40 test runs**

To run on specific browser:
```powershell
npx playwright test --project=chromium
```

## Environment Variables

Create a `.env.test` file if you need different settings for testing:

```env
NEXT_PUBLIC_API_URL=https://localhost:7213
PLAYWRIGHT_BASE_URL=http://localhost:3000
```

## Debugging Tips

### 1. Use UI Mode (Recommended)
```powershell
npm run test:e2e:ui
```
This opens an interactive UI where you can:
- Step through tests
- See screenshots and traces
- Watch tests run in real-time
- Time-travel debugging

### 2. Add Debug Points
```typescript
await page.pause(); // Pauses test execution
```

### 3. Enable Verbose Logging
```powershell
npx playwright test --debug
```

### 4. Screenshots on Failure
Screenshots are automatically captured on failure and saved to `test-results/`.

### 5. View Traces
Traces are captured on first retry:
```powershell
npx playwright show-trace test-results/*/trace.zip
```

## Common Issues

### Backend Not Running
**Error**: `Failed to connect to backend`
**Solution**: Make sure Aspire is running and the backend is accessible at the configured URL.

```powershell
cd VideoManager\VideoManager.AppHost
aspire run
```

### Port Already in Use
**Error**: `Port 3000 is already in use`
**Solution**: Stop the existing Next.js server or change the port in `playwright.config.ts`.

### Database Not Migrated
**Error**: Authentication tests fail with database errors
**Solution**: Ensure database migrations are applied:
```powershell
cd VideoManager\VManBackend
dotnet ef database update
```

### Fixture Already Registered Error
**Error**: `Fixture "storageState" has already been registered`
**Solution**: ✅ **FIXED** - Don't redefine built-in Playwright fixtures. Use `test.use({ storageState: 'path' })` instead.

### Timeout Issues
**Error**: Tests timeout waiting for navigation
**Solution**: 
- Increase timeout in test: `{ timeout: 30000 }`
- Check if backend/frontend are responding slowly
- Check network tab in UI mode
- Verify backend API is running

## CI/CD Integration

Tests are configured to run in CI with:
- Automatic retries (2 retries)
- Sequential execution (non-parallel)
- HTML reporter for artifacts

To simulate CI environment locally:
```powershell
$env:CI = "true"
npm run test:e2e
```

## Best Practices

1. **Use Page Objects**: Keep locators and page logic in page objects (REQUIRED)
2. **Unique Test Data**: Generate unique emails/usernames to avoid conflicts
3. **Explicit Waits**: Use `waitForURL`, `waitForSelector` instead of arbitrary timeouts
4. **Clean Up**: Tests should be independent and not rely on execution order
5. **Descriptive Names**: Use clear test descriptions that explain what's being tested
6. **Screenshot Assertions**: Use visual assertions when appropriate
7. **Test User Flow**: Test complete user journeys, not just individual actions
8. **Don't Redefine Fixtures**: Use built-in Playwright fixtures with `test.use()`

## References

- [Playwright Documentation](https://playwright.dev/)
- [Best Practices](https://playwright.dev/docs/best-practices)
- [Page Object Model](https://playwright.dev/docs/pom)
- [Test Fixtures](https://playwright.dev/docs/test-fixtures)

