import { expect, test } from '@playwright/test';
import { credentials, login } from './helpers';

// Signing out destroys the server-side session, so this file logs in on its own
// instead of using the shared session saved by auth.setup.ts.
test.use({ storageState: { cookies: [], origins: [] } });

test.describe('Sign out', () => {
  test('the user menu "Sign Out" ends the session', async ({ page }) => {
    const { user, password } = credentials();
    await login(page, user, password);

    await page.locator('.user-menu-toggle').click();
    await page.locator('.user-menu-list').getByRole('link', { name: 'Sign Out' }).click();
    await expect(page).toHaveURL(/\/Account\/Login/);

    await page.goto('/Home/Welcome');
    await expect(page).toHaveURL(/\/Account\/Login/);
  });

  test('the sidebar "Sign Out" button is hidden on desktop widths', async ({ page }) => {
    const { user, password } = credentials();
    await login(page, user, password);
    await expect(page.locator('#sidebarMenu .sidebar-signout')).toBeHidden();
  });

  test('on narrow screens the sidebar "Sign Out" button ends the session', async ({ page }) => {
    await page.setViewportSize({ width: 800, height: 900 }); // css shows it below 861px
    const { user, password } = credentials();
    await login(page, user, password);

    if (!(await page.locator('#sidebarMenu').isVisible())) {
      await page.getByRole('button', { name: 'Toggle sidebar' }).click();
    }
    await page.locator('#sidebarMenu .sidebar-signout').click();
    await expect(page).toHaveURL(/\/Account\/Login/);

    await page.goto('/Home/Diagnostics');
    await expect(page).toHaveURL(/\/Account\/Login/);
  });
});
