import { expect, test } from '@playwright/test';
import { DESIGN_SYSTEM_BRANDS, INTERACTIVE_TIMEOUT } from './catalog-helpers';
import {
  assertNoBlazorError,
  DESIGN_SYSTEM_PAGES,
  exerciseColorsPageControls,
  exerciseHubPageControls,
  exercisePlaybookPageControls,
  exerciseRecipesPageControls,
  exerciseTypographyPageControls,
  openDesignSystemPage,
  type DesignSystemBrand,
  type DesignSystemPage,
} from './design-system-helpers';
const PAGE_EXERCISES: Record<
  DesignSystemPage['id'],
  (page: import('@playwright/test').Page, brand: DesignSystemBrand) => Promise<void>
> = {
  hub: exerciseHubPageControls,
  colors: exerciseColorsPageControls,
  recipes: exerciseRecipesPageControls,
  typography: exerciseTypographyPageControls,
  playbook: exercisePlaybookPageControls,
};

for (const brand of DESIGN_SYSTEM_BRANDS) {
  test.describe(`Design system · ${brand.toUpperCase()}`, () => {
    test.describe.configure({ mode: 'serial' });

    for (const designPage of DESIGN_SYSTEM_PAGES) {
      test(`${designPage.id} page loads and controls work`, async ({ page }) => {
        test.setTimeout(60_000);

        const consoleErrors: string[] = [];
        page.on('console', (message) => {
          if (message.type() === 'error' &&
              !message.text().includes('404') &&
              !message.text().includes('WebSocket')) {
            consoleErrors.push(message.text());
          }
        });

        await openDesignSystemPage(page, brand, designPage);
        await assertNoBlazorError(page);
        await PAGE_EXERCISES[designPage.id](page, brand);

        expect(consoleErrors, consoleErrors.join('\n')).toEqual([]);
      });
    }

    test('hub navigation reaches every design system page', async ({ page }) => {
      test.setTimeout(60_000);

      await openDesignSystemPage(page, brand, DESIGN_SYSTEM_PAGES[0]);
      await assertNoBlazorError(page);

      const routes = [
        { link: /colors/i, heading: /design token colors/i },
        { link: /recipes/i, heading: /design token recipes/i },
        { link: /brand workbook/i, heading: /brand workbook/i },
        { link: /responsive typography/i, heading: /responsive typography/i },
        { link: /mudblazor playbook/i, heading: /mudblazor playbook/i },
      ];

      for (const route of routes) {
        await page.getByRole('link', { name: route.link }).first().click();
        await expect(page.getByRole('heading', { name: route.heading })).toBeVisible({
          timeout: INTERACTIVE_TIMEOUT,
        });
        await assertNoBlazorError(page);
        await page.goto('/nerd-design-system');
        await expect(page.getByRole('heading', { name: /nerd design system/i })).toBeVisible({
          timeout: INTERACTIVE_TIMEOUT,
        });
      }
    });
  });
}
