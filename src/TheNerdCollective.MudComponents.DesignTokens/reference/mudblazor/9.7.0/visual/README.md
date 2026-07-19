# MudBlazor visual regression baselines

Playwright locator screenshots for palette-first dogfood surfaces. Used by `tests/e2e/mud-visual-regression.spec.ts` (TS-052 / HR-146).

## Layout

```text
visual/
  README.md
  tnc-design-guide-layout-scene.png
  dnf-design-guide-layout-scene.png
  tnc-playbook-section-tabs.png
  tnc-design-tokens-coral-buttons.png
  dnf-design-tokens-skov-buttons.png
```

Snapshots are pinned to Mud **9.7.0** brand rendering. When bumping MudBlazor:

1. Run harvest + palette manifest diff (`scripts/diff-mudblazor-upgrade.sh`).
2. Update adapter + regenerate CSS.
3. Refresh baselines:

```bash
cd tests/e2e
CI=1 npx playwright test mud-visual-regression --update-snapshots
```

4. Commit updated PNGs under the new `reference/mudblazor/{version}/visual/` folder.

## Conventions

- Viewport: 1280×720, `deviceScaleFactor: 1`
- Animations disabled; style-guard badge hidden during capture
- TNC + DNF matrix catches alias/palette regressions across brands
