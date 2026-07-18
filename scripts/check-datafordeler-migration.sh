#!/usr/bin/env bash
# Datafordeler migrations-check: sammenligner opslag med og uden DAWA-fallback.
# Kræver DATAFORDELER_API_KEY eller TheNerdCollective__Dar__ApiKey (eller TestWeb appsettings.local.json).
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

if [[ -z "${DATAFORDELER_API_KEY:-}" && -z "${TheNerdCollective__Dar__ApiKey:-}" ]]; then
  LOCAL="${ROOT}/src/TheNerdCollective.Integrations.Dar.TestWeb/appsettings.local.json"
  if [[ -f "$LOCAL" ]]; then
    export TheNerdCollective__Dar__ApiKey="$(
      python3 -c "import json,sys; print(json.load(open(sys.argv[1]))['TheNerdCollective']['Dar']['ApiKey'])" "$LOCAL"
    )"
  fi
fi

ARTIFACTS="${ROOT}/artifacts"
mkdir -p "$ARTIFACTS"
TIMESTAMP="$(date +%Y%m%d-%H%M%S)"
REPORT_FILE="${ARTIFACTS}/datafordeler-migration-report-${TIMESTAMP}.txt"
export DAR_MIGRATION_REPORT_PATH="$REPORT_FILE"

echo "Kører Datafordeler migration probe..."
echo "Rapport gemmes til: $REPORT_FILE"
echo

dotnet test tests/TheNerdCollective.Integrations.Dar.IntegrationTests/TheNerdCollective.Integrations.Dar.IntegrationTests.csproj \
  --filter "FullyQualifiedName~DarDawaMigrationProbeTests" \
  --logger "console;verbosity=normal" \
  --nologo

echo
if [[ -f "$REPORT_FILE" ]]; then
  echo "--- Rapport (kopi) ---"
  cat "$REPORT_FILE"
  echo
  if grep -q 'READY_TO_DISABLE_DAWA_FALLBACK=true' "$REPORT_FILE"; then
    echo "✓ Datafordeler dækker alle opslag — DAWA-fallback kan overvejes fjernet."
    exit 0
  fi
  if grep -q 'DAF-AUTH-0005' "$REPORT_FILE"; then
    echo "⚠ Datafordeler blokeret (IP ikke whitelisted) — migration ikke fuldt afgjort."
    exit 2
  fi
  echo "→ DAWA-fallback er stadig nødvendig for mindst ét opslag."
  exit 0
fi

echo "Rapportfil blev ikke oprettet — se test-output ovenfor."
exit 1
