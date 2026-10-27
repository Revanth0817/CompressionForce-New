import { expect, test } from '@playwright/test';
import { openScreen } from './helpers';

// None of these tests saves, edits or deletes a recipe: they stop at the screens that precede the
// save/confirm step, so no data is created or removed on the target system.
test.describe('Recipe  (/Recipe/RecipeParameter)', () => {
  test.beforeEach(async ({ page }) => {
    await openScreen(page, '/Recipe/RecipeParameter', 'Recipe');
  });

  test('shows the Parameter / Records tabs and the four action buttons', async ({ page }) => {
    await expect(page.locator('#nav-home-tab')).toHaveText('Parameter');
    await expect(page.locator('#nav-profile-tab')).toHaveText('Records');
    await expect(page.locator('#nav-home')).toBeVisible();

    await expect(page.locator('#btnAdd')).toHaveText('Add Recipe');
    await expect(page.locator('#btnEdit')).toHaveText('Edit Recipe');
    await expect(page.locator('#btnRemove')).toHaveText('Remove Recipe');
    await expect(page.locator('#btnPrint')).toHaveText('Print Recipe');
  });

  test('the parameter form is rendered with its key fields', async ({ page }) => {
    await expect(page.locator('#recipeSection')).toBeVisible();
    for (const id of ['recipeCode', 'productName', 'shape', 'size', 'tabletThickness', 'tabletHardness', 'tabletWeight', 'maxTurretRpm']) {
      await expect(page.locator(`#${id}`), id).toBeAttached();
    }
  });

  test('Records tab loads the recipe history table', async ({ page }) => {
    await page.locator('#nav-profile-tab').click();
    await expect(page.locator('#nav-profile')).toBeVisible();
    await expect(page.locator('#nav-profile thead th')).toHaveText(['Date & Time', 'Recipe Code', 'Action', 'Changed By']);
    // either history rows or the "no history" message - never the loading / error placeholder
    await expect(page.locator('#recipeRecordsBody')).not.toContainText('Loading...');
    await expect(page.locator('#recipeRecordsBody')).not.toContainText('Could not load the history.');
  });

  test('"All recipes" check box reloads the history', async ({ page }) => {
    await page.locator('#nav-profile-tab').click();
    await page.locator('#recordsAllRecipes').check();
    await expect(page.locator('#recordsAllRecipes')).toBeChecked();
    await expect(page.locator('#recipeRecordsBody')).not.toContainText('Loading...');
  });

  test('Add Recipe opens the "Enter the Recipe Code" dialog and Cancel closes it', async ({ page }) => {
    await page.locator('#btnAdd').click();
    const modal = page.locator('#addRecipeModal');
    await expect(modal).toBeVisible();
    await expect(modal.getByText('Enter the Recipe Code')).toBeVisible();
    await expect(page.locator('#recipeName')).toHaveAttribute('placeholder', 'e.g. R-1001');
    await expect(page.locator('#duplicateError')).toBeHidden();

    await modal.getByRole('button', { name: 'Cancel' }).click();
    await expect(modal).toBeHidden();
  });

  test('saving an empty recipe code is refused', async ({ page }) => {
    await page.locator('#btnAdd').click();
    let message = '';
    page.once('dialog', async (d) => {
      message = d.message();
      await d.accept();
    });
    await page.locator('#btnConfirmAdd').click();
    await expect.poll(() => message).toBe('Please enter a Recipe Code.');
    await expect(page).toHaveURL(/\/Recipe\/RecipeParameter/);
  });

  test('a new, unused code continues to the Add Recipe page (nothing saved yet)', async ({ page }) => {
    const code = `E2E-${Date.now()}`;
    await page.locator('#btnAdd').click();
    await page.locator('#recipeName').fill(code);
    await page.locator('#btnConfirmAdd').click();

    await expect(page).toHaveURL(new RegExp(`/Recipe/AddRecipe\\?code=${code}`));
    await expect(page).toHaveTitle('Add Recipe - Compression_Force');
  });
});

test.describe('Add Recipe  (/Recipe/AddRecipe)', () => {
  test('shows the code, the parameter form, Save / Cancel and the limits table', async ({ page }) => {
    const code = `E2E-${Date.now()}`;
    await openScreen(page, `/Recipe/AddRecipe?code=${code}`, 'Add Recipe');

    await expect(page.getByText('Add Recipe', { exact: true }).first()).toBeVisible();
    await expect(page.locator('input[name="code"]')).toHaveValue(code);
    await expect(page.locator('input[name="code"]')).toHaveAttribute('readonly', '');
    await expect(page.getByRole('button', { name: 'Save' })).toBeVisible();
    await expect(page.getByRole('button', { name: 'Cancel' })).toBeVisible();

    const limits = page.locator('table.addRecipeTable');
    await expect(limits.locator('thead th')).toHaveText(['Parameter Name', 'Min. Limit', 'Max. Limit', 'Units']);
    await expect(limits.getByRole('row', { name: /Tablet Thickness\s+1\s+10\s+mm/ })).toBeVisible();
    await expect(limits.getByRole('row', { name: /Turret RPM\s+10\s+35\s+RPM/ })).toBeVisible();
  });

  test('Cancel returns to the Recipe page without saving', async ({ page }) => {
    await openScreen(page, `/Recipe/AddRecipe?code=E2E-${Date.now()}`, 'Add Recipe');
    await page.getByRole('button', { name: 'Cancel' }).click();
    await expect(page).toHaveURL(/\/Recipe\/RecipeParameter/);
  });
});

test.describe('Edit / Remove Recipe pages', () => {
  test('Edit Recipe button without a selected recipe asks to select one, otherwise opens Edit Recipe', async ({ page }) => {
    await openScreen(page, '/Recipe/RecipeParameter', 'Recipe');
    const hasRecipe = ((await page.locator('#currentRecipeCode').inputValue()) ?? '').trim() !== '';

    if (!hasRecipe) {
      let message = '';
      page.once('dialog', async (d) => {
        message = d.message();
        await d.accept();
      });
      await page.locator('#btnEdit').click();
      await expect.poll(() => message).toBe('Select a recipe code first.');
      return;
    }
    await page.locator('#btnEdit').click();
    await expect(page).toHaveURL(/\/Recipe\/EditRecipe\?code=/);
    await expect(page).toHaveTitle('Edit Recipe - Compression_Force');
    await expect(page.getByRole('columnheader', { name: 'Parameter Name' }).first()).toBeVisible();
    await expect(page.getByRole('columnheader', { name: 'Max. Limit' }).first()).toBeVisible();
  });

  test('Remove Recipe page shows the confirmation form for an existing recipe', async ({ page }) => {
    await openScreen(page, '/Recipe/RecipeParameter', 'Recipe');
    const code = ((await page.locator('#currentRecipeCode').inputValue()) ?? '').trim();
    test.skip(!code, 'No recipe exists on this system to open the Remove page for');

    await page.locator('#btnRemove').click();
    await expect(page).toHaveURL(/\/Recipe\/RemoveRecipe\?code=/);
    await expect(page).toHaveTitle('Remove Recipe - Compression_Force');
    await expect(page.locator('#deleteForm')).toBeAttached();
    // intentionally NOT submitting the delete form
  });
});
