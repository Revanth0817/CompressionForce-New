import { expect, test } from '@playwright/test';
import { HOME_URL, openScreen } from './helpers';

test.describe('Help Menu  (/Home/HelpMenu)', () => {
  test.beforeEach(async ({ page }) => {
    await openScreen(page, '/Home/HelpMenu', 'Help Menu');
  });

  test('lists every screen with a working link', async ({ page }) => {
    await expect(page.getByRole('heading', { name: 'Help Menu' })).toBeVisible();
    await expect(page.getByText('Screens', { exact: true })).toBeVisible();

    const expected: Record<string, string> = {
      Diagnostics: '/Home/Diagnostics',
      Recipe: '/Recipe/RecipeParameter',
      Batch: '/Home/Batch',
      'Auto Mode': '/Home/AutoMode',
      Report: '/Report/Report',
      Calibration: '/Home/Calibration',
      Alarm: '/Home/Alarm',
      'Auto Tare Mode': '/Home/AutoTare',
      'Access Management': '/Home/AccessManagement',
      'Audit Trail': '/AuditTrail/AuditTrail',
      'Change Password': '/Account/ChangePassword',
    };
    for (const [text, href] of Object.entries(expected)) {
      await expect(page.locator('.help-screens').getByRole('link', { name: text, exact: true }), text).toHaveAttribute('href', href);
    }
  });

  test('states the password policy', async ({ page }) => {
    await expect(page.locator('.help-screens')).toContainText('8-15 characters');
  });

  test('shows the support contact numbers', async ({ page }) => {
    await expect(page.getByText('Support', { exact: true })).toBeVisible();
    await expect(page.locator('a[href="tel:+919145000862"]')).toBeVisible();
    await expect(page.locator('a[href="tel:+914427660534"]')).toBeVisible();
    await expect(page.getByText('Fax: +91-44-42608332')).toBeVisible();
  });

  test('a screen link navigates there', async ({ page }) => {
    await page.locator('.help-screens').getByRole('link', { name: 'Diagnostics', exact: true }).click();
    await expect(page).toHaveURL(/\/Home\/Diagnostics|\/Home\/AccessDenied/);
  });
});

test.describe('Access Denied  (/Home/AccessDenied)', () => {
  test('explains the missing permission and links back to Home', async ({ page }) => {
    await openScreen(page, '/Home/AccessDenied', 'Access Denied');

    await expect(page.getByText('Your group does not have permission to open this screen.')).toBeVisible();
    await expect(page.getByText('ask an administrator to grant the privilege in Access Management')).toBeVisible();

    await page.getByRole('link', { name: 'Back to Home' }).click();
    await expect(page).toHaveURL(HOME_URL);
  });
});
