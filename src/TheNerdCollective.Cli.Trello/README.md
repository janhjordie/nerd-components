# TheNerdCollective.Cli.Trello

Minimal Trello CLI for the first useful workflow automation pass: creating or ensuring checklists on a card, adding checklist items, marking them complete, and adding card comments.

## MVP Scope

- Ensure a checklist exists on a Trello card
- Add checklist items
- Mark checklist items complete or incomplete
- Add comments to a card
- List card checklists and items for inspection

This package is intentionally small. It focuses on task-tracking flows that fit development work logs.

## Installation

```bash
dotnet tool install --global TheNerdCollective.Cli.Trello
```

For local testing from source:

```bash
dotnet pack src/TheNerdCollective.Cli.Trello -c Release
dotnet tool install --global --add-source src/TheNerdCollective.Cli.Trello/nupkg TheNerdCollective.Cli.Trello
```

## Authentication

Set these environment variables before running the tool:

```bash
export TRELLO_KEY="your_trello_api_key"
export TRELLO_TOKEN="your_trello_api_token"
```

Optional:

```bash
export TRELLO_BASE_URL="https://api.trello.com/1/"
```

Get your Trello API key and token from the Trello developer flow:

- API key from your Trello Power-Up admin page
- API token by authorizing that key for your Trello account

## Commands

### List checklists on a card

```bash
tnc-trello checklists --card 67f123abc456def789012345
```

### Ensure a checklist exists

```bash
tnc-trello ensure-checklist --card 67f123abc456def789012345 --name "Implementation"
```

### Add an item to a named checklist on a card

```bash
tnc-trello add-item --card 67f123abc456def789012345 --checklist "Implementation" --name "Add Trello CLI MVP"
```

### Add an item directly to a checklist id

```bash
tnc-trello add-item --checklist-id 67f999abc456def789012999 --name "Build SDK solution"
```

### Mark an item complete

```bash
tnc-trello set-item-state --card 67f123abc456def789012345 --item 67f111abc456def789012111 --state complete
```

### Add a comment to the card

```bash
tnc-trello comment --card 67f123abc456def789012345 --text "CLI MVP created and SDK solution builds."
```

## Notes

- This tool uses Trello REST API endpoints directly.
- Output is JSON for mutating commands so other scripts can pipe or parse it.
- The tool currently assumes you already know the Trello card id.

## Planned Next Steps

- Search cards by board and title
- Ensure checklist items by name instead of id-only updates
- Structured config file support in addition to environment variables
- Optional Markdown or compact text output mode