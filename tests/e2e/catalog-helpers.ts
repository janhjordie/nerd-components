import { expect, type Locator, type Page } from '@playwright/test';

export const INTERACTIVE_TIMEOUT = 15_000;

export const DESIGN_SYSTEM_BRANDS = ['dnf', 'tnc', 'acme', 'demo'] as const;
export type DesignSystemBrand = (typeof DESIGN_SYSTEM_BRANDS)[number];

export const BRAND_PROBE_TABS: Record<DesignSystemBrand, RegExp> = {
  dnf: /^skov$/i,
  tnc: /^navy$/i,
  acme: /^forest$/i,
  demo: /^violet$/i,
};

export async function waitForInteractiveCatalog(page: Page) {
  await page.goto('/nerd-design-tokens');
  await expect(page.getByRole('heading', { name: 'Design token colors' })).toBeVisible({
    timeout: INTERACTIVE_TIMEOUT,
  });
  await page.waitForFunction(
    () => document.querySelector('[data-nerd-token]') !== null,
    undefined,
    { timeout: INTERACTIVE_TIMEOUT },
  );
}

export async function blazorClick(locator: Locator) {
  await locator.waitFor({ state: 'visible', timeout: INTERACTIVE_TIMEOUT });
  await locator.click();
}

export async function clickTab(page: Page, name: string | RegExp) {
  const tab = page.getByRole('tab', { name });
  await tab.waitFor({ state: 'visible', timeout: INTERACTIVE_TIMEOUT });
  await tab.evaluate((element) => {
    element.scrollIntoView({ inline: 'center', block: 'nearest' });
    (element as HTMLElement).click();
  });
  await expect(tab).toHaveAttribute('aria-selected', 'true', { timeout: INTERACTIVE_TIMEOUT });
}

const BRAND_SEARCH_TERMS: Record<DesignSystemBrand, string> = {
  dnf: 'skov',
  tnc: 'navy',
  acme: 'forest',
  demo: 'violet',
};

async function isCatalogBrandActive(page: Page, brand: string) {
  const combobox = page.getByRole('combobox', { name: /^brand$/i });
  if ((await combobox.count()) > 0) {
    const text = await combobox.innerText();
    return new RegExp(`\\b${brand}\\b`, 'i').test(text);
  }

  const tabPattern = BRAND_PROBE_TABS[brand.toLowerCase() as DesignSystemBrand];
  return tabPattern ? (await page.getByRole('tab', { name: tabPattern }).count()) > 0 : false;
}

export async function switchCatalogBrand(page: Page, brand: string) {
  if (await isCatalogBrandActive(page, brand)) {
    return;
  }

  const brandLabel = brand.toUpperCase();
  const combobox = page.getByRole('combobox', { name: /^brand$/i });
  await expect(combobox).toBeVisible({ timeout: INTERACTIVE_TIMEOUT });

  const adornment = combobox.locator('.mud-input-adornment button').first();
  if (await adornment.count()) {
    await adornment.click();
  } else {
    await combobox.click();
  }

  if ((await page.locator('.mud-popover-open').count()) === 0) {
    await combobox.focus();
    await page.keyboard.press('ArrowDown');
  }

  const popover = page.locator('.mud-popover-open').last();
  await expect(popover).toBeVisible({ timeout: INTERACTIVE_TIMEOUT });

  const option = popover.getByRole('option', { name: new RegExp(`^${brandLabel}(?:\\s|\\(|$)`, 'i') });
  if ((await option.count()) === 0) {
    await popover
      .locator('.mud-list-item')
      .filter({ hasText: new RegExp(`^${brandLabel}(?:\\s|\\()`, 'i') })
      .first()
      .click();
  } else {
    await option.click();
  }

  const tabPattern = BRAND_PROBE_TABS[brand.toLowerCase() as DesignSystemBrand];
  const onColorsCatalog =
    /\/nerd-design-tokens(?:\?|#|$)/.test(page.url()) &&
    !page.url().includes('/nerd-design-token-recipes');
  if (tabPattern && onColorsCatalog) {
    await expect(page.getByRole('tab', { name: tabPattern })).toBeVisible({
      timeout: INTERACTIVE_TIMEOUT,
    });
  }

  await expect(combobox).toContainText(new RegExp(brandLabel, 'i'), { timeout: INTERACTIVE_TIMEOUT });
}

export async function openTokenDatePicker(page: Page, tokenName: string, prefix = 'dnf') {
  await clickTab(page, new RegExp(`^${tokenName}$`, 'i'));

  const tokenClass = `${prefix}-${tokenName}`;
  const panel = page.locator(`[data-nerd-token="${tokenClass}"]`);
  await expect(panel).toBeVisible({ timeout: INTERACTIVE_TIMEOUT });

  const picker = panel.getByTestId(`picker-${tokenName}`);
  await expect(picker).toBeVisible({ timeout: INTERACTIVE_TIMEOUT });

  const adornmentButton = picker.locator('.mud-input-adornment button').first();
  if (await adornmentButton.count()) {
    await blazorClick(adornmentButton);
  } else {
    await blazorClick(picker.locator('.mud-input-slot, input').first());
  }

  const popover = page.locator('.mud-popover-open.mud-picker-popover').first();
  await expect(popover).toBeVisible({ timeout: INTERACTIVE_TIMEOUT });
  await expect(popover).toHaveClass(new RegExp(tokenClass.replace('-', '\\-')));
}
