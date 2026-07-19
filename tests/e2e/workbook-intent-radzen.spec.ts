import { expect, test } from '@playwright/test';
import { INTERACTIVE_TIMEOUT } from './catalog-helpers';
import { MUD_DEFAULT_PRIMARY_RGB, TNC_CORAL_RGB } from './mud-palette-helpers';

async function openWorkbookIntentsStep(page: import('@playwright/test').Page) {
  await page.goto('/nerd-brand-workbook');
  await expect(page.getByRole('heading', { name: /brand workbook/i })).toBeVisible({
    timeout: INTERACTIVE_TIMEOUT,
  });

  const intentsTab = page.getByRole('tab', { name: /intents/i });
  await intentsTab.scrollIntoViewIfNeeded();
  await intentsTab.click();
  await expect(page.getByTestId('workbook-intents-panel')).toBeVisible({
    timeout: INTERACTIVE_TIMEOUT,
  });
}

test.describe('Workbook intent previews (HR-155)', () => {
  test('intents step shows mud and radzen side-by-side', async ({ page }) => {
    await openWorkbookIntentsStep(page);

    await expect(page.getByTestId('workbook-intent-mud-preview')).toBeVisible();
    await expect(page.getByTestId('workbook-intent-radzen-preview')).toBeVisible();
    await expect(page.getByTestId('workbook-radzen-primary-button')).toBeVisible();
  });

  test('primary previews use TNC brand color not mud default', async ({ page }) => {
    await openWorkbookIntentsStep(page);

    const mudButton = page
      .getByTestId('workbook-intent-mud-preview')
      .getByRole('button', { name: 'Primary button' });
    const mudBackground = await mudButton.evaluate((element) => getComputedStyle(element).backgroundColor);
    expect(mudBackground).not.toBe(MUD_DEFAULT_PRIMARY_RGB);
    expect(mudBackground).toBe(TNC_CORAL_RGB);

    const radzenButton = page.getByTestId('workbook-radzen-primary-button');
    const radzenBackground = await radzenButton.evaluate((element) => getComputedStyle(element).backgroundColor);
    expect(radzenBackground).not.toBe(MUD_DEFAULT_PRIMARY_RGB);
    expect(radzenBackground).toBe(TNC_CORAL_RGB);
  });
});
