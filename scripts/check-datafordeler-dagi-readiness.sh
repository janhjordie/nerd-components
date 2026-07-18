#!/usr/bin/env bash
# Alias — brug check-datafordeler-migration.sh for fuld rapport.
exec "$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)/check-datafordeler-migration.sh" "$@"
