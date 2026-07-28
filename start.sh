#!/usr/bin/env bash
#
# StudyBuddy — start both the backend API and the frontend dev server.
#
# Usage:  ./start.sh
# Stop:   press Ctrl+C once (both processes are shut down together)
#
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BACKEND_DIR="$REPO_ROOT/backend/StudyBuddy.API"
FRONTEND_DIR="$REPO_ROOT/frontend"

BACKEND_PORT=5017
FRONTEND_PORT=5180

backend_pid=""
frontend_pid=""

# Shut both processes down together, however the script exits.
cleanup() {
  echo ""
  echo "Shutting down…"
  [[ -n "$backend_pid" ]] && kill "$backend_pid" 2>/dev/null || true
  [[ -n "$frontend_pid" ]] && kill "$frontend_pid" 2>/dev/null || true
  wait 2>/dev/null || true
  echo "Stopped."
}
trap cleanup EXIT INT TERM

# Fail early with a clear message rather than a confusing CORS error later.
check_port() {
  local port=$1
  local label=$2
  if lsof -ti:"$port" >/dev/null 2>&1; then
    echo "ERROR: port $port ($label) is already in use."
    echo "       Find it with:  lsof -i :$port"
    echo "       Free it with:  lsof -ti:$port | xargs kill -9"
    exit 1
  fi
}

check_port "$BACKEND_PORT" "backend"
check_port "$FRONTEND_PORT" "frontend"

echo "Starting StudyBuddy…"
echo ""

cd "$BACKEND_DIR"
dotnet run &
backend_pid=$!

cd "$FRONTEND_DIR"
npm run dev &
frontend_pid=$!

echo ""
echo "  Backend   http://localhost:$BACKEND_PORT"
echo "  Frontend  http://localhost:$FRONTEND_PORT"
echo ""
echo "Press Ctrl+C to stop both."
echo ""

# Keep the script alive until either process exits.
wait -n "$backend_pid" "$frontend_pid"
