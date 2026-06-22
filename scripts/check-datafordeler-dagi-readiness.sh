#!/usr/bin/env bash
# Kører DAGI readiness-probe mod TheNerdCollective.Integrations.Dar.
# Kræver DATAFORDELER_API_KEY eller TheNerdCollective__Dar__ApiKey i miljøet.
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

dotnet test tests/TheNerdCollective.Integrations.Dar.IntegrationTests/TheNerdCollective.Integrations.Dar.IntegrationTests.csproj \
  --filter "FullyQualifiedName~DarDagiReadinessProbeTests" \
  --logger "console;verbosity=detailed" \
  --nologo
