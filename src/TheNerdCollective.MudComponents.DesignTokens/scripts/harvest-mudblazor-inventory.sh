#!/usr/bin/env bash
# HR-133 / HR-134 — fetch MudBlazor SCSS list, validate inventory YAML, and gate on generated CSS rule table.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
TESTS="$ROOT/../../tests/TheNerdCollective.MudComponents.DesignTokens.Tests/TheNerdCollective.MudComponents.DesignTokens.Tests.csproj"
VERSION="${MUD_VERSION:-$(node -pe "require('fs').readFileSync('$ROOT/reference/mudblazor/9.7.0/MANIFEST.json','utf8')" 2>/dev/null | sed -n 's/.*"mudblazorVersion"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p' || echo "9.7.0")}"
TAG="v${VERSION}"
INVENTORY="$ROOT/reference/mudblazor/${VERSION}/inventory"
SOURCES="$ROOT/reference/mudblazor/${VERSION}/sources"
LIST_FILE="$SOURCES/COMPONENTS.md"
RULE_TABLE="$ROOT/reference/mudblazor/${VERSION}/generated-rule-table.md"

mkdir -p "$SOURCES" "$INVENTORY"

echo "# MudBlazor ${VERSION} component SCSS" >"$LIST_FILE"
echo "" >>"$LIST_FILE"
echo "Fetched from https://github.com/MudBlazor/MudBlazor/tree/${TAG}/src/MudBlazor/Styles/components" >>"$LIST_FILE"
echo "" >>"$LIST_FILE"

curl -fsSL "https://api.github.com/repos/MudBlazor/MudBlazor/git/trees/${TAG}?recursive=1" \
  | python3 -c "
import json, sys
data = json.load(sys.stdin)
for item in sorted(data.get('tree', []), key=lambda x: x.get('path','')):
    path = item.get('path','')
    if '/components/_' in path and path.endswith('.scss'):
        print(path.split('/')[-1])
" >>"$LIST_FILE"

echo "Wrote $(wc -l <"$LIST_FILE" | tr -d ' ') lines to $LIST_FILE"

HARVEST_FILTER="FullyQualifiedName~NerdMudInventoryValidatorTests|FullyQualifiedName~NerdMudInventoryRuleTableTests|FullyQualifiedName~NerdMudStateParityToolsTests|FullyQualifiedName~MudBlazorComponentStateBridgeTests|FullyQualifiedName~MudBlazorVersionSyncTests"

dotnet test "$TESTS" --filter "$HARVEST_FILTER" --nologo -v q

dotnet test "$TESTS" --filter "FullyQualifiedName~NerdMudInventoryRuleTableTests.WriteGeneratedArtifacts_updates_committed_markdown" --nologo -v q

if [[ -f "$ROOT/bin/Debug/net10.0/reference/mudblazor/${VERSION}/generated-rule-table.md" ]]; then
  cp "$ROOT/bin/Debug/net10.0/reference/mudblazor/${VERSION}/generated-rule-table.md" "$RULE_TABLE"
elif [[ -f "$ROOT/../../tests/TheNerdCollective.MudComponents.DesignTokens.Tests/bin/Debug/net10.0/reference/mudblazor/${VERSION}/generated-rule-table.md" ]]; then
  cp "$ROOT/../../tests/TheNerdCollective.MudComponents.DesignTokens.Tests/bin/Debug/net10.0/reference/mudblazor/${VERSION}/generated-rule-table.md" "$RULE_TABLE"
fi

echo "Inventory validation + CSS rule-table gate passed for $INVENTORY"
