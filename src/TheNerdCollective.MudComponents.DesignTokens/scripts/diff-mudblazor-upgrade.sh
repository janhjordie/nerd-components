#!/usr/bin/env bash
# HR-147 / TS-053 — dry-run MudBlazor upgrade diff (palette manifest + upgrade report).
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
TESTS="$ROOT/../../tests/TheNerdCollective.MudComponents.DesignTokens.Tests/TheNerdCollective.MudComponents.DesignTokens.Tests.csproj"
FROM_VERSION="${1:-9.7.0}"
TO_VERSION="${2:-9.8.0}"
FROM_TAG="v${FROM_VERSION}"
TO_TAG="v${TO_VERSION}"
FROM_URL="https://raw.githubusercontent.com/MudBlazor/MudBlazor/${FROM_TAG}/src/MudBlazor/Components/ThemeProvider/MudThemeProvider.razor.cs"
SOURCE_URL="https://raw.githubusercontent.com/MudBlazor/MudBlazor/${TO_TAG}/src/MudBlazor/Components/ThemeProvider/MudThemeProvider.razor.cs"
BASELINE_FILE="$(mktemp -t mud-theme-baseline.XXXXXX.cs)"
CANDIDATE_FILE="$(mktemp -t mud-theme-candidate.XXXXXX.cs)"

cleanup() {
  rm -f "$BASELINE_FILE" "$CANDIDATE_FILE"
}
trap cleanup EXIT

echo "Fetching MudThemeProvider for ${FROM_VERSION}..."
if ! curl -fsSL "$FROM_URL" -o "$BASELINE_FILE"; then
  echo "Failed to download ${FROM_URL}" >&2
  exit 1
fi

echo "Fetching MudThemeProvider for ${TO_VERSION}..."
if ! curl -fsSL "$SOURCE_URL" -o "$CANDIDATE_FILE"; then
  echo "Failed to download ${SOURCE_URL}" >&2
  echo "Tip: Mud ${TO_VERSION} may not be tagged yet. Try an existing pair, e.g.:" >&2
  echo "  $0 9.6.0 9.7.0" >&2
  exit 1
fi

export MUD_UPGRADE_FROM="$FROM_VERSION"
export MUD_UPGRADE_TO="$TO_VERSION"
export MUD_UPGRADE_BASELINE_SOURCE="$BASELINE_FILE"
export MUD_UPGRADE_CANDIDATE_SOURCE="$CANDIDATE_FILE"

dotnet test "$TESTS" \
  --filter "FullyQualifiedName~NerdMudUpgradeDiffToolsTests.Write_upgrade_report_when_requested" \
  --nologo -v q

REPORT="$ROOT/reference/mudblazor/upgrades/${FROM_VERSION}-to-${TO_VERSION}.md"
echo "Wrote upgrade report: $REPORT"
