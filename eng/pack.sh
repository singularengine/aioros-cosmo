#!/usr/bin/env bash
# Usage: eng/pack.sh [version suffix]
# Packs Aioros.Cosmo, Aioros.Cosmo.Charts and Aioros.Cosmo.Markdown (with symbols) into artifacts/packages.
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
out="$root/artifacts/packages"
suffix="${1:-}"
mkdir -p "$out"
args=(-c Release -o "$out" --nologo -v quiet)
if [ -n "$suffix" ]; then args+=("-p:VersionSuffix=$suffix"); fi
for project in Aioros.Cosmo Aioros.Cosmo.Charts Aioros.Cosmo.Markdown; do
  dotnet pack "$root/src/$project/$project.csproj" "${args[@]}"
done
ls -1 "$out"
