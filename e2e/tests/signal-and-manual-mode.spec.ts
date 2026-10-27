import { expect, test } from '@playwright/test';
import { openScreen } from './helpers';

test.describe('Signal Monitoring  (/Home/Signal)', () => {
  test.beforeEach(async ({ page }) => {
    await openScreen(page, '/Home/Signal', 'Signal');
  });

  test('top navigation has Signal Monitoring (active) and Input & Output', async ({ page }) => {
    await expect(page.getByRole('link', { name: 'Signal Monitoring' })).toHaveClass(/active/);
    await expect(page.getByRole('link', { name: 'Input & Output' })).toHaveAttribute('href', '/Home/Diagnostics');
  });

  test('offers Main Compression, Pre Compression and Ejection graphs', async ({ page }) => {
    const tabs = page.locator('.graph-stage-tab');
    await expect(tabs).toHaveText(['Main Compression', 'Pre Compression', 'Ejection']);
    await expect(tabs.first()).toHaveClass(/active/);
  });

  test('switching the stage moves the active tab', async ({ page }) => {
    for (const stage of ['Pre Compression', 'Ejection', 'Main Compression']) {
      const tab = page.locator('.graph-stage-tab', { hasText: stage });
      await tab.click();
      await expect(tab, stage).toHaveClass(/active/);
      await expect(tab, stage).toHaveAttribute('aria-selected', 'true');
    }
  });

  test('"Input & Output" returns to Diagnostics', async ({ page }) => {
    await page.getByRole('link', { name: 'Input & Output' }).click();
    await expect(page).toHaveURL(/\/Home\/Diagnostics/);
  });
});

test.describe('Manual Mode  (/Home/ManualMode)', () => {
  test.beforeEach(async ({ page }) => {
    await openScreen(page, '/Home/ManualMode', 'Manual Mode');
  });

  test('has the Input & Output and Signal Monitoring tabs', async ({ page }) => {
    const tabs = page.locator('#myTab button[role="tab"]');
    await expect(tabs).toHaveText(['Input & Output', 'Signal Monitoring']);
    await expect(page.locator('#home-tab-pane')).toBeVisible();
  });

  test('Input & Output pane lists the I/O sections', async ({ page }) => {
    const pane = page.locator('#home-tab-pane');
    await expect(pane.getByRole('heading', { name: 'Digital Inputs' })).toBeVisible();
    await expect(pane.getByRole('heading', { name: 'Digital Outputs' })).toBeVisible();
    await expect(pane.getByRole('heading', { name: 'Analog Inputs' })).toBeVisible();
  });

  test('Signal Monitoring tab reveals its pane', async ({ page }) => {
    await page.locator('#myTab').getByRole('tab', { name: 'Signal Monitoring' }).click();
    await expect(page.locator('#contact-tab-pane')).toBeVisible();
    await expect(page.locator('#home-tab-pane')).toBeHidden();
  });
});
