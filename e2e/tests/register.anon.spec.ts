import { expect, test } from '@playwright/test';
import { TITLE_SUFFIX } from './helpers';

// These tests never submit a *valid* registration: that would create a real (pending) account.
test.describe('Register page  (/Account/Register)', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/Account/Register');
  });

  test('renders the sign-up form', async ({ page }) => {
    await expect(page).toHaveTitle('Register' + TITLE_SUFFIX);
    await expect(page.getByRole('heading', { name: 'Create Account' })).toBeVisible();
    await expect(page.getByPlaceholder('Enter User Name')).toBeVisible();
    await expect(page.getByPlaceholder('Enter Email')).toBeVisible();
    await expect(page.getByPlaceholder('Enter Password')).toBeVisible();
    await expect(page.getByPlaceholder('Confirm Password')).toBeVisible();
    await expect(page.locator('#signUpButton')).toHaveText(/Sign Up/);
  });

  test('"Already have an account? Login" returns to the login page', async ({ page }) => {
    await page.getByRole('link', { name: /Already have an account/ }).click();
    await expect(page).toHaveURL(/\/Account\/Login/);
  });

  test('empty submit shows every required-field message', async ({ page }) => {
    await page.locator('#signUpButton').click();

    await expect(page.getByText('Username is required')).toBeVisible();
    await expect(page.getByText('Email is required')).toBeVisible();
    await expect(page.getByText('Password is required', { exact: true })).toBeVisible();
    await expect(page.getByText('Confirm Password is required')).toBeVisible();
    await expect(page).toHaveURL(/\/Account\/Register/);
  });

  test('rejects a too-short user name', async ({ page }) => {
    await page.getByPlaceholder('Enter User Name').fill('ab');
    await page.getByPlaceholder('Enter Email').fill('someone@example.com');
    await page.locator('#signUpButton').click();
    await expect(page.getByText('Username must be at least 3 characters')).toBeVisible();
  });

  test('rejects user names with unsupported characters', async ({ page }) => {
    await page.getByPlaceholder('Enter User Name').fill('bad name!');
    await page.locator('#signUpButton').click();
    await expect(
      page.getByText('Username can only use letters, numbers, dot, underscore and dash'),
    ).toBeVisible();
  });

  test('rejects an invalid email address', async ({ page }) => {
    await page.getByPlaceholder('Enter Email').fill('not-an-email');
    await page.locator('#signUpButton').click();
    await expect(page.getByText('Enter a valid email address')).toBeVisible();
  });

  test('rejects a password that breaks the password policy', async ({ page }) => {
    await page.getByPlaceholder('Enter Password').fill('abc');
    await page.locator('#signUpButton').click();
    // Policy (Help Menu): 8-15 chars, upper + lower case, a number and a special character
    await expect(page.locator('[data-valmsg-for="Password"]')).not.toBeEmpty();
    await expect(page).toHaveURL(/\/Account\/Register/);
  });
});
