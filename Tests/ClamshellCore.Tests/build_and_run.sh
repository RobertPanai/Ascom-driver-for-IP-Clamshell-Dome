#!/usr/bin/env bash
# Compiles ../../Clamshell/Core/*.cs together with the ClamshellCore.Tests sources using Mono's mcs,
# then runs the test exe with mono and CLAMSHELL_TEST_MODE=1 (loopback-only network guard).
# Usage: ./build_and_run.sh [name-filter]
set -euo pipefail

HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CORE_DIR="$HERE/../../Clamshell/Core"
OUT_DIR="$(mktemp -d "${TMPDIR:-/tmp}/clamshell-core-tests.XXXXXX")"
EXE="$OUT_DIR/ClamshellCore.Tests.exe"

command -v mcs >/dev/null 2>&1 || { echo "mcs (Mono C# compiler) not found" >&2; exit 2; }
command -v mono >/dev/null 2>&1 || { echo "mono not found" >&2; exit 2; }

shopt -s nullglob
CORE_FILES=( "$CORE_DIR"/*.cs )
TEST_FILES=( "$HERE"/*.cs )
shopt -u nullglob

if [ ${#CORE_FILES[@]} -eq 0 ]; then
    echo "No Core sources found in $CORE_DIR" >&2
    exit 2
fi

echo "Compiling ${#CORE_FILES[@]} Core file(s) and ${#TEST_FILES[@]} test file(s) -> $EXE"
# mcs 6.8 accepts language versions up to 7.2 (the project targets C# 7.3; keep code to 7.2-or-lower features).
mcs -nologo -langversion:7.2 -target:exe -main:ClamshellCore.Tests.TestProgram \
    -out:"$EXE" \
    -r:System.dll \
    -r:System.Core.dll \
    -r:System.Runtime.Serialization.dll \
    -r:System.Security.dll \
    -r:System.Net.Http.dll \
    "${CORE_FILES[@]}" "${TEST_FILES[@]}"

echo "Running tests"
CLAMSHELL_TEST_MODE=1 mono "$EXE" "$@"
