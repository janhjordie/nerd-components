import { expect, test, type Page } from '@playwright/test';
import {
  blazorClick,
  clickTab,
  INTERACTIVE_TIMEOUT,
  switchCatalogBrand,
  waitForInteractiveCatalog,
} from './catalog-helpers';

async function waitForInteractiveCatalogLegacy(page: Page) {
  await waitForInteractiveCatalog(page);
  await switchCatalogBrand(page, 'dnf');
}

test.describe('Design tokens catalog interactivity', () => {
  test('core controls respond to clicks', async ({ page }) => {
    const consoleErrors: string[] = [];
    page.on('console', (message) => {
      if (message.type() === 'error' && !message.text().includes('404')) {
        consoleErrors.push(message.text());
      }
    });

    await waitForInteractiveCatalogLegacy(page);

    await expect(page.locator('#blazor-error-ui')).toBeHidden();

    const darkSwitch = page.getByRole('switch', { name: /preview dark mode/i });
    const initialState = await darkSwitch.getAttribute('aria-checked');
    await darkSwitch.click();
    await expect(darkSwitch).not.toHaveAttribute('aria-checked', initialState ?? '');

    await page.getByRole('button', { name: 'Export CSS' }).click();
    await clickTab(page, /^skov$/i);
    await expect(page.getByRole('tabpanel')).toContainText('skov components', { timeout: 4_000 });

    await clickTab(page, /^Swatches$/);
    const searchField = page.getByLabel(/search tokens/i);
    await expect(searchField).toBeVisible({ timeout: INTERACTIVE_TIMEOUT });
    await searchField.fill('skov');
    await expect(page.locator('[data-nerd-token="dnf-skov"]')).toBeVisible();

    const skovCard = page.locator('[data-nerd-token="dnf-skov"]');
    await skovCard.getByRole('button', { name: 'Toggle favorite' }).click();
    await expect(skovCard.getByRole('button', { name: 'Toggle favorite' })).toHaveClass(/dnf-highlight/);

    await page.getByRole('switch', { name: /favorites only/i }).click();
    await expect(skovCard).toBeVisible();

    const copyButton = skovCard.getByRole('button', { name: 'Copy', exact: true });
    await blazorClick(copyButton);
    await expect(page.getByRole('tab', { name: 'Swatches' })).toHaveAttribute('aria-selected', 'true');

    expect(consoleErrors, consoleErrors.join('\n')).toEqual([]);
  });

  test('token title opens detail tab without freezing swatch interactions', async ({ page }) => {
    await waitForInteractiveCatalogLegacy(page);
    await clickTab(page, /^Swatches$/);

    await blazorClick(
      page.locator('[data-nerd-token="dnf-skov"] p.mud-typography-subtitle1').first(),
    );
    await expect(page.getByRole('tab', { name: /^skov$/i })).toHaveAttribute('aria-selected', 'true');
    await expect(page.getByRole('tabpanel')).toContainText('skov components', { timeout: 4_000 });
  });
});
