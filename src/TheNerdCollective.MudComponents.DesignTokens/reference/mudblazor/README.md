# MudBlazor versioned CSS archive

Committed analysis for each pinned MudBlazor NuGet version. Used for adapter upgrades, golden diffs, and agent harvest runs.

See `docs/01-mudblazor-css-generator-analysis.md` (§5.4) in the Token Studio repo for the full workflow.

## Layout per version

```text
{version}/
  MANIFEST.json
  ANALYSIS.md
  coverage-matrix.md
  inventory/{component}.yaml
  visual/                      # Playwright screenshot baselines (HR-146)
  upgrades/{from}-to-{to}.md   # sibling folder under mudblazor/
```

Scripts:

- `scripts/harvest-mudblazor-inventory.sh` — inventory + CSS rule-table gate
- `scripts/diff-mudblazor-upgrade.sh {from} {to}` — palette manifest diff report (HR-147)

Never delete older version folders. Add a new folder when bumping MudBlazor.
