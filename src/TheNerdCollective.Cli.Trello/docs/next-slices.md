# Next Slices

## Slice 1: Card Lookup by Title

Goal:
- avoid requiring raw Trello card ids for day-to-day usage

Add:
- board lookup by board name or id
- card search by exact or partial title within a board
- `--card-name` and `--board` support on all current commands

Notes:
- first require board scope to avoid ambiguous global search
- support `--exact` and `--first`

## Slice 2: Ensure Item by Name

Goal:
- make checklist item updates idempotent

Add:
- `ensure-item` command
- optional `--checklist` or `--checklist-id`
- `set-item-state` support by item name, not only item id

Notes:
- use exact name match first
- fail if multiple checklist items have the same name unless `--first` is passed

## Slice 3: Structured Output Modes

Goal:
- make the CLI easier to use in both humans and scripts

Add:
- `--output json|text|compact`
- `--quiet` for mutation commands
- non-zero exit codes with stable error messages

## Slice 4: Config File Support

Goal:
- reduce repeated environment setup for local development

Add:
- optional config file at `.tnc-trello.json`
- env vars still override config values
- config supports key, token, default board, and default checklist

## Slice 5: Worklog Command

Goal:
- one command to log a completed work slice

Add:
- `worklog` command that:
  - ensures checklist exists
  - ensures item exists
  - marks item complete
  - adds a comment with timestamp and summary

Example:

```bash
tnc-trello worklog --board "BilletSalg" --card-name "Seat plan image upload" --checklist "Implementation" --item "Legacy fallback for venue client ownership" --comment "Upload verified locally"
```

## Slice 6: Packaged Install Experience

Goal:
- make local and NuGet installation frictionless

Add:
- package icon
- changelog or release notes discipline
- install script examples for macOS and CI