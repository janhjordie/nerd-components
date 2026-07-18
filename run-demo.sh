#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "$0")" && pwd)"
echo "Starting MudQuillEditor demo from $ROOT_DIR"

PROJECT_PATH="$ROOT_DIR/src/TheNerdCollective.Demo/TheNerdCollective.Demo.csproj"
if [ ! -f "$PROJECT_PATH" ]; then
  echo "Demo project not found at $PROJECT_PATH"
  exit 1
fi

dotnet run --project "$PROJECT_PATH"
