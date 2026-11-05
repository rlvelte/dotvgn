#!/usr/bin/env bash
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$root"

configuration="${CONFIGURATION:-Release}"
samples_dir="${SAMPLES_DIR:-./samples}"
publish_dir="${PUBLISH_DIR:-./build/publish}"

detect_rid() {
  local os arch
  case "$(uname -s)" in
    Linux*) os=linux ;;
    MINGW*|MSYS*|CYGWIN*) os=win ;;
    *) echo "unsupported OS: $(uname -s)" >&2; exit 1 ;;
  esac
  case "$(uname -m)" in
    x86_64|amd64) arch=x64 ;;
    aarch64|arm64) arch=arm64 ;;
    *) echo "unsupported arch: $(uname -m)" >&2; exit 1 ;;
  esac
  echo "${os}-${arch}"
}

rid="$(detect_rid)"

for csproj in "$root/$samples_dir"/*/*.csproj; do
  name="$(basename "$csproj" .csproj)"
  grep -q '<OutputType>Exe</OutputType>' "$csproj" || continue
  project_dir="$(dirname "$csproj")"
  source="$project_dir/bin/$configuration/net10.0/$rid/publish"
  out="$root/$publish_dir/$rid/$name"
  mkdir -p "$out"

  dotnet publish "$csproj" \
    -c "$configuration" \
    -r "$rid" \
    --self-contained \
    -p:StripSymbols=true \
    --verbosity minimal

  cp -a "$source/." "$out/"
  echo "published: $out"
done
