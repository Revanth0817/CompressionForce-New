import { test as setup, expect } from '@playwright/test';
import * as fs from 'fs';
import * as path from 'path';
import { STORAGE_STATE } from '../playwright.config';
import { HOME_URL, credentials, login } from './helpers';

setup('log in and save the session', async ({ page }) => {
  const { user, password } = credentials();

  await login(page, user, password);
  await expect(page).toHaveURL(HOME_URL);

  fs.mkdirSync(path.dirname(STORAGE_STATE), { recursive: true });
  await page.context().storageState({ path: STORAGE_STATE });
});
