import { expect, test } from '@playwright/test';
import { TITLE_SUFFIX } from './helpers';

test.describe('Login page  (/Account/Login)', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/Account/Login');
  });

  test('renders the branding, welcome panel and login form', async ({ page }) => {
    await expect(page).toHaveTitle('Login' + TITLE_SUFFIX);
    await expect(page.getByText('Kaar Infotech Private Limited')).toBeVisible();
    await expect(page.getByText('SRC-51 Compression Force Control System')).toBeVisible();
    await expect(page.getByRole('heading', { name: 'Welcome back!' })).toBeVisible();

    await expect(page.getByPlaceholder('Enter Username/Email')).toBeVisible();
    await expect(page.getByPlaceholder('Enter Password')).toBeVisible();
    await expect(page.getByLabel('Remember Me')).not.toBeChecked();
    await expect(page.getByRole('button', { name: 'Forgot Password?' })).toBeVisible();
    await expect(page.locator('#loginButton')).toHaveText(/Login/);
  });

  test('password field is masked by default and the eye button toggles it', async ({ page }) => {
    const pwd = page.locator('input[name="Password"]');
    const toggle = page.locator('#togglePassword');

    await expect(pwd).toHaveAttribute('type', 'password');
    await expect(toggle).toHaveAttribute('aria-pressed', 'false');

    await toggle.click();
    await expect(pwd).toHaveAttribute('type', 'text');
    await expect(toggle).toHaveAttribute('aria-pressed', 'true');
    await expect(toggle).toHaveAttribute('aria-label', 'Hide password');

    await toggle.click();
    await expect(pwd).toHaveAttribute('type', 'password');
    await expect(toggle).toHaveAttribute('aria-label', 'Show password');
  });

  test('"Forgot Password?" shows and hides the administrator note', async ({ page }) => {
    const note = page.locator('#forgotPasswordNote');
    const link = page.locator('#forgotPassword');

    await expect(note).toBeHidden();
    await link.click();
    await expect(note).toBeVisible();
    await expect(note).toContainText('ask an administrator to reset your password');
    await expect(link).toHaveAttribute('aria-expanded', 'true');

    await link.click();
    await expect(note).toBeHidden();
  });

  test('submitting an empty form shows required-field messages and stays on the page', async ({ page }) => {
    await page.locator('#loginButton').click();

    await expect(page.getByText('Username or Email is required')).toBeVisible();
    await expect(page.getByText('Password is required')).toBeVisible();
    await expect(page).toHaveURL(/\/Account\/Login/);
  });

  test('unknown credentials are rejected with a generic message', async ({ page }) => {
    // random name: never touches a real account's failed-attempt / lockout counter
    const ghost = `no-such-user-${Date.now()}`;
    await page.locator('input[name="Email"]').fill(ghost);
    await page.locator('input[name="Password"]').fill('Wrong#Pass1');
    await page.locator('#loginButton').click();

    await expect(page.getByText('Invalid username/email or password')).toBeVisible();
    await expect(page).toHaveURL(/\/Account\/Login/);
    // the entered user name is kept, the password is not echoed back
    await expect(page.locator('input[name="Email"]')).toHaveValue(ghost);
    await expect(page.locator('input[name="Password"]')).toHaveValue('');
  });

  test('"Sign Up" link opens the Register page', async ({ page }) => {
    await page.getByRole('link', { name: /Sign Up/ }).click();
    await expect(page).toHaveURL(/\/Account\/Register/);
    await expect(page).toHaveTitle('Register' + TITLE_SUFFIX);
  });
});

test.describe('Login guard', () => {
  const protectedPages = [
    '/Home/Welcome',
    '/Home/Diagnostics',
    '/Home/Batch',
    '/Home/AutoMode',
    '/Home/Calibration',
    '/Home/Alarm',
    '/Home/AutoTare',
    '/Home/AccessManagement',
    '/Recipe/RecipeParameter',
    '/Report/Report',
    '/AuditTrail/AuditTrail',
  ];

  for (const url of protectedPages) {
    test(`${url} redirects an anonymous visitor to the login page`, async ({ page }) => {
      await page.goto(url);
      await expect(page).toHaveURL(/\/Account\/Login/);
      await expect(page.locator('#loginButton')).toBeVisible();
    });
  }

  test('API endpoints answer 401 without a session', async ({ request }) => {
    const res = await request.get('/AdminUser/MyPrivileges');
    expect(res.status()).toBe(401);
  });
});
