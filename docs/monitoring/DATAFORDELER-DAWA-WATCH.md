# Datafordeler / DAWA — ugentlig overvågning

Pakken `TheNerdCollective.Integrations.Dar` bruger **DAWA** som fallback mens **DAGI GraphQL** på Datafordeler stadig returnerer tomme resultater for geografi (kommune-liste, punkt-opslag, spatial intersect). Denne guide beskriver hvor vi følger med, og hvornår DAWA-fallback kan fjernes.

## Hvornår kan DAWA-fallback fjernes?

Alle punkter skal være opfyldt i **produktions-lignende miljø** (whitelisted IP + gyldig API-nøgle):

| Signal | Datafordeler-only test / probe | DAWA i dag |
|--------|--------------------------------|------------|
| Kommune-liste | `Kommune.GetAllAsync` via GraphQL ≥ 90 kommuner | DAWA fallback |
| Punkt → kommune | `FindByCoordinatesDatafordelerAsync` returnerer kommune + repræsentativt punkt | DAWA reverse |
| Cirkel → postnumre | `GetByCircleAsync` uden DAWA returnerer forventede postnumre | DAWA `?cirkel=` |
| Multi-kommune | `GetByMunicipalityCodeWithKommunerAsync` uden DAWA returnerer data | DAWA `?kommunekode=` |

Kør probe lokalt eller i CI:

```bash
./scripts/check-datafordeler-dagi-readiness.sh
# eller
dotnet test tests/TheNerdCollective.Integrations.Dar.IntegrationTests \
  --filter "FullyQualifiedName~DarDagiReadinessProbeTests"
```

Output-linjen `READY_TO_DISABLE_DAWA_FALLBACK=true` betyder at I bør planlægge fjernelse af fallback (stadig manuel beslutning + release).

## Officielle kilder (tjek ugentligt)

### Datafordeler — migration og udfasning

| Kilde | URL | Hvad du finder |
|-------|-----|----------------|
| Forside + udfasning 2026 | https://datafordeler.dk/ | Link til planlagt udfasning af gamle tjenester |
| Tjenester til udfasning 2026 | https://confluence.sdfi.dk/display/DML/Oversigt+over+tjenester+til+udfasning+2026 | WFS/REST/WMS der lukker juli/ultimo 2026 |
| Transitionsnetværk | https://datafordeler.dk/vejledning/transitionsnetvaerk/ | Møder, status, tilmelding `daf2@kds.dk` |
| Transition-events | https://datafordeler.dk/Transition | Aktuelle begivenheder |
| Mapning DAWA → Datafordeler | https://confluence.sdfi.dk/display/DML/Mapning+fra+DAWA+til+Datafordeleren | **DAWA lukker 17. august 2026** |
| GraphQL transitionsguide (PDF) | https://datafordeler.dk/media/unppjisf/transitionsguide-graphql.pdf | GraphQL vs REST, paginering |
| REST DAGI (udfases) | https://confluence.sdfi.dk/pages/viewpage.action?pageId=13666129 | REST DAGI udfases **15. januar 2027** |
| Nyt DAGI-system (migration) | https://confluence.sdfi.dk/pages/viewpage.action?pageId=225247260 | DAGI3-migration, `SystemMigreringDagi3` |

### DAWA — udfasning

| Kilde | URL | Hvad du finder |
|-------|-----|----------------|
| DAWA dokumentation | https://dawadocs.dataforsyningen.dk/ | API-reference (postnumre, kommuner, cirkel) |
| Dataforsyningen dataset 4924 | https://dataforsyningen.dk/data/4924 | Officiel udfasningsbesked (kan kræve VPN) |
| Klimadatastyrelsen nyhed | https://www.klimadatastyrelsen.dk/om-klimadatastyrelsen/nyheder/nyhedsarkiv/2025/aug/moderniseringen-af-datafordeleren-naermer-sig-sin-afslutning | Paralleldrift til **30. juni 2026** |

### Vigtige datoer (jun 2026)

- **30. juni 2026** — paralleldrift ophører; ikke-moderniserede Datafordeler-tjenester skal være udfaset
- **1. juli 2026** — flere gamle WFS/REST-tjenester lukker (se Confluence-oversigt)
- **17. august 2026** — **DAWA lukker** (Confluence mapning)
- **15. januar 2027** — REST DAGI på Datafordeler udfases

### Praktisk signal for *vores* pakke

Tom DAGI GraphQL er **ikke** det samme som “Datafordeler er nede” — det skyldes typisk at geodata stadig migreres (jf. nyt DAGI-system). Følg derfor **Transitionsnetværket** og kør **readiness-proben**; offentlige datoer siger hvornår DAWA *skal* være væk, ikke hvornår GraphQL-geografi *virker*.

## Automatisering i dette repo

| Mekanisme | Fil | Frekvens |
|-----------|-----|----------|
| Cursor Automation (agent) | `.cursor/automations/datafordeler-dawa-weekly-watch.yaml` | Mandage kl. 09:00 (Europe/Copenhagen) |
| GitHub Actions | `.github/workflows/datafordeler-dawa-watch.yml` | Mandage kl. 07:00 UTC |
| Probe-script | `scripts/check-datafordeler-dagi-readiness.sh` | Manuelt / CI |

### GitHub Actions

Workflow kræver secret `DATAFORDELER_API_KEY` (samme nøgle som TestWeb). Den opdaterer ikke kode automatisk — den kører proben og uploader rapport som artifact.

### Cursor Automation

Importér workflow-filen i **Cursor → Automations → Create** (prefill). Agenten læser denne guide, søger nettet efter ændringer på kilderne ovenfor, kører proben, og opdaterer `docs/monitoring/last-status.md` hvis status ændrer sig.

Kontakt ved spørgsmål om transition: **daf2@kds.dk**

### Tilmeld transitionsnetværket

Der er **ingen online formular** — tilmelding sker via e-mail:

- **Info og baggrund:** [datafordeler.dk/vejledning/transitionsnetvaerk/](https://datafordeler.dk/vejledning/transitionsnetvaerk/)
- **Tilmeld dig:** [mailto:DAF2@kds.dk?subject=Tilmelding%20til%20Transitionsnetværket](mailto:DAF2@kds.dk?subject=Tilmelding%20til%20Transitionsnetværket)
- **Kommende møder og materialer:** [datafordeler.dk/Transition](https://datafordeler.dk/Transition)
