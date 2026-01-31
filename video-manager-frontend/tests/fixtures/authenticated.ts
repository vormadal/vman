import { test, expect } from '@playwright/test';

// Re-export test and expect from Playwright
// This file exists for future extension if we need authenticated fixtures
// For now, tests that need authentication can use auth.setup.ts
export { test, expect };
