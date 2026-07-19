import { expect, test } from '@playwright/test';
import { DESIGN_SYSTEM_BRANDS, INTERACTIVE_TIMEOUT } from './catalog-helpers';
import {
  assertReadableContrast,
  DESIGN_SYSTEM_PAGES,
  openDesignSystemPage,
  type DesignSystemBrand,
} from './design-system-helpers';

for (const brand of DESIGN_SYSTEM_BRANDS) {
  test.describe(`Style guard · ${brand.toUpperCase()}`, () => {
    test('typography catalog chrome stays readable', async ({ page }) => {
      await openDesignSystemPage(page, brand, {
        id: 'typography',
        path: '/nerd-typography',
        heading: /responsive typography/i,
      });

      await assertReadableContrast(page.getByRole('switch', { name: /show all roles/i }));
      for (const name of ['Previews', 'Breakpoints', 'Editor', 'Packs']) {
        const tab = page.getByRole('tab', { name });
        const colors = await tab.evaluate((element) => {
          const style = getComputedStyle(element);
          return { color: style.color, backgroundColor: style.backgroundColor };
        });
        expect(colors.color).not.toBe(colors.backgroundColor);
      }
    });

    for (const designPage of DESIGN_SYSTEM_PAGES.filter((entry) => entry.usesBrandSwitcher)) {
      test(`${designPage.id} page interactive text stays readable`, async ({ page }) => {
        await openDesignSystemPage(page, brand, designPage);

        const tabs = page.getByRole('tab');
        for (const tab of await tabs.all()) {
          if (!(await tab.isVisible())) {
            continue;
          }

          const text = (await tab.innerText()).trim();
          if (!text) {
            continue;
          }

          await assertReadableContrast(tab, { minRatio: 2.5 });
        }

        const switches = page.getByRole('switch');
        for (const toggle of await switches.all()) {
          if (!(await toggle.isVisible())) {
            continue;
          }

          await assertReadableContrast(toggle);
        }

        await expect(page.locator('#blazor-error-ui')).toBeHidden({ timeout: INTERACTIVE_TIMEOUT });
      });
    }
  });
}
