import { expect, test } from '@playwright/test';
import { openScreen, trackPageErrors } from './helpers';

test.describe('Auto Mode  (/Home/AutoMode)', () => {
  test.beforeEach(async ({ page }) => {
    await openScreen(page, '/Home/AutoMode', 'Auto Mode');
  });

  test('top tabs: Force Graph and Data (active), View Parameters, Fill Cam Guide', async ({ page }) => {
    const tabs = page.locator('#myTab button[role="tab"]');
    await expect(tabs).toHaveText(['Force Graph and Data', 'View Parameters', 'Fill Cam Guide']);
    await expect(tabs.first()).toHaveClass(/active/);
    await expect(page.locator('#forceGraphData')).toBeVisible();
  });

  test('each top tab reveals its own pane', async ({ page }) => {
    await page.locator('#myTab').getByRole('tab', { name: 'View Parameters' }).click();
    await expect(page.locator('#viewParameters')).toBeVisible();
    await expect(page.locator('#forceGraphData')).toBeHidden();

    await page.locator('#myTab').getByRole('tab', { name: 'Fill Cam Guide' }).click();
    await expect(page.locator('#fillCamGuide')).toBeVisible();

    await page.locator('#myTab').getByRole('tab', { name: 'Force Graph and Data' }).click();
    await expect(page.locator('#forceGraphData')).toBeVisible();
  });

  test('side tabs: Control-1, Control-2 (active), Force Graph, Alarm', async ({ page }) => {
    const side = page.locator('button.vertical-nav');
    await expect(side).toHaveText(['Control-1', 'Control-2', 'Force Graph', 'Alarm']);
    await expect(page.locator('button.vertical-nav.active')).toHaveText('Control-2');
  });

  test('side tabs switch between controls, force graph and alarm panes', async ({ page }) => {
    await page.locator('button.vertical-nav', { hasText: 'Control-1' }).click();
    await expect(page.locator('#control1')).toBeVisible();
    await expect(page.locator('#control2')).toBeHidden();

    await page.locator('button.vertical-nav', { hasText: 'Force Graph' }).click();
    await expect(page.locator('#forceGraph')).toBeVisible();

    await page.locator('button.vertical-nav', { hasText: 'Alarm' }).click();
    await expect(page.locator('#alarm')).toBeVisible();
  });

  test('right-hand Turret / S1 Feeder / S2 Feeder control panels are present', async ({ page }) => {
    await expect(page.getByRole('heading', { name: 'Turret Control' })).toBeVisible();
    await expect(page.getByRole('heading', { name: 'S1 Feeder Control' })).toBeVisible();
    await expect(page.getByRole('heading', { name: 'S2 Feeder Control' })).toBeVisible();
  });

  test('loads without JavaScript errors', async ({ page }) => {
    const errors = trackPageErrors(page);
    await page.reload();
    await page.waitForLoadState('networkidle');
    expect(errors).toEqual([]);
  });
});

test.describe('Operation Mode  (/Home/OperationMode)', () => {
  test.beforeEach(async ({ page }) => {
    await openScreen(page, '/Home/OperationMode', 'Operation Mode');
  });

  test('top tabs: Controls and Graphs (active) and Parameters', async ({ page }) => {
    const tabs = page.locator('#myTab button[role="tab"]');
    await expect(tabs).toHaveText(['Controls and Graphs', 'Parameters']);
    await expect(page.locator('#controlsandGraphs')).toBeVisible();

    await tabs.nth(1).click();
    await expect(page.locator('#parameters')).toBeVisible();
    await expect(page.locator('#controlsandGraphs')).toBeHidden();
  });

  test('side tabs: Controls, Force Graph, Alarm switch their panes', async ({ page }) => {
    const side = page.locator('button.vertical-nav');
    await expect(side).toHaveText(['Controls', 'Force Graph', 'Alarm']);

    await side.nth(1).click();
    await expect(page.locator('#OperationforceGraph')).toBeVisible();
    await side.nth(2).click();
    await expect(page.locator('#Operationalarm')).toBeVisible();
    await side.nth(0).click();
    await expect(page.locator('#control2')).toBeVisible();
  });

  test('force-graph stage buttons: Main Compression (active), Pre Compression, Ejection', async ({ page }) => {
    await page.locator('button.vertical-nav', { hasText: 'Force Graph' }).click();
    const stages = page.locator('.force-stage-tab');
    await expect(stages).toHaveText(['Main Compression', 'Pre Compression', 'Ejection']);
    await expect(stages.first()).toHaveClass(/active/);

    await stages.nth(2).click();
    await expect(stages.nth(2)).toHaveClass(/active/);
    await expect(stages.nth(2)).toHaveAttribute('aria-pressed', 'true');
  });

  test('right-hand control panels are present', async ({ page }) => {
    await expect(page.getByRole('heading', { name: 'Turret Control' })).toBeVisible();
    await expect(page.getByRole('heading', { name: 'S1 Feeder Control' })).toBeVisible();
    await expect(page.getByRole('heading', { name: 'S2 Feeder Control' })).toBeVisible();
  });
});
