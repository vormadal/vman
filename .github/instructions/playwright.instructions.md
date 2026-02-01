---
applyTo: '**/tests/**'
name: 'Playwright Testing Instructions'
description: 'Guidelines for writing Playwright tests using the Page Object Model pattern.'
---

### Testing

#### Frontend E2E Testing (Playwright)

- **Framework**: Playwright with TypeScript
- **Location**: `video-manager-frontend/tests/`
- **Pattern**: Page Object Model (POM) - **REQUIRED**
- **Browsers**: Chromium, Firefox
- **Configuration**: `playwright.config.ts`

- Only write tests for critical user flows (login, video management, collections) and avoid excessive coverage and verbose tests.

**Running Tests:**

```bash
cd video-manager-frontend

# Run all tests
npm run test:e2e

# Interactive UI mode (recommended during development)
npm run test:e2e:ui

# Debug mode with browser DevTools
npm run test:e2e:debug

# View HTML report
npm run test:e2e:report

# Run specific test file
npx playwright test auth.spec.ts

# Run specific browser
npx playwright test --project=chromium
```

**Test Organization:**

```
tests/
├── pages/                    # Page Object Models (POM)
│   ├── login.page.ts
│   ├── videos.page.ts
│   └── base.page.ts
├── fixtures/                 # Reusable fixtures
│   └── authenticated.ts
├── auth.spec.ts             # Test specs
├── videos.spec.ts
└── auth.setup.ts            # Setup scripts
```

**Page Object Model Pattern (REQUIRED):**

All tests MUST use Page Object Model. Never write selectors or page interactions directly in test files.

```typescript
// pages/login.page.ts - Page Object Model
import { Page, Locator, expect } from '@playwright/test'

export class LoginPage {
  readonly page: Page
  readonly emailInput: Locator
  readonly passwordInput: Locator
  readonly signInButton: Locator
  readonly heading: Locator
  readonly registerLink: Locator

  constructor(page: Page) {
    this.page = page
    this.emailInput = page.getByLabel(/email/i)
    this.passwordInput = page.getByLabel(/password/i)
    this.signInButton = page.getByRole('button', { name: /sign in/i })
    this.heading = page.getByRole('heading', { name: /sign in/i })
    this.registerLink = page.getByRole('link', { name: /register|sign up/i })
  }

  async goto() {
    await this.page.goto('/login')
  }

  async login(email: string, password: string) {
    await this.emailInput.fill(email)
    await this.passwordInput.fill(password)
    await this.signInButton.click()
  }

  async expectToBeVisible() {
    await expect(this.heading).toBeVisible()
    await expect(this.emailInput).toBeVisible()
    await expect(this.passwordInput).toBeVisible()
  }
}
```

```typescript
// auth.spec.ts - Test using Page Object
import { test, expect } from '@playwright/test'
import { LoginPage } from './pages/login.page'

test.describe('Authentication', () => {
  test('should display login page', async ({ page }) => {
    const loginPage = new LoginPage(page)
    await loginPage.goto()
    await loginPage.expectToBeVisible()
  })

  test('should login successfully', async ({ page }) => {
    const loginPage = new LoginPage(page)
    await loginPage.goto()
    await loginPage.login('test@example.com', 'password123')

    // Verify redirect to dashboard
    await expect(page).toHaveURL(/.*\/(videos|dashboard)/)
  })

  test('should navigate to register page', async ({ page }) => {
    const loginPage = new LoginPage(page)
    await loginPage.goto()
    await loginPage.registerLink.click()

    await expect(page).toHaveURL(/.*register/)
  })
})
```

**POM Best Practices:**

- One Page Object per page/component
- Locators as readonly properties
- Actions as methods (login, submit, etc.)
- Assertions as expectToX methods
- Extend BasePage for common functionality
- Use descriptive method names
- Keep page logic in Page Objects, test logic in specs
- Never use page selectors directly in test files

**Playwright in Aspire:**

- **Do NOT** include Playwright tests in main AppHost
- Tests are a separate validation step, not part of app orchestration
- Run tests independently after starting the app
