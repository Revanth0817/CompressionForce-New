import { expect, test } from '@playwright/test';
import { openScreen } from './helpers';

// "Fetch Report" / "Print Report" on the Audit Trail download a PDF built by wkhtmltopdf (Windows
// binaries), so only their presence is checked. No account, group or setting is saved or changed.
test.describe('Audit Trail  (/AuditTrail/AuditTrail)', () => {
  test.beforeEach(async ({ page }) => {
    await openScreen(page, '/AuditTrail/AuditTrail', 'Audit Trail');
  });

  test('shows the heading, filters and the three report buttons', async ({ page }) => {
    await expect(page.getByRole('heading', { name: 'Audit Trail' })).toBeVisible();
    await expect(page.locator('#fromDate')).toBeVisible();
    await expect(page.locator('#toDate')).toBeVisible();
    await expect(page.locator('#UserName')).toHaveAttribute('readonly', '');
    await expect(page.locator('#Activity')).toHaveAttribute('readonly', '');
    await expect(page.getByRole('button', { name: 'Fetch Report' })).toBeVisible();
    await expect(page.getByRole('button', { name: 'Print Report' })).toBeVisible();
    await expect(page.getByRole('button', { name: 'Clear Filters' })).toBeVisible();
  });

  test('the user-name filter defaults to "None" and offers the known users', async ({ page }) => {
    await expect(page.locator('#UserName')).toHaveValue('None');
    await page.locator('#UserName').locator('xpath=following-sibling::button').click();
    const menu = page.locator('#UserName').locator('xpath=following-sibling::ul');
    await expect(menu).toBeVisible();
    await expect(menu.getByRole('button', { name: 'None' })).toBeVisible();
  });

  test('"Clear Filters" resets the date and user filters', async ({ page }) => {
    await page.locator('#fromDate').fill('2025-01-01T00:00');
    await page.locator('#toDate').fill('2025-12-31T23:59');
    await page.getByRole('button', { name: 'Clear Filters' }).click();

    await expect(page.locator('#fromDate')).toHaveValue('');
    await expect(page.locator('#toDate')).toHaveValue('');
    await expect(page.locator('#UserName')).toHaveValue('None');
  });

  test('the results table has its four columns', async ({ page }) => {
    await expect(page.locator('table thead th')).toHaveCount(4);
  });
});

test.describe('Access Management  (/Home/AccessManagement)', () => {
  test.beforeEach(async ({ page }) => {
    await openScreen(page, '/Home/AccessManagement', 'Access Management');
  });

  test('shows the Account Management toolbar', async ({ page }) => {
    await expect(page.getByRole('heading', { name: 'Account Management' })).toBeVisible();
    for (const name of [
      'Change Password',
      'Change Access',
      'Create Account',
      'Add Group',
      'Account Deactivate',
      'Account Activate',
      'Reset Password Expiry',
    ]) {
      await expect(page.getByRole('button', { name }).first(), name).toBeVisible();
    }
  });

  test('user table has the expected columns and is populated from the server', async ({ page }) => {
    await expect(page.locator('.table-wrapper thead th')).toHaveText([
      '',
      'User Name',
      'Access',
      'Expires On',
      'Activate / Deactivate',
      'Last Login',
    ]);
    const list = await page.request.get('/AdminUser/List');
    expect(list.ok()).toBeTruthy();
    const users: unknown[] = await list.json();
    expect(users.length).toBeGreaterThan(0); // at least the account running this test
    await expect(page.locator('#userTableBody tr').first()).toBeVisible();
  });

  test('Create Account opens the "User Creation" dialog with its form fields', async ({ page }) => {
    await page.getByRole('button', { name: 'Create Account' }).first().click();
    const modal = page.locator('#exampleModalToggle');
    await expect(modal).toBeVisible();
    await expect(modal.getByText('User Creation')).toBeVisible();
    await expect(page.locator('#createUserForm')).toBeAttached();
    for (const id of ['userName', 'email', 'password', 'confirmPassword']) {
      await expect(modal.locator(`#${id}`), id).toBeAttached();
    }
    await modal.getByRole('button', { name: 'Close' }).click();
    await expect(modal).toBeHidden();
  });

  test('Change Access opens its dialog', async ({ page }) => {
    await page.getByRole('button', { name: 'Change Access' }).first().click();
    const modal = page.locator('#exampleModalToggle2');
    await expect(modal).toBeVisible();
    await expect(modal.getByText('Change Access').first()).toBeVisible();
    await modal.getByRole('button', { name: 'Close' }).click();
    await expect(modal).toBeHidden();
  });

  test('Add Group opens the "Manage Groups" dialog and closes without adding a group', async ({ page }) => {
    await page.getByRole('button', { name: 'Add Group' }).click();
    const modal = page.locator('#addGroupModal');
    await expect(modal).toBeVisible();
    await expect(modal.getByText('Manage Groups')).toBeVisible();
    await expect(page.locator('#groupInput')).toBeVisible();
    await modal.locator('[data-bs-dismiss="modal"]').first().click();
    await expect(modal).toBeHidden();
  });

  test('Security Settings dialog exposes the timeout / lockout / expiry fields', async ({ page }) => {
    await page.getByRole('button', { name: 'Security Settings' }).first().click();
    const modal = page.locator('#exampleModalToggle4');
    await expect(modal).toBeVisible();
    for (const id of ['maxAttempts', 'lockoutMinutes', 'appTimeout', 'passwordExpiry']) {
      await expect(modal.locator(`#${id}`), id).toBeAttached();
    }
    await modal.locator('[data-bs-dismiss="modal"]').first().click();
    await expect(modal).toBeHidden();
  });

  test('Privilege dialog ("Privilege Settings") lists the group privilege check boxes', async ({ page }) => {
    await page.getByRole('button', { name: 'Privilege', exact: true }).click();
    const modal = page.locator('#exampleModalToggle3');
    await expect(modal).toBeVisible();
    await expect(modal.getByText('Privilege Settings')).toBeVisible();
    await expect(modal.locator('#privilegeCheckboxes')).toBeAttached();
    await modal.locator('[data-bs-dismiss="modal"]').first().click();
    await expect(modal).toBeHidden();
  });

  test('the privilege / group APIs behind the page answer', async ({ page }) => {
    for (const url of ['/AdminUser/MyPrivileges', '/AdminUser/Groups', '/AdminUser/GetSecuritySettings']) {
      const res = await page.request.get(url);
      expect(res.ok(), url).toBeTruthy();
    }
  });
});
