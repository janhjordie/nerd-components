# Ideas

## Great Near-Term Suggestions

- Add `ensure-card-comment` with dedupe markers so the same status note is not posted twice.
- Add `attach-link` so a build URL, PR URL, or local docs link can be added to the Trello card.
- Add `start-slice` and `finish-slice` commands to create a lightweight audit trail for focused work sessions.
- Add `sync-markdown-checklist` to mirror a markdown task list into a Trello checklist.
- Add `export-card-context` to print card title, labels, checklist status, and recent comments in one compact view.

## Useful Dev Workflow Suggestions

- Support reading card metadata from git branch names, for example `feature/trello-abc123-seatplan-upload`.
- Support a local session file that remembers the active Trello card for the current repo.
- Add `--comment-template` support for standard completion notes.
- Add a dry-run mode before any write operation.

## Longer-Range Suggestions

- Add board and list creation commands if this evolves from a task logger into a broader Trello automation tool.
- Add webhook companion tooling for inbound automation.
- Add GitHub PR integration so a PR can update a Trello checklist or comment automatically.
- Add an opinionated `release-note` command that writes a structured deployment comment back to Trello.

## Design Constraints to Keep

- Keep the CLI script-friendly before making it pretty.
- Prefer explicit flags over hidden defaults.
- Keep auth out of source and config committed to git.
- Keep command names short and task-oriented.