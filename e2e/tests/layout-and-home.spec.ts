import { expect, test } from '@playwright/test';
import { SIDEBAR_LINKS, openScreen } from './helpers';

test.describe('Home page  (/Home/Welcome) and shared layout', () => {
  test.beforeEach(async ({ page }) => {
    await openScreen(page, '/Home/Welcome', 'Home');
  });

  test('shows the three company unit cards', async ({ page }) => {
    for (const unit of ['Unit 1', 'Unit 2', 'Unit 3']) {
      await expect(page.getByText(unit, { exact: true })).toBeVisible();
    }
    await expect(page.getByText('Contact number: +91-9145000862')).toBeVisible();
    await expect(page.getByText('Contact number: +91-44-27660534/35').first()).toBeVisible();
    await expect(page.getByText('Vasai E')).toBeVisible();
  });

  test('top bar shows the company and system name', async ({ page }) => {
    await expect(page.locator('.navbar-heading-brand')).toHaveText('Kaar Infotech Private Limited');
    await expect(page.locator('.custom-navbar-brand')).toContainText('SRC-51 Compression Force Control System');
  });

  test('sidebar lists every screen with the right link', async ({ page }) => {
    for (const item of SIDEBAR_LINKS) {
      const link = page.locator(`#sidebarMenu a.sidebar-nav-link[data-privilege="${item.privilege}"]`);
      await expect(link, item.text).toHaveAttribute('href', item.href);
      await expect(link, item.text).toContainText(item.text);
    }
    await expect(page.locator('#sidebarMenu .sidebar-signout')).toContainText('Sign Out');
  });

  test('menu entries without a privilege are greyed out, the rest stay clickable', async ({ page }) => {
    // applyPrivileges() runs after /AdminUser/MyPrivileges answers
    const allowed: string[] = await (await page.request.get('/AdminUser/MyPrivileges')).json();
    await page.waitForLoadState('networkidle');

    for (const item of SIDEBAR_LINKS) {
      const link = page.locator(`#sidebarMenu a.sidebar-nav-link[data-privilege="${item.privilege}"]`);
      if (allowed.includes(item.privilege)) {
        await expect(link, item.text).not.toHaveClass(/disabled/);
      } else {
        await expect(link, item.text).toHaveClass(/disabled/);
        await expect(link, item.text).toHaveAttribute('aria-disabled', 'true');
      }
    }
  });

  test('user menu offers Change Password and Sign Out', async ({ page }) => {
    await page.locator('.user-menu-toggle').click();
    const menu = page.locator('.user-menu-list');
    await expect(menu).toBeVisible();
    await expect(menu).toContainText('Signed in as');
    await expect(menu.getByRole('link', { name: 'Change Password' })).toHaveAttribute('href', '/Account/ChangePassword');
    await expect(menu.getByRole('link', { name: 'Sign Out' })).toHaveAttribute('href', '/Account/Logout');
  });

  test('the PLC status card is present with its fields', async ({ page }) => {
    await expect(page.locator('.card-header', { hasText: /PLC PC Communication (Connected|Disconnected)/ })).toBeVisible();
    for (const id of ['plc-date', 'plc-time', 'plc-login', 'plc-user', 'plc-level', 'plc-ip', 'plc-runtime']) {
      await expect(page.locator(`#${id}`), id).toBeAttached();
    }
    await expect(page.locator('#plc-date')).not.toHaveValue('');
    await expect(page.locator('#plc-time')).not.toHaveValue('');
  });

  test('the live clock shows the current date and time (minute resolution)', async ({ page }) => {
    await expect(page.locator('#plc-time')).toHaveValue(/^\d{1,2}:\d{2}(:\d{2})?\s?(AM|PM)?$/i);
    await expect(page.locator('#plc-date')).toHaveValue(/\d{1,2}\s\w{3}\s\d{4}/);
    await expect(page.locator('#plc-time')).toHaveAttribute('data-live', 'time');
    await expect(page.locator('#plc-date')).toHaveAttribute('data-live', 'date');
  });
});

test.describe('Sidebar navigation', () => {
  for (const item of SIDEBAR_LINKS) {
    test(`"${item.text}" opens ${item.href}`, async ({ page }) => {
      await page.goto('/Home/Welcome');
      const link = page.locator(`#sidebarMenu a.sidebar-nav-link[data-privilege="${item.privilege}"]`);
      await page.waitForLoadState('networkidle');
      test.skip((await link.getAttribute('aria-disabled')) === 'true', 'The test account has no privilege for this screen');

      await link.click();
      await expect(page).toHaveURL(new RegExp(item.href.replace(/\//g, '\\/'), 'i'));
    });
  }
});
