import { expect, test } from '@playwright/test';
import { openScreen } from './helpers';

// Nothing here submits a valid change, so the test account's real password is never altered.
test.describe('Change Password  (/Account/ChangePassword)', () => {
  test.beforeEach(async ({ page }) => {
    await openScreen(page, '/Account/ChangePassword', 'Change Password');
  });

  test('renders the three password fields and the submit button', async ({ page }) => {
    await expect(page.getByPlaceholder('Current password')).toBeVisible();
    await expect(page.getByPlaceholder('New password')).toBeVisible();
    await expect(page.getByPlaceholder('Confirm password')).toBeVisible();
    await expect(page.locator('#changePasswordButton')).toHaveText('Change Password');
  });

  test('empty submit shows every required-field message', async ({ page }) => {
    await page.locator('#changePasswordButton').click();

    await expect(page.getByText('Old password is required')).toBeVisible();
    await expect(page.getByText('New password is required')).toBeVisible();
    await expect(page.getByText('Confirm password is required')).toBeVisible();
    await expect(page).toHaveURL(/\/Account\/ChangePassword/);
  });

  test('rejects a new password shorter than 8 characters', async ({ page }) => {
    await page.locator('#pwdOld').fill('anything');
    await page.locator('#pwdNew').fill('Ab1!');
    await page.locator('#pwdConf').fill('Ab1!');
    await page.locator('#changePasswordButton').click();

    await expect(page.locator('[data-valmsg-for="NewPassword"]')).not.toBeEmpty();
    await expect(page).toHaveURL(/\/Account\/ChangePassword/);
  });

  test('rejects a confirmation that does not match', async ({ page }) => {
    await page.locator('#pwdOld').fill('anything');
    await page.locator('#pwdNew').fill('Valid#Pass1');
    await page.locator('#pwdConf').fill('Different#Pass1');
    await page.locator('#changePasswordButton').click();

    await expect(page.getByText('New password and confirmation do not match.')).toBeVisible();
  });

  test('a wrong current password is refused without changing anything', async ({ page }) => {
    await page.locator('#pwdOld').fill('definitely-not-my-password');
    await page.locator('#pwdNew').fill('Valid#Pass1');
    await page.locator('#pwdConf').fill('Valid#Pass1');
    await page.locator('#changePasswordButton').click();

    await expect(page).toHaveURL(/\/Account\/ChangePassword/);
    await expect(page.getByText('Old password is incorrect')).toBeVisible();
  });
});
