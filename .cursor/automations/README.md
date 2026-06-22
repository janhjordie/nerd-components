# Cursor Automations (projekt-specifikke)

## Datafordeler / DAWA ugentlig watch

**Formål:** Følg med i hvornår DAGI GraphQL på Datafordeler har nok geodata til at DAWA-fallback kan fjernes i `TheNerdCollective.Integrations.Dar`.

### Hvor finder jeg Automations i Cursor?

Automations ligger **ikke** i Settings eller Command Palette som almindelig menu. Sådan finder du det:

1. **Agents Window** (agent-panelet i IDE — ikon for agenter/chat i sidebaren).
2. Fanen **Automations** øverst i Agents Window.
3. **New automation** → konfigurer trigger, prompt og repo.

**Web (altid tilgængelig):** [cursor.com/automations](https://cursor.com/automations) eller [cursor.com/automations/new](https://cursor.com/automations/new) — log ind med samme Cursor-konto.

**Hvis fanen mangler:** Opdater Cursor (**Cursor → About Cursor**). Automations i IDE kræver nyere Cursor (ca. 3.5+). Brug web-URL'en ovenfor eller GitHub Actions-workflowet nedenfor.

### Opsætning (web eller IDE)

1. Opret ny automation (web eller Agents Window).
2. **Trigger:** Scheduled → cron `0 8 * * 1` (mandag kl. 08:00 UTC — justér tidszone i editoren).
3. **Repository:** `janhjordie/nerd-components`, branch `main`.
4. **Prompt:** kopier fra `datafordeler-dawa-weekly-watch.yaml` (feltet `prompts`).
5. Aktivér **Cloud Agent** og gem.

YAML-filen i denne mappe er reference — Cursor importerer ikke altid filer direkte; kopier navn, beskrivelse, cron og prompt manuelt.

### Alternativ: GitHub Actions (kræver ikke Cursor Automations)

Workflow `.github/workflows/datafordeler-dawa-watch.yml` kører samme probe mandag morgen. Opret secret `DATAFORDELER_API_KEY` — bemærk at CI-runner typisk **ikke** er IP-whitelisted på Datafordeler.

### Manuel probe

```bash
chmod +x scripts/check-datafordeler-dagi-readiness.sh
./scripts/check-datafordeler-dagi-readiness.sh
```

Se `docs/monitoring/DATAFORDELER-DAWA-WATCH.md` for officielle kilder.
