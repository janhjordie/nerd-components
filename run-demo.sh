#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "$0")" && pwd)"
DEMO_PORTS=(5072 7252)

kill_port() {
  local port=$1
  local pids
  pids=$(lsof -tiTCP:"$port" -sTCP:LISTEN 2>/dev/null || true)
  if [ -z "$pids" ]; then
    return 0
  fi

  echo "Stopping previous demo on port $port (PID: $(echo "$pids" | tr '\n' ' '))"
  # shellcheck disable=SC2086
  kill $pids 2>/dev/null || true

  for _ in {1..10}; do
    if ! lsof -tiTCP:"$port" -sTCP:LISTEN >/dev/null 2>&1; then
      return 0
    fi
    sleep 0.2
  done

  pids=$(lsof -tiTCP:"$port" -sTCP:LISTEN 2>/dev/null || true)
  if [ -n "$pids" ]; then
    echo "Force-stopping process(es) on port $port"
    # shellcheck disable=SC2086
    kill -9 $pids 2>/dev/null || true
  fi
}

echo "Starting demo from $ROOT_DIR"

for port in "${DEMO_PORTS[@]}"; do
  kill_port "$port"
done

PROJECT_PATH="$ROOT_DIR/src/TheNerdCollective.Demo/TheNerdCollective.Demo.csproj"
if [ ! -f "$PROJECT_PATH" ]; then
  echo "Demo project not found at $PROJECT_PATH"
  exit 1
fi

export DOTNET_HOST_SHUTDOWN_TIMEOUT=5

shutdown_requested=0
dotnet_pid=""

cleanup() {
  if [ -z "$dotnet_pid" ]; then
    exit 130
  fi

  if [ "$shutdown_requested" -eq 0 ]; then
    shutdown_requested=1
    echo ""
    echo "Shutting down demo (press Ctrl+C again to force stop)..."
    kill -INT "$dotnet_pid" 2>/dev/null || true
    return
  fi

  echo "Force stopping demo..."
  kill -9 "$dotnet_pid" 2>/dev/null || true
  exit 130
}

trap cleanup INT TERM

dotnet run --project "$PROJECT_PATH" --launch-profile http &
dotnet_pid=$!
wait "$dotnet_pid"
