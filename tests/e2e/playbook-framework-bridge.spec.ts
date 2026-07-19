import { test, expect } from '@playwright/test';
import {
  clickTab,
  INTERACTIVE_TIMEOUT,
} from './catalog-helpers';

test.describe('PlayBook framework bridge', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/nerd-playbook');
    await expect(page.getByRole('heading', { name: /mudblazor playbook/i })).toBeVisible({
      timeout: INTERACTIVE_TIMEOUT,
    });
    await expect(page.locator('.mud-tabs').first()).toBeVisible({ timeout: INTERACTIVE_TIMEOUT });
    await clickTab(page, /framework bridge/i);
  });

  test('shows mud preview and fluent token map', async ({ page }) => {
    const panel = page.getByTestId('playbook-framework-bridge-panel');
    await expect(panel).toBeVisible();
    await expect(panel.getByTestId('fluent-map-brand-background')).toBeVisible();
    await expect(panel.getByTestId('radzen-map-primary')).toBeVisible();
    await expect(panel.getByTestId('radzen-preview-panel')).toBeVisible();
    await expect(panel.getByTestId('radzen-live-primary-button')).toBeVisible();
    await expect(panel.getByText(/Color\.Primary/i)).toBeVisible();
  });

  test('tnc brand shows default spacing scale chips', async ({ page }) => {
    await expect(page.getByTestId('spacing-4')).toBeVisible();
    await expect(page.getByTestId('spacing-8')).toBeVisible();
  });
});
