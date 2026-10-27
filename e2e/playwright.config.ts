import { defineConfig, devices } from '@playwright/test';
import * as dotenv from 'dotenv';
import * as path from 'path';

dotenv.config({ path: path.resolve(__dirname, '.env') });

export const STORAGE_STATE = path.resolve(__dirname, '.auth/user.json');

/**
 * Three projects:
 *  - setup          logs in once and saves the session (tests/auth.setup.ts)
 *  - authenticated  every page behind the login guard (tests/*.spec.ts), reuses the saved session
 *  - anonymous      pages reachable without a session (tests/*.anon.spec.ts)
 */
export default defineConfig({
  testDir: './tests',
  timeout: 45_000,
  expect: { timeout: 10_000 },
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 1 : 0,
  workers: process.env.CI ? 2 : undefined,
  reporter: [['list'], ['html', { open: 'never' }]],
  use: {
    baseURL: process.env.BASE_URL ?? 'https://localhost:7111',
    ignoreHTTPSErrors: true, // ASP.NET dev certificate
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
    video: 'off',
  },
  projects: [
    {
      name: 'setup',
      testMatch: /auth\.setup\.ts/,
    },
    {
      name: 'authenticated',
      testMatch: /.*\.spec\.ts/,
      testIgnore: [/.*\.anon\.spec\.ts/, /auth\.setup\.ts/],
      dependencies: ['setup'],
      use: { ...devices['Desktop Chrome'], storageState: STORAGE_STATE },
    },
    {
      name: 'anonymous',
      testMatch: /.*\.anon\.spec\.ts/,
      use: { ...devices['Desktop Chrome'] },
    },
  ],
});
