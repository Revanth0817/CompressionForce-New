import { expect, Page, test } from '@playwright/test';
import { openScreen } from './helpers';

// The page hides its view tabs until a batch with force data exists.
async function skipWithoutBatchData(page: Page) {
  await page.waitForLoadState('networkidle');
  const tab = page.getByRole('button', { name: 'Force VS Punch Number' });
  test.skip(!(await tab.isVisible()), 'No batch with force data exists on this system - the view tabs are hidden');
}

// "Print Report" renders a PDF through wkhtmltopdf (Windows binaries) and is deliberately not clicked.
test.describe('Report  (/Report/Report)', () => {
  test.beforeEach(async ({ page }) => {
    await openScreen(page, '/Report/Report', 'Report');
  });

  test('shows the "Offline Force Data" heading, batch selector and print button', async ({ page }) => {
    await expect(page.getByRole('heading', { name: 'Offline Force Data' })).toBeVisible();
    await expect(page.locator('#currentBatch')).toBeAttached();
    await expect(page.locator('#batchMenu')).toBeAttached();
    await expect(page.locator('#btnPrintReport')).toBeVisible();
  });

  test('stage buttons: S1 / S2 Main, Pre and Ejection, S1 Main first', async ({ page }) => {
    const stages = page.locator('#stageTabs button[data-stage]');
    await expect(stages).toHaveText([
      'S1 Main Comp.',
      'S1 Pre Comp.',
      'S1 Ejection',
      'S2 Main Comp.',
      'S2 Pre Comp.',
      'S2 Ejection',
    ]);
    await expect(stages.first()).toHaveClass(/active/);
  });

  test('picking another stage moves the active button', async ({ page }) => {
    const stages = page.locator('#stageTabs button[data-stage]');
    for (const [i, key] of [[3, 's2-main'], [1, 's1-pre'], [5, 's2-eject']] as Array<[number, string]>) {
      await stages.nth(i).click();
      await expect(stages.nth(i)).toHaveClass(/active/);
      await expect(stages.nth(i)).toHaveAttribute('data-stage', key);
    }
  });

  test('view tabs: Force VS Revolutions, Force VS Punch Number and Force Data', async ({ page }) => {
    await skipWithoutBatchData(page);
    await expect(page.locator('#view-rev')).toBeVisible();

    await page.getByRole('button', { name: 'Force VS Punch Number' }).click();
    await expect(page.locator('#view-punch')).toBeVisible();
    await expect(page.locator('#view-rev')).toBeHidden();

    await page.getByRole('button', { name: 'Force Data' }).click();
    await expect(page.locator('#view-data')).toBeVisible();

    await page.getByRole('button', { name: 'Force VS Revolutions' }).click();
    await expect(page.locator('#view-rev')).toBeVisible();
  });

  test('punch selection helpers All / None are available on the punch view', async ({ page }) => {
    await skipWithoutBatchData(page);
    await page.getByRole('button', { name: 'Force VS Punch Number' }).click();
    await expect(page.locator('#punchAll')).toBeAttached();
    await expect(page.locator('#punchNone')).toBeAttached();
    await expect(page.locator('#punchStart')).toBeAttached();
    await expect(page.locator('#punchEnd')).toBeAttached();
  });

  test('batch list endpoint answers JSON', async ({ page }) => {
    const res = await page.request.get('/Report/Batches', { headers: { Accept: 'application/json' } });
    expect(res.ok()).toBeTruthy();
    expect(res.headers()['content-type']).toContain('application/json');
  });
});

// Home/Table and Home/Graph were unfinished copies of the report; the app redirects both to it.
for (const legacy of ['/Home/Table', '/Home/Graph']) {
  test(`${legacy} redirects to the Report page`, async ({ page }) => {
    await page.goto(legacy);
    test.skip(new URL(page.url()).pathname.toLowerCase() === '/home/accessdenied', 'The test account has no Report privilege');
    await expect(page).toHaveURL(/\/Report\/Report/);
    await expect(page).toHaveTitle('Report - Compression_Force');
    await expect(page.getByRole('heading', { name: 'Offline Force Data' })).toBeVisible();
  });
}
