import { expect, Page, test } from '@playwright/test';

export const TITLE_SUFFIX = ' - Compression_Force';

/** Sidebar entries exactly as rendered by Views/Shared/_Layout.cshtml (link text -> href). */
export const SIDEBAR_LINKS: Array<{ text: string; href: string; privilege: string }> = [
  { text: 'Diagnostics', href: '/Home/Diagnostics', privilege: 'Diagnostics' },
  { text: 'Recipe', href: '/Recipe/RecipeParameter', privilege: 'Recipe' },
  { text: 'Batch', href: '/Home/Batch', privilege: 'Batch' },
  { text: 'Auto Mode', href: '/Home/AutoMode', privilege: 'AutoMode' },
  { text: 'Report', href: '/Report/Report', privilege: 'Report' },
  { text: 'Calibration', href: '/Home/Calibration', privilege: 'Calibration' },
  { text: 'Alarm', href: '/Home/Alarm', privilege: 'Alarm' },
  { text: 'Help Menu', href: '/Home/HelpMenu', privilege: 'HelpMenu' },
  { text: 'Auto Tare Mode', href: '/Home/AutoTare', privilege: 'AutoTareMode' },
  { text: 'Access Management', href: '/Home/AccessManagement', privilege: 'AccessManagement' },
  { text: 'Audit Trail', href: '/AuditTrail/AuditTrail', privilege: 'AuditTrail' },
  { text: 'Change Password', href: '/Account/ChangePassword', privilege: 'ChangePassword' },
];

export function credentials() {
  const user = process.env.E2E_USER;
  const password = process.env.E2E_PASSWORD;
  if (!user || !password) {
    throw new Error(
      'E2E_USER and E2E_PASSWORD are not set. Copy e2e/.env.example to e2e/.env and fill them in ' +
        '(an active account that has already changed its first-login password).',
    );
  }
  return { user, password };
}

/** Home page URL pattern: "/" (default route) or "/Home/Welcome". */
export const HOME_URL = /\/(Home\/Welcome)?$/i;

/** Logs in through the real login form and waits for the Home (Welcome) page. */
export async function login(page: Page, user: string, password: string) {
  await page.goto('/Account/Login');
  await page.locator('input[name="Email"]').fill(user);
  await page.locator('input[name="Password"]').fill(password);
  await page.locator('#loginButton').click();
  // the default route is Home/Welcome, so the app redirects to "/"
  await page.waitForURL((url) => /^\/(Home\/Welcome)?$/i.test(url.pathname));
}

/**
 * Opens a screen. When the test account's group lacks the privilege the app redirects to
 * /Home/AccessDenied; the test is then skipped (not failed) with a clear reason.
 */
export async function openScreen(page: Page, url: string, title: string) {
  await page.goto(url);
  if (new URL(page.url()).pathname.toLowerCase() === '/home/accessdenied' && title !== 'Access Denied') {
    test.skip(true, `The test account has no privilege for ${url}`);
  }
  await expect(page).toHaveTitle(title + TITLE_SUFFIX);
}

/** Collects JavaScript errors thrown by the page (not network/404 noise). */
export function trackPageErrors(page: Page) {
  const errors: string[] = [];
  page.on('pageerror', (e) => errors.push(e.message));
  return errors;
}
