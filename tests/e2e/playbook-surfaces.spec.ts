import { expect, test, type Locator } from '@playwright/test';
import { INTERACTIVE_TIMEOUT, switchCatalogBrand } from './catalog-helpers';
import { resolveBackgroundColor } from './design-system-helpers';

type Rgb = { r: number; g: number; b: number };

function parseRgb(color: string): Rgb | null {
  const match = color.match(/rgba?\((\d+),\s*(\d+),\s*(\d+)/i);
  if (!match) {
    return null;
  }

  return {
    r: Number(match[1]),
    g: Number(match[2]),
    b: Number(match[3]),
  };
}

/** Detects the ThemeKit nerd-brand mint wash (#F0F7F0) and similar green tints. */
function greenTintScore({ r, g, b }: Rgb) {
  return g - Math.max(r, b);
}

async function assertShellIsNotGreenTinted(locator: Locator, label: string) {
  const background = await resolveBackgroundColor(locator);
  const rgb = parseRgb(background);
  expect(rgb, `${label} background must be parseable, got ${background}`).not.toBeNull();
  expect(
    greenTintScore(rgb!),
    `${label} looks green-tinted (bg=${background})`,
  ).toBeLessThan(8);
}

test.describe('PlayBook shell surfaces', () => {
  test.beforeEach(async ({ page }) => {
    await page.addInitScript(() => {
      localStorage.removeItem('nerd.themeKit.themeId');
      localStorage.removeItem('nerd.themeKit.isDark');
    });
  });

  for (const brand of ['tnc', 'dnf', 'acme', 'demo'] as const) {
    test(`layout shell stays neutral when previewing ${brand.toUpperCase()}`, async ({ page }) => {
      await page.goto('/nerd-playbook');
      await expect(page.getByRole('heading', { name: /mudblazor playbook/i })).toBeVisible({
        timeout: INTERACTIVE_TIMEOUT,
      });

      if (brand !== 'tnc') {
        await switchCatalogBrand(page, brand);
      }

      const appBar = page.locator('.mud-appbar').first();
      const mainContent = page.locator('.mud-main-content').first();

      await assertShellIsNotGreenTinted(appBar, 'Header');
      await assertShellIsNotGreenTinted(mainContent, 'Main layout');
    });
  }
});
