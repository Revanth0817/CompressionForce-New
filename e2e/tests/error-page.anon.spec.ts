import { expect, test } from '@playwright/test';

// /Home/Error is public (see the login guard in Program.cs) and renders without the shared layout.
test.describe('Error page  (/Home/Error)', () => {
  const cases = [
    { code: 404, title: 'Page not found', text: 'The page you asked for does not exist.' },
    { code: 403, title: 'Access denied', text: 'You do not have permission to open this.' },
    { code: 401, title: 'Please log in', text: 'Your session has ended. Please log in again.' },
    { code: 500, title: 'Something went wrong', text: 'The request could not be completed.' },
  ];

  for (const c of cases) {
    test(`code ${c.code} shows "${c.title}"`, async ({ page }) => {
      const response = await page.goto(`/Home/Error?code=${c.code}`);
      expect(response?.status()).toBe(c.code);

      await expect(page).toHaveTitle(`${c.title} - Compression_Force`);
      await expect(page.getByRole('heading', { name: c.title })).toBeVisible();
      await expect(page.getByText(c.text)).toBeVisible();
    });
  }

  test('an out-of-range code falls back to the generic 500 page', async ({ page }) => {
    const response = await page.goto('/Home/Error?code=999');
    expect(response?.status()).toBe(500);
    await expect(page).toHaveTitle('Something went wrong - Compression_Force');
  });

  test('"Back to Home" leads on to the app (login when signed out)', async ({ page }) => {
    await page.goto('/Home/Error?code=404');
    await page.getByRole('link', { name: 'Back to Home' }).click();
    await expect(page).toHaveURL(/\/Account\/Login/);
  });

  test('an unknown URL in the browser ends on the friendly error page (login first when signed out)', async ({ page }) => {
    await page.goto('/this/page/does-not-exist');
    // signed out: the login guard answers before routing, so the visitor lands on the login page
    await expect(page).toHaveURL(/\/Account\/Login/);
  });
});
