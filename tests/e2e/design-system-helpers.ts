import { expect, type Locator, type Page } from '@playwright/test';
import {
  BRAND_PROBE_TABS,
  clickTab,
  INTERACTIVE_TIMEOUT,
  switchCatalogBrand,
  waitForInteractiveCatalog,
  type DesignSystemBrand,
} from './catalog-helpers';

export type { DesignSystemBrand } from './catalog-helpers';
export { DESIGN_SYSTEM_BRANDS } from './catalog-helpers';

export type DesignSystemPage = {
  id: string;
  path: string;
  heading: RegExp;
  usesBrandSwitcher?: boolean;
};

export const DESIGN_SYSTEM_PAGES: DesignSystemPage[] = [
  {
    id: 'hub',
    path: '/nerd-design-system',
    heading: /nerd design system/i,
  },
  {
    id: 'colors',
    path: '/nerd-design-tokens',
    heading: /design token colors/i,
    usesBrandSwitcher: true,
  },
  {
    id: 'recipes',
    path: '/nerd-design-token-recipes',
    heading: /design token recipes/i,
    usesBrandSwitcher: true,
  },
  {
    id: 'typography',
    path: '/nerd-typography',
    heading: /responsive typography/i,
  },
  {
    id: 'playbook',
    path: '/nerd-playbook',
    heading: /mudblazor playbook/i,
    usesBrandSwitcher: true,
  },
];

export const BRAND_STUDIO_PAIRING: Record<DesignSystemBrand, RegExp> = {
  dnf: /skov on kridt-lys/i,
  tnc: /chalk on navy/i,
  acme: /ink on cloud/i,
  demo: /paper on slate/i,
};

const BRAND_SEARCH_TERMS: Record<DesignSystemBrand, string> = {
  dnf: 'skov',
  tnc: 'navy',
  acme: 'forest',
  demo: 'violet',
};

export const BRAND_TOKEN_PREFIX: Record<DesignSystemBrand, string> = {
  dnf: 'dnf',
  tnc: 'tnc',
  acme: 'acme',
  demo: 'demo',
};

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

function relativeLuminance({ r, g, b }: Rgb) {
  const channel = (value: number) => {
    const normalized = value / 255;
    return normalized <= 0.03928
      ? normalized / 12.92
      : ((normalized + 0.055) / 1.055) ** 2.4;
  };

  return 0.2126 * channel(r) + 0.7152 * channel(g) + 0.0722 * channel(b);
}

export function contrastRatio(foreground: string, background: string) {
  const fg = parseRgb(foreground);
  const bg = parseRgb(background);
  if (!fg || !bg) {
    return 0;
  }

  const lighter = Math.max(relativeLuminance(fg), relativeLuminance(bg));
  const darker = Math.min(relativeLuminance(fg), relativeLuminance(bg));
  return (lighter + 0.05) / (darker + 0.05);
}

export async function resolveBackgroundColor(locator: Locator) {
  return locator.evaluate((element) => {
    let node: HTMLElement | null = element;
    while (node) {
      const background = getComputedStyle(node).backgroundColor;
      if (background && background !== 'rgba(0, 0, 0, 0)' && background !== 'transparent') {
        return background;
      }
      node = node.parentElement;
    }

    return getComputedStyle(document.body).backgroundColor;
  });
}

export async function assertReadableContrast(
  locator: Locator,
  options?: { minRatio?: number; background?: Locator },
) {
  const minRatio = options?.minRatio ?? 4.5;
  await expect(locator).toBeVisible({ timeout: INTERACTIVE_TIMEOUT });

  const foreground = await locator.evaluate((el) => getComputedStyle(el).color);
  const background = options?.background
    ? await options.background.evaluate((el) => getComputedStyle(el).backgroundColor)
    : await resolveBackgroundColor(locator);
  const ratio = contrastRatio(foreground, background);

  expect(
    ratio,
    `Expected contrast >= ${minRatio}, got ${ratio.toFixed(2)} (fg=${foreground}, bg=${background})`,
  ).toBeGreaterThanOrEqual(minRatio);
}

export async function activateBrand(page: Page, brand: DesignSystemBrand) {
  await waitForInteractiveCatalog(page);
  await switchCatalogBrand(page, brand);
  await expect(page.getByRole('combobox', { name: /^brand$/i })).toContainText(
    new RegExp(brand, 'i'),
    { timeout: INTERACTIVE_TIMEOUT },
  );
}

export async function openDesignSystemPage(page: Page, brand: DesignSystemBrand, target: DesignSystemPage) {
  if (target.usesBrandSwitcher) {
    await page.goto(target.path);
    await expect(page.getByRole('heading', { name: target.heading })).toBeVisible({
      timeout: INTERACTIVE_TIMEOUT,
    });
    await switchCatalogBrand(page, brand);
    return;
  }

  await activateBrand(page, brand);
  await page.goto(target.path);
  await expect(page.getByRole('heading', { name: target.heading })).toBeVisible({
    timeout: INTERACTIVE_TIMEOUT,
  });
}

export async function assertNoBlazorError(page: Page) {
  await expect(page.locator('#blazor-error-ui')).toBeHidden();
}

export async function exerciseColorsPageControls(page: Page, brand: DesignSystemBrand) {
  const prefix = BRAND_TOKEN_PREFIX[brand];
  const probeTab = BRAND_PROBE_TABS[brand];

  await expect(page.getByRole('switch', { name: /preview dark mode/i })).toBeVisible();
  await expect(page.getByRole('button', { name: 'Export CSS' })).toBeVisible();
  await clickTab(page, /^Swatches$/);

  const searchField = page.getByLabel(/search tokens/i);
  await searchField.fill(BRAND_SEARCH_TERMS[brand]);
  await expect(page.locator(`[data-nerd-token^="${prefix}-"]`).first()).toBeVisible({
    timeout: INTERACTIVE_TIMEOUT,
  });

  await clickTab(page, probeTab);
  await expect(page.getByRole('tabpanel')).toBeVisible({ timeout: INTERACTIVE_TIMEOUT });
}

export async function exerciseRecipesPageControls(page: Page, brand: DesignSystemBrand) {
  const pairingPattern = BRAND_STUDIO_PAIRING[brand];
  const studio = page.locator('.mud-paper').filter({
    has: page.getByRole('heading', { name: /live token studio/i }),
  });
  const preview = studio.locator('.nerd-pairing-surface').first();

  await expect(studio.getByRole('heading', { name: /live token studio/i })).toBeVisible();
  await expect(studio.getByRole('combobox', { name: /^edit token$/i })).toBeVisible();
  await expect(studio.getByRole('combobox', { name: /^surface$/i })).toBeVisible();
  await expect(studio.getByRole('combobox', { name: /^content$/i })).toBeVisible();
  await expect(studio.getByRole('combobox', { name: /^action$/i })).toBeVisible();

  await expect(preview).toBeVisible({ timeout: INTERACTIVE_TIMEOUT });
  await expect(preview.getByText(pairingPattern)).toBeVisible();

  const title = preview.locator('.nerd-pairing-swatch__title').first();
  await assertReadableContrast(title, { background: preview, minRatio: 4.5 });

  const outlined = preview.getByRole('button', { name: 'Outlined' });
  await expect(outlined).toBeVisible();
  await expect(outlined).toBeEnabled();

  await expect(page.getByText(/WCAG .* AA/i).first()).toBeVisible();
  await expect(page.getByRole('heading', { name: /recipe composer/i })).toBeVisible();
}

export async function exerciseTypographyPageControls(page: Page) {
  await expect(page.getByRole('switch', { name: /show all roles/i })).toBeVisible({
    timeout: INTERACTIVE_TIMEOUT,
  });
  await expect(page.getByLabel(/^clientid$/i)).toBeVisible();
  await expect(page.getByRole('button', { name: /preview scale/i })).toBeVisible();
  await expect(page.getByRole('button', { name: /export tokens studio/i })).toBeVisible();
  await expect(page.locator('table').first()).toBeVisible({ timeout: INTERACTIVE_TIMEOUT });
}

export async function exercisePlaybookPageControls(page: Page, brand: DesignSystemBrand) {
  const prefix = BRAND_TOKEN_PREFIX[brand];

  await expect(page.getByLabel(/search components/i)).toBeVisible();
  await expect(page.getByRole('combobox', { name: /^brand$/i })).toBeVisible();
  await expect(page.getByRole('combobox', { name: /^brand$/i })).toContainText(new RegExp(brand, 'i'));
  await expect(page.getByRole('combobox', { name: /^theme$/i })).toHaveCount(0);
  await expect(page.getByRole('combobox', { name: /design token/i })).toBeVisible();
  await expect(page.getByRole('combobox', { name: /category/i })).toBeVisible();
  await expect(page.getByText(/token combinations/i)).toBeVisible();
  await expect(page.locator(`code`, { hasText: prefix })).toBeVisible({ timeout: INTERACTIVE_TIMEOUT });
  await expect(page.locator(`[class*="${prefix}-"]`).first()).toBeVisible({
    timeout: INTERACTIVE_TIMEOUT,
  });
}

export async function exerciseHubPageControls(page: Page, brand: DesignSystemBrand) {
  await expect(page.getByRole('link', { name: /colors/i }).first()).toBeVisible();
  await expect(page.getByRole('link', { name: /recipes/i }).first()).toBeVisible();
  await expect(page.getByRole('link', { name: /brand workbook/i })).toBeVisible();
  await expect(page.getByRole('link', { name: /responsive typography/i })).toBeVisible();
  await expect(page.getByRole('link', { name: /mudblazor playbook/i })).toBeVisible();
  await expect(page.getByRole('heading', { name: /import brand pack/i })).toBeVisible({
    timeout: INTERACTIVE_TIMEOUT,
  });
  await expect(page.getByText(new RegExp(`token pack:.*${brand}`, 'i'))).toBeVisible({
    timeout: INTERACTIVE_TIMEOUT,
  });
}
