import { expect, test } from '@playwright/test';
import { INTERACTIVE_TIMEOUT, switchCatalogBrand } from './catalog-helpers';

test.describe('PlayBook brand selector', () => {
  test.beforeEach(async ({ page }) => {
    await page.addInitScript(() => {
      localStorage.removeItem('nerd.themeKit.themeId');
      localStorage.removeItem('nerd.themeKit.isDark');
    });
  });

  test('loads with TNC brand pack after reload and exposes a single Brand selector', async ({ page }) => {
    await page.goto('/nerd-playbook');
    await expect(page.getByRole('heading', { name: /mudblazor playbook/i })).toBeVisible({
      timeout: INTERACTIVE_TIMEOUT,
    });

    const prefixCode = page.getByText(/Prefix/i).locator('code');
    await expect(prefixCode).toHaveText('tnc', { timeout: INTERACTIVE_TIMEOUT });
    await expect(page.locator('[class*="tnc-"]').first()).toBeVisible({ timeout: INTERACTIVE_TIMEOUT });

    const brandSelectors = page.getByRole('combobox', { name: /^brand$/i });
    await expect(brandSelectors).toHaveCount(1);
    await expect(brandSelectors).toContainText(/TNC/i);
    await expect(brandSelectors).not.toContainText(/_selectedBrand/i);

    await expect(page.getByRole('combobox', { name: /^theme$/i })).toHaveCount(0);

    await page.reload();
    await expect(page.getByRole('heading', { name: /mudblazor playbook/i })).toBeVisible({
      timeout: INTERACTIVE_TIMEOUT,
    });
    await expect(prefixCode).toHaveText('tnc', { timeout: INTERACTIVE_TIMEOUT });
    await expect(page.getByRole('combobox', { name: /^brand$/i })).toContainText(/TNC/i);
  });

  test('switches design-token brand to DNF', async ({ page }) => {
    await page.goto('/nerd-playbook');
    await expect(page.getByRole('heading', { name: /mudblazor playbook/i })).toBeVisible({
      timeout: INTERACTIVE_TIMEOUT,
    });

    await switchCatalogBrand(page, 'dnf');

    const prefixCode = page.getByText(/Prefix/i).locator('code');
    await expect(prefixCode).toHaveText('dnf', { timeout: INTERACTIVE_TIMEOUT });
    await expect(page.locator('[class*="dnf-"]').first()).toBeVisible({ timeout: INTERACTIVE_TIMEOUT });
  });

  test('reload resets brand switch back to default TNC', async ({ page }) => {
    await page.goto('/nerd-playbook');
    await expect(page.getByRole('heading', { name: /mudblazor playbook/i })).toBeVisible({
      timeout: INTERACTIVE_TIMEOUT,
    });

    await switchCatalogBrand(page, 'acme');
    const prefixCode = page.getByText(/Prefix/i).locator('code');
    await expect(prefixCode).toHaveText('acme', { timeout: INTERACTIVE_TIMEOUT });

    await page.reload();
    await expect(page.getByRole('heading', { name: /mudblazor playbook/i })).toBeVisible({
      timeout: INTERACTIVE_TIMEOUT,
    });
    await expect(prefixCode).toHaveText('tnc', { timeout: INTERACTIVE_TIMEOUT });
    await expect(page.getByRole('combobox', { name: /^brand$/i })).toContainText(/TNC/i);
  });
});
