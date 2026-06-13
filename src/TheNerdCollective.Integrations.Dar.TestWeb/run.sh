#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "$0")"

PORT=5095

echo "Killing process on port ${PORT}..."
if command -v lsof >/dev/null 2>&1; then
  PIDS="$(lsof -ti :"${PORT}" 2>/dev/null || true)"
  if [ -n "${PIDS}" ]; then
    echo "${PIDS}" | xargs kill -9 2>/dev/null || true
  fi
fi

sleep 1

echo "Starting TestWeb on http://localhost:${PORT} ..."
dotnet run --launch-profile http
