import { expect, Page, test } from '@playwright/test';
import { openScreen } from './helpers';

// Dialogs are opened and cancelled only: no batch is created or deactivated.
// The page disables "Add Batch" when there is no recipe to add a batch to.
async function hasRecipes(page: Page) {
  await page.waitForLoadState('networkidle');
  const recipes: unknown[] = await (await page.request.get('/Batch/Recipes')).json();
  return recipes.length > 0;
}

async function skipWithoutRecipes(page: Page) {
  test.skip(!(await hasRecipes(page)), 'No recipe exists on this system, so the Add Batch dialog cannot open');
}

test.describe('Batch  (/Home/Batch)', () => {
  test.beforeEach(async ({ page }) => {
    await openScreen(page, '/Home/Batch', 'Batch');
  });

  test('has Parameter and Records tabs, Parameter active', async ({ page }) => {
    await expect(page.locator('#nav-home-tab')).toHaveText('Parameter');
    await expect(page.locator('#nav-profile-tab')).toHaveText('Records');
    await expect(page.locator('#nav-home-tab')).toHaveClass(/active/);
    await expect(page.locator('#nav-home')).toBeVisible();
  });

  test('action buttons: Add Batch is enabled, the unfinished ones are disabled', async ({ page }) => {
    await expect(page.locator('#btnAddBatch')).toBeEnabled();
    await expect(page.getByRole('button', { name: 'Edit Batch' })).toBeDisabled();
    await expect(page.getByRole('button', { name: 'View Batch' })).toBeDisabled();
    await expect(page.getByRole('button', { name: 'Print Batch' })).toBeDisabled();
    await expect(page.getByRole('button', { name: 'Edit Batch' })).toHaveAttribute('title', 'Not available yet');
  });

  test('Recipe Code and Batch Number selectors are present and read-only', async ({ page }) => {
    await expect(page.getByText('Recipe Code', { exact: true }).first()).toBeVisible();
    await expect(page.locator('#recipeSelected')).toBeVisible();
    await expect(page.locator('#recipeSelected')).toHaveAttribute('readonly', '');
    await expect(page.locator('#batchSelected')).toBeVisible();
    await expect(page.locator('#batchSelected')).toHaveAttribute('readonly', '');
  });

  test('Add Batch without any recipe shows a warning instead of the dialog', async ({ page }) => {
    test.skip(await hasRecipes(page), 'Recipes exist on this system');
    await expect(page.locator('#batchMessage')).toContainText('There are no recipes yet');

    await page.locator('#btnAddBatch').click();
    await expect(page.locator('#batchMessage')).toContainText('Create a recipe first');
    await expect(page.locator('#addBatchModal')).toBeHidden();
  });

  test('Add Batch dialog shows its fields and Cancel closes it', async ({ page }) => {
    await skipWithoutRecipes(page);
    await page.locator('#btnAddBatch').click();
    const modal = page.locator('#addBatchModal');
    await expect(modal).toBeVisible();
    await expect(modal.getByText('Add Batch').first()).toBeVisible();
    await expect(modal.getByText('Recipe Code')).toBeVisible();
    await expect(modal.getByText('Batch Code')).toBeVisible();
    await expect(modal.getByText('Batch Quantity')).toBeVisible();
    await expect(page.locator('#addBatchCode')).toHaveAttribute('placeholder', 'e.g. B-1001');
    await expect(page.locator('#addBatchCode')).toHaveAttribute('maxlength', '20');
    await expect(page.locator('#addBatchQty')).toBeVisible();

    await modal.getByRole('button', { name: 'Cancel' }).click();
    await expect(modal).toBeHidden();
  });

  test('saving an empty Add Batch form does not create a batch', async ({ page }) => {
    await skipWithoutRecipes(page);
    await page.locator('#btnAddBatch').click();
    await page.locator('#btnSaveBatch').click();
    // validation keeps the dialog open
    await expect(page.locator('#addBatchModal')).toBeVisible();
    await expect(page).toHaveURL(/\/Home\/Batch/);
  });

  test('Records tab shows the batch history table and count', async ({ page }) => {
    await page.locator('#nav-profile-tab').click();
    await expect(page.locator('#nav-profile')).toBeVisible();
    await expect(page.locator('#nav-profile thead th')).toHaveText([
      'Date & Time',
      'Batch Code',
      'Recipe',
      'Batch Qty',
      'Good Qty',
      'Status',
      'User',
    ]);
    await expect(page.locator('#recordsBody')).toBeAttached();
    await expect(page.locator('#recordsCount')).toBeAttached();
  });

  test('the "only this recipe" filter can be toggled', async ({ page }) => {
    await page.locator('#nav-profile-tab').click();
    const box = page.locator('#onlyThisRecipe');
    const before = await box.isChecked();
    await box.setChecked(!before);
    await expect(box).toBeChecked({ checked: !before });
  });
});
