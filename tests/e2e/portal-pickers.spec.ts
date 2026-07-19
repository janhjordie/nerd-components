import { expect, test } from '@playwright/test';
import {
  INTERACTIVE_TIMEOUT,
  openTokenDatePicker,
  switchCatalogBrand,
  waitForInteractiveCatalog,
} from './catalog-helpers';

test.describe('HR-006 portal-aware pickers', () => {
  test('date picker popover inherits active design token class', async ({ page }) => {
    await waitForInteractiveCatalog(page);
    await switchCatalogBrand(page, 'dnf');
    await openTokenDatePicker(page, 'himmel');

    const popover = page.locator('.mud-popover-open, .mud-picker-popover.mud-popover-open').first();
    await expect(popover).toBeVisible({ timeout: INTERACTIVE_TIMEOUT });
    await expect(popover).toHaveClass(/dnf-himmel/, { timeout: INTERACTIVE_TIMEOUT });
  });
});
