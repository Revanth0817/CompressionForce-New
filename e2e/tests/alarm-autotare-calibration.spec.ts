import { expect, test } from '@playwright/test';
import { openScreen, trackPageErrors } from './helpers';

test.describe('Alarm  (/Home/Alarm)', () => {
  test.beforeEach(async ({ page }) => {
    await openScreen(page, '/Home/Alarm', 'Alarm');
  });

  test('shows the summary header, filters, buttons and table columns', async ({ page }) => {
    await expect(page.getByRole('heading', { name: 'Alarm Summary' })).toBeVisible();
    await expect(page.getByText('From Date Time')).toBeVisible();
    await expect(page.getByText('To Date Time')).toBeVisible();
    await expect(page.locator('#fromDate')).toHaveAttribute('type', 'datetime-local');
    await expect(page.locator('#toDate')).toHaveAttribute('type', 'datetime-local');
    await expect(page.locator('#fetchReportBtn')).toHaveText(/Fetch Report/);
    await expect(page.getByRole('button', { name: 'Print Report' })).toBeVisible();
    await expect(page.locator('#totalCount')).toHaveAttribute('readonly', '');
    await expect(page.locator('table thead th')).toHaveText([
      'Alarm Code',
      'Alarm Created Time',
      'Alarm Description',
      'User Name',
    ]);
  });

  test('fetching a very wide date range fills the table / count without script errors', async ({ page }) => {
    const errors = trackPageErrors(page);
    await page.locator('#fromDate').fill('2000-01-01T00:00');
    await page.locator('#toDate').fill('2099-12-31T23:59');

    const response = page.waitForResponse((r) => r.url().includes('/Alarm/GetAllAlarms'));
    await page.locator('#fetchReportBtn').click();
    expect((await response).ok()).toBeTruthy();

    await expect(page.locator('#totalCount')).toHaveValue(/^\d+$/);
    expect(errors).toEqual([]);
  });

  test('a non-zero total means rows are listed; zero shows the empty-state row', async ({ page }) => {
    await page.locator('#fromDate').fill('2000-01-01T00:00');
    await page.locator('#toDate').fill('2099-12-31T23:59');
    const response = page.waitForResponse((r) => r.url().includes('/Alarm/GetAllAlarms'));
    await page.locator('#fetchReportBtn').click();
    await response;

    const total = Number(await page.locator('#totalCount').inputValue());
    if (total === 0) {
      await expect(page.locator('#alarmTableBody td[colspan="4"]')).toBeVisible();
    } else {
      await expect(page.locator('#alarmTableBody tr').first()).toBeVisible();
      await expect(page.locator('#alarmTableBody td[colspan="4"]')).toHaveCount(0);
    }
  });
});

test.describe('Auto Tare  (/Home/AutoTare)', () => {
  test.beforeEach(async ({ page }) => {
    await openScreen(page, '/Home/AutoTare', 'Auto Tare');
  });

  test('shows the "No Load" offset card with a read-only door interlock indicator', async ({ page }) => {
    await expect(page.getByText("Loadcell 'No Load' Offset Setting")).toBeVisible();
    await expect(page.getByRole('button', { name: /Door Interlock/ })).toBeDisabled();
    await expect(page.locator('#tareStatus')).toBeAttached();
    await expect(page.locator('#tareUpdated')).toBeAttached();
  });

  test('values show "--" until the PLC has written a reading', async ({ page }) => {
    const inputs = page.locator('.custom-card-height input[readonly]');
    const count = await inputs.count();
    expect(count).toBeGreaterThan(0);
    for (let i = 0; i < count; i++) {
      await expect(inputs.nth(i)).toHaveValue(/^(--|-?\d+(\.\d+)?)$/);
    }
  });

  test('the status endpoint answers JSON', async ({ page }) => {
    const res = await page.request.get('/Home/AutoTareStatus', { headers: { Accept: 'application/json' } });
    expect(res.ok()).toBeTruthy();
    expect(res.headers()['content-type']).toContain('application/json');
  });
});

test.describe('Calibration  (/Home/Calibration)', () => {
  test.beforeEach(async ({ page }) => {
    await openScreen(page, '/Home/Calibration', 'Calibration');
  });

  test('has Loadcell and Servo calibration tabs, Loadcell first', async ({ page }) => {
    const tabs = page.locator('#myTab button[role="tab"]');
    await expect(tabs).toHaveText(['Loadcell Calibration', 'Servo Calibration']);
    await expect(page.locator('#loadcellCali')).toBeVisible();
    await expect(page.locator('#servoCali')).toBeHidden();
  });

  test('Loadcell tab exposes the sensor selector and calibration fields', async ({ page }) => {
    await expect(page.getByText('Sensor Name:')).toBeVisible();
    await expect(page.locator('#selectedItem')).toHaveAttribute('readonly', '');
    await expect(page.getByText('Calibrated Value')).toBeVisible();
    for (const id of ['calibratedValue', 'factor', 'offset', 'maxValue', 'minValue', 'maxVolt', 'minVolt', 'sensorVoltage']) {
      await expect(page.locator(`#${id}`), id).toBeAttached();
    }
    await expect(page.locator('#calibratedValue')).toHaveValue('0.000');
  });

  test('the sensor dropdown lists the load cells from the database', async ({ page }) => {
    const response = page.waitForResponse((r) => r.url().includes('/Calibration/GetLoadCells'));
    await page.reload();
    expect((await response).ok()).toBeTruthy();
    await page.locator('#selectedItem').locator('xpath=following-sibling::button').click();
    await expect(page.locator('#loadcellDropdown')).toBeVisible();
  });

  test('Servo tab shows its controls', async ({ page }) => {
    await page.locator('#myTab').getByRole('tab', { name: 'Servo Calibration' }).click();
    const pane = page.locator('#servoCali');
    await expect(pane).toBeVisible();
    await expect(page.locator('#loadcellCali')).toBeHidden();
    for (const name of ['Jog Up', 'Jog Down', 'Run', 'Stop']) {
      await expect(pane.getByRole('button', { name }).first(), name).toBeAttached();
    }
  });
});
