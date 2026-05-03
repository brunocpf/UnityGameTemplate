#!/usr/bin/env bash
set -euo pipefail

usage() {
  cat <<'EOF'
Usage:
  ./scripts/run-tests.sh [edit|play|all]

Arguments:
  edit    Run Edit Mode tests only
  play    Run Play Mode tests only
  all     Run both Edit Mode and Play Mode tests (default)

Environment variables:
  UNITY_PATH        Optional absolute path to Unity executable
  TEST_RESULTS_DIR  Optional output directory for logs and XML results
EOF
}

MODE="${1:-all}"
if [[ "$MODE" != "edit" && "$MODE" != "play" && "$MODE" != "all" ]]; then
  usage
  exit 1
fi

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
RESULTS_DIR="${TEST_RESULTS_DIR:-$PROJECT_ROOT/TestResults}"
mkdir -p "$RESULTS_DIR"

resolve_unity_path() {
  if [[ -n "${UNITY_PATH:-}" ]]; then
    if [[ -x "$UNITY_PATH" ]]; then
      echo "$UNITY_PATH"
      return 0
    fi

    echo "UNITY_PATH is set but not executable: $UNITY_PATH" >&2
    exit 1
  fi

  local latest_unity
  latest_unity="$(ls -d /Applications/Unity/Hub/Editor/*/Unity.app/Contents/MacOS/Unity 2>/dev/null | sort -V | tail -n 1 || true)"

  if [[ -z "$latest_unity" ]]; then
    echo "Could not find Unity executable. Set UNITY_PATH to continue." >&2
    exit 1
  fi

  echo "$latest_unity"
}

UNITY_EXECUTABLE="$(resolve_unity_path)"

echo "Using Unity executable: $UNITY_EXECUTABLE"
echo "Project path: $PROJECT_ROOT"
echo "Results directory: $RESULTS_DIR"

run_tests() {
  local platform="$1"
  local xml_out="$RESULTS_DIR/TestResults-${platform}.xml"
  local log_out="$RESULTS_DIR/TestResults-${platform}.log"

  echo "Running ${platform} tests..."
  "$UNITY_EXECUTABLE" -batchmode -nographics -quit \
    -projectPath "$PROJECT_ROOT" \
    -runTests \
    -testPlatform "$platform" \
    -testResults "$xml_out" \
    -logFile "$log_out"

  echo "Finished ${platform} tests"
  echo "  XML: $xml_out"
  echo "  Log: $log_out"
}

case "$MODE" in
  edit)
    run_tests "EditMode"
    ;;
  play)
    run_tests "PlayMode"
    ;;
  all)
    run_tests "EditMode"
    run_tests "PlayMode"
    ;;
esac

echo "All requested test runs completed successfully."
