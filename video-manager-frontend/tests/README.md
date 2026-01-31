# Playwright E2E Tests

This directory contains end-to-end tests for the Video Manager frontend using Playwright with **Page Object Model (POM)** pattern.

## Setup

1. Install dependencies:
   ```bash
   npm install
   ```

2. Install Playwright browsers:
   ```bash
   npx playwright install
   ```

## Running Tests

### Run all tests
```bash
npm run test:e2e
```

### Run tests in UI mode (interactive)
```bash
npm run test:e2e:ui
```

### Run tests in debug mode
```bash
npm run test:e2e:debug
```

### View test report
```bash
npm run test:e2e:report
```

### Run specific test file
```bash
npx playwright test auth.spec.ts
```

### Run tests in specific browser
```bash
npx playwright test --project=chromium
```

## Test Structure

```
tests/
├── pages/                    # Page Object Models (POM)
│   ├── base.page.ts         # Base page with common functionality
│   ├── login.page.ts        # Login page object
│   ├── register.page.ts     # Register page object
│   └── videos.page.ts       # Videos page object
├── fixtures/                 # Reusable fixtures
│   └── authenticated.ts     # Auth fixture for authenticated tests
├── auth.spec.ts             # Authentication tests
├── home.spec.ts             # Home page tests
├── videos.spec.ts           # Videos page tests (authenticated)
├── auth.setup.ts            # Authentication setup script
└── setup-verification.spec.ts # Basic setup verification
```

## Page Object Model (POM) Pattern

**All tests MUST use Page Object Model.** Never write selectors or page interactions directly in test files.

### Why POM?
- **Maintainability**: Changes to UI only require updates to page objects, not all tests
- **Reusability**: Page objects can be shared across multiple tests
- **Readability**: Tests read like user actions, not technical implementation
- **Type Safety**: TypeScript provides autocomplete and type checking

### Creating a Page Object

```typescript
// pages/login.page.ts
import { Page, Locator, expect } from '@playwright/test';
import { BasePage } from './base.page';

export class LoginPage extends BasePage {
  // Define locators as readonly properties
  readonly emailInput: Locator;
  readonly passwordInput: Locator;
  readonly signInButton: Locator;

  constructor(page: Page) {
    super(page);
    this.emailInput = page.getByLabel(/email/i);
    this.passwordInput = page.getByLabel(/password/i);
    this.signInButton = page.getByRole('button', { name: /sign in/i });
  }

  // Navigation methods
  async goto() {
    await super.goto('/login');
  }

  // Action methods
  async login(email: string, password: string) {
    await this.emailInput.fill(email);
    await this.passwordInput.fill(password);
    await this.signInButton.click();
  }

  // Assertion methods
  async expectToBeVisible() {
    await expect(this.emailInput).toBeVisible();
    await expect(this.passwordInput).toBeVisible();
    await expect(this.signInButton).toBeVisible();
  }
}
```

### Writing Tests with POM

```typescript
// auth.spec.ts
import { test, expect } from '@playwright/test';
import { LoginPage } from './pages/login.page';

test('should login successfully', async ({ page }) => {
  const loginPage = new LoginPage(page);
  
  await loginPage.goto();
  await loginPage.login('test@example.com', 'password123');
  
  // Verify redirect
  await expect(page).toHaveURL(/.*dashboard/);
});
```

### POM Best Practices

1. **One Page Object per page/component**
2. **Locators as readonly properties** - Initialize in constructor
3. **Actions as methods** - `login()`, `submit()`, `search()`
4. **Assertions as expectToX methods** - `expectToBeVisible()`, `expectErrorMessage()`
5. **Extend BasePage** - Inherit common functionality
6. **Use descriptive method names** - Reflect user actions
7. **Keep page logic in Page Objects** - Test logic in spec files only
8. **Never use page selectors in tests** - Always go through page objects

## Authenticated Tests

For tests requiring authentication:

```typescript
import { test, expect } from './fixtures/authenticated';
import { VideosPage } from './pages/videos.page';

test.describe('Videos (Authenticated)', () => {
  test.use({ storageState: 'playwright/.auth/user.json' });

  test('should display videos', async ({ page }) => {
    const videosPage = new VideosPage(page);
    await videosPage.goto();
    await videosPage.expectToBeVisible();
  });
});
```

## Configuration

The test configuration is in `playwright.config.ts`. Key settings:

- **Base URL**: `http://localhost:3000` (configurable via `PLAYWRIGHT_BASE_URL`)
- **Browsers**: Chromium, Firefox, WebKit, Mobile Chrome, Mobile Safari
- **Web Server**: Automatically starts dev server before tests
- **Screenshots**: Captured on failure
- **Traces**: Captured on first retry

## CI/CD

The configuration includes GitHub Actions support (`.github/workflows/playwright.yml`). Tests will:
- Run serially on CI (to avoid resource issues)
- Retry failed tests twice
- Fail if `test.only` is left in code
- Upload test reports as artifacts

## Tips

- Use the Playwright VS Code extension for better DX
- Run tests in UI mode during development (`npm run test:e2e:ui`)
- Use `page.pause()` in page objects to debug tests interactively
- Check the HTML report after test runs for detailed results
- Always create page objects before writing tests
- Update page objects when UI changes, not individual tests
