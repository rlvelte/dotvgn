#!/usr/bin/env bash
# Build all samples as AOT binaries and package them as RPMs.
# Usage: ./samples/build-rpm.sh [version]

set -euo pipefail

SAMPLES_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT="$(dirname "$SAMPLES_DIR")"
VERSION="${1:-$(git -C "$ROOT" describe --tags --abbrev=0 | sed 's/^v//')}"
OUT_DIR="$ROOT/rpm"
WORK="$(mktemp -d)"
trap 'rm -rf "$WORK"' EXIT

shopt -s nullglob
specs=("$SAMPLES_DIR"/*/*.spec)
if [ ${#specs[@]} -eq 0 ]; then
  echo "No spec files found in $SAMPLES_DIR/*/" >&2
  exit 1
fi

mkdir -p "$WORK/SOURCES"

for spec in "${specs[@]}"; do
  sample_dir="$(dirname "$spec")"
  name="$(sed -n 's/^Name:[[:space:]]*//p' "$spec" | head -1)"
  if [ -z "$name" ]; then
    echo "Skipping $spec: no 'Name:' field" >&2
    continue
  fi

  echo "Building $name (from $(basename "$sample_dir"), v${VERSION})"
  dotnet publish "$sample_dir" -c Release -r linux-x64 --self-contained -o "$WORK/publish/$name"

  echo "Staging tarball for $name"
  mkdir "$WORK/$name-$VERSION"
  cp "$WORK/publish/$name/$name" "$WORK/$name-$VERSION/"
  cp "$ROOT/LICENSE" "$WORK/$name-$VERSION/"
  tar -czf "$WORK/SOURCES/$name-$VERSION.tar.gz" -C "$WORK" "$name-$VERSION"

  echo "Building RPM for $name"
  rpmbuild -bb \
    --define "_sourcedir $WORK/SOURCES" \
    --define "_rpmdir $OUT_DIR" \
    --define "pkg_version $VERSION" \
    "$spec"
done

find "$OUT_DIR" -name '*.rpm'