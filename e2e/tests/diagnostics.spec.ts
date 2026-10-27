import { expect, test } from '@playwright/test';
import { openScreen, trackPageErrors } from './helpers';

test.describe('Diagnostics  (/Home/Diagnostics)', () => {
  test.beforeEach(async ({ page }) => {
    await openScreen(page, '/Home/Diagnostics', 'Diagnostics');
  });

  test('has the four diagnostic tabs, Input & Output first', async ({ page }) => {
    const tabs = page.locator('#myTab button[role="tab"]');
    await expect(tabs).toHaveText([
      'Input & Output',
      'Servo Management',
      'Signal Monitoring',
      /Servo Home & Retrieve Setting/,
    ]);
    await expect(tabs.first()).toHaveClass(/active/);
    await expect(page.locator('#home-tab-pane')).toBeVisible();
  });

  test('Input & Output pane shows the digital/analog I/O sections', async ({ page }) => {
    const pane = page.locator('#home-tab-pane');
    await expect(pane.getByRole('heading', { name: 'Digital Inputs' })).toBeVisible();
    await expect(pane.getByRole('heading', { name: 'Digital Outputs' })).toBeVisible();
    await expect(pane.getByRole('heading', { name: 'Analog Inputs' })).toBeVisible();
  });

  test('each tab switches the visible pane', async ({ page }) => {
    const panes: Array<[string, string]> = [
      ['Servo Management', '#profile-tab-pane'],
      ['Signal Monitoring', '#contact-tab-pane'],
      ['Servo Home & Retrieve Setting', '#disabled-tab-pane'],
      ['Input & Output', '#home-tab-pane'],
    ];
    for (const [name, pane] of panes) {
      await page.locator('#myTab').getByRole('tab', { name }).click();
      await expect(page.locator(pane), name).toBeVisible();
      await expect(page.locator(pane), name).toHaveClass(/active/);
    }
  });

  test('Servo Management pane exposes the jog / run / stop controls', async ({ page }) => {
    await page.locator('#myTab').getByRole('tab', { name: 'Servo Management' }).click();
    const pane = page.locator('#profile-tab-pane');
    for (const name of ['Jog Up', 'Jog Down', 'Run', 'Stop']) {
      await expect(pane.getByRole('button', { name }).first(), name).toBeAttached();
    }
  });

  test('loads without JavaScript errors', async ({ page }) => {
    const errors = trackPageErrors(page);
    await page.reload();
    await page.waitForLoadState('networkidle');
    expect(errors).toEqual([]);
  });
});
