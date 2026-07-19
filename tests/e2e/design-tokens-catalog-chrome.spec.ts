import { expect, test } from '@playwright/test';
import { INTERACTIVE_TIMEOUT, switchCatalogBrand, waitForInteractiveCatalog } from './catalog-helpers';

test.describe('Design tokens catalog chrome colors', () => {
  test.beforeEach(async ({ page }) => {
    await waitForInteractiveCatalog(page);
    await switchCatalogBrand(page, 'tnc');
  });

  test('active nav tab uses brand chrome, not CTA coral', async ({ page }) => {

    const colorsTab = page.getByRole('link', { name: /^Colors/i });
    await expect(colorsTab).toBeVisible({ timeout: INTERACTIVE_TIMEOUT });
    await expect(colorsTab).toHaveClass(/tnc-brand-chrome/);
    await expect(colorsTab).not.toHaveClass(/tnc-primary-action/);

    const background = await colorsTab.evaluate((el) => getComputedStyle(el).backgroundColor);
    expect(background).toBe('rgb(18, 46, 67)');
  });

  test('export toolbar buttons stay neutral', async ({ page }) => {

    const exportCss = page.getByRole('button', { name: 'Export CSS' });
    await expect(exportCss).toBeVisible({ timeout: INTERACTIVE_TIMEOUT });
    await expect(exportCss).not.toHaveClass(/tnc-primary-action/);
    await expect(exportCss).not.toHaveClass(/tnc-brand-chrome/);
  });

  test('brand bundle export buttons have readable text', async ({ page }) => {

    const bundleJson = page.getByRole('button', { name: /Export brand bundle \(JSON\)/i });
    await expect(bundleJson).toBeVisible({ timeout: INTERACTIVE_TIMEOUT });
    await expect(bundleJson).not.toHaveClass(/tnc-primary-action/);

    const color = await bundleJson.evaluate((el) => getComputedStyle(el).color);
    expect(color).not.toBe('rgb(255, 255, 255)');
    expect(color).not.toBe('rgba(255, 255, 255, 1)');
  });

  test('live token studio chips stay readable on recipes page', async ({ page }) => {
    await page.goto('/nerd-design-token-recipes');
    await switchCatalogBrand(page, 'tnc');
    await expect(page.getByRole('heading', { name: 'Design token recipes' })).toBeVisible({
      timeout: INTERACTIVE_TIMEOUT,
    });
    await page.waitForTimeout(1500);

    const pairingChip = page.locator('.mud-chip').filter({ hasText: / on /i }).first();
    await expect(pairingChip).toBeVisible({ timeout: INTERACTIVE_TIMEOUT });
    const color = await pairingChip.evaluate((el) => getComputedStyle(el).color);
    expect(color).not.toBe('rgb(255, 255, 255)');
  });

  test('live token studio preview uses recipe pairing', async ({ page }) => {
    await page.goto('/nerd-design-token-recipes');
    await switchCatalogBrand(page, 'tnc');
    await expect(page.getByRole('heading', { name: 'Design token recipes' })).toBeVisible({
      timeout: INTERACTIVE_TIMEOUT,
    });
    await page.waitForTimeout(1500);

    const preview = page.locator('.nerd-pairing-surface').first();
    await expect(preview).toBeVisible({ timeout: INTERACTIVE_TIMEOUT });

    const background = await preview.evaluate((el) => getComputedStyle(el).backgroundColor);
    expect(background).toBe('rgb(18, 46, 67)');

    const previewTitle = preview.locator('.nerd-pairing-swatch__title');
    const titleColor = await previewTitle.evaluate((el) => getComputedStyle(el).color);
    expect(titleColor).toBe('rgb(255, 255, 255)');

    const outlined = preview.getByRole('button', { name: 'Outlined' });
    await expect(outlined).toBeVisible();
    const outlinedColor = await outlined.evaluate((el) => getComputedStyle(el).color);
    expect(outlinedColor).toBe('rgb(255, 255, 255)');

    await expect(preview.getByText('chalk on navy', { exact: true })).toBeVisible();
  });
});
