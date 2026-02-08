---
paths: "**/tests/**/*.ts"
---

# Playwright Testing Rules

## Page Object Model is REQUIRED

Never write selectors or page interactions directly in test files. All tests MUST use Page Objects.

```typescript
// pages/example.page.ts
import { Page, Locator, expect } from '@playwright/test'

export class ExamplePage {
  readonly page: Page
  readonly heading: Locator
  readonly submitButton: Locator

  constructor(page: Page) {
    this.page = page
    this.heading = page.getByRole('heading', { name: /example/i })
    this.submitButton = page.getByRole('button', { name: /submit/i })
  }

  async goto() {
    await this.page.goto('/example')
  }

  async expectToBeVisible() {
    await expect(this.heading).toBeVisible()
  }
}
```

```typescript
// example.spec.ts - use Page Object, never raw selectors
import { test } from '@playwright/test'
import { ExamplePage } from './pages/example.page'

test('should display page', async ({ page }) => {
  const examplePage = new ExamplePage(page)
  await examplePage.goto()
  await examplePage.expectToBeVisible()
})
```

## Rules

- Only write tests for critical user flows (login, video management, collections)
- Avoid excessive coverage and verbose tests
- One Page Object per page/component
- Locators as readonly properties
- Actions as methods, assertions as `expectToX` methods
- Do NOT include Playwright tests in Aspire AppHost
- Run tests independently after starting the app
- Use selectors in the following order: role selectors, text selectors, test id selectors. Avoid at all cost xpath selectors.

## Commands

```bash
npx playwright test auth.spec.ts      # Run specific file
npx playwright test --project=chromium # Run specific browser
```
