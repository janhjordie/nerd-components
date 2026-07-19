import { test, expect } from '@playwright/test';
import { INTERACTIVE_TIMEOUT } from './catalog-helpers';

test.describe('Token tree navigator', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/nerd-design-tokens');
    await expect(page.getByRole('heading', { name: 'Design token colors' })).toBeVisible({
      timeout: INTERACTIVE_TIMEOUT,
    });
  });

  test('shows grouped tree with spacing tokens for TNC', async ({ page }) => {
    const tree = page.getByTestId('token-tree-navigator');
    await expect(tree).toBeVisible();
    await expect(tree.getByText(/spacing/i)).toBeVisible();
    await expect(page.getByTestId('token-tree-item-Spacing-4')).toBeVisible();
  });

  test('search filters tree items', async ({ page }) => {
    await page.getByTestId('token-tree-search').fill('coral');
    await expect(page.getByTestId('token-tree-item-Color-coral')).toBeVisible();
    await expect(page.getByTestId('token-tree-item-Spacing-4')).toHaveCount(0);
  });

  test('clicking spacing token scrolls spacing panel into view', async ({ page }) => {
    await page.getByTestId('token-tree-item-Spacing-8').click();
    await expect(page.getByTestId('token-spacing-8')).toBeInViewport();
  });
});
