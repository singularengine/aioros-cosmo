#!/usr/bin/env bash
# Cosmo's lab bench (docs/benchmarks.md): the Gallery served by Aioros, measured beside shadcn/ui's own blocks served by
# Next.js (tools/shadcn/next), the stack shadcn/ui targets, on the same machine and cores.
#   aioros         the Gallery published in Release and precompiled (aioros.plans.json), production
#   aioros-base    another commit's Gallery publish in AIOROS_BASE_DIR, measured beside this tree
#   aioros-engine  the same publish without its plans file: Avalonia starts with the server and compiles every page
#                  (measured once, for the posts the plans do not answer)
#   nextjs         the blocks rendered on every request (force-dynamic), one Next.js server per server core
#   nextjs-static  the blocks prerendered at build time (force-static), the same servers
# Per run of a variant: cold start (to the first page's 200), the warm-up (Aioros's "Warm-up finished"), idle memory;
# per page its weight (raw, gzip, as served) and a closed loop at CONNECTIONS connections for SECS seconds (LoadGen, its
# peak memory); on the first run of each variant the browser metrics (bench/browser.mjs). Runs are interleaved.
# The precompile is timed once and recorded with the plans file's size.
# usage: bench/gallery.sh        Env: VARIANTS, RUNS (3), SECS (5), CONNECTIONS (64), BROWSER_RUNS (5), SERVER_CPUS,
#        CLIENT_CPUS, SKIP_BUILD=1, AIOROS_BASE_DIR, OUT (bench/results/gallery-<time>)
set -euo pipefail
cd "$(dirname "$0")/.."
ROOT=$PWD
VARIANTS=${VARIANTS:-"aioros nextjs nextjs-static aioros-engine${AIOROS_BASE_DIR:+ aioros-base}"}
RUNS=${RUNS:-3}
SECS=${SECS:-5}
CONNECTIONS=${CONNECTIONS:-64}
BROWSER_RUNS=${BROWSER_RUNS:-5}
SERVER_CPUS=${SERVER_CPUS:-0-7}
CLIENT_CPUS=${CLIENT_CPUS:-8-31}
NEXT_WORKERS=${NEXT_WORKERS:-$(taskset -c "$SERVER_CPUS" nproc)}
OUT=${OUT:-bench/results/gallery-$(date +%Y%m%d-%H%M%S)}
PUBLISH=$ROOT/bench/out/gallery
ENGINE=$ROOT/bench/out/gallery-engine
SHADCN=$ROOT/tools/shadcn
LOADGEN="taskset -c $CLIENT_CPUS dotnet $ROOT/bench/LoadGen/bin/Release/net10.0/LoadGen.dll"
mkdir -p "$OUT"
export ASPNETCORE_ENVIRONMENT=Production NEXT_TELEMETRY_DISABLED=1

# The pages, as key=path: the keys the stacks share are compared with each other in the report.
AIOROS_PAGES="dashboard-01=/fidelity/dashboard-01 sidebar-01=/fidelity/sidebar-01 login-01=/fidelity/login-01 home=/ button=/components/button button-variants=/fidelity/button-variants themes=/themes?color=blue&radius=large binding=/binding data-table=/components/data-table"
NEXT_PAGES="dashboard-01=/dashboard-01 sidebar-01=/sidebar-01 login-01=/login-01"

if [[ ${SKIP_BUILD:-0} != 1 ]]; then
  echo "== building"
  dotnet publish samples/Gallery/Gallery -c Release -o "$PUBLISH" -v q -nologo > /dev/null
  rm -f "$PUBLISH/aioros.plans.json"
  t0=$(date +%s%N)
  (cd "$PUBLISH" && Logging__LogLevel__Default=Warning dotnet Gallery.dll --precompile="$PUBLISH/aioros.plans.json" > "$ROOT/$OUT/precompile.log" 2>&1)
  precompile=$(( ($(date +%s%N) - t0) / 1000000 ))
  rm -rf "$ENGINE" && cp -r "$PUBLISH" "$ENGINE" && rm "$ENGINE/aioros.plans.json"
  python3 -c "import json,sys; json.dump(dict(ms=int(sys.argv[1]), plansBytes=int(sys.argv[2]), report=[l.strip() for l in open(sys.argv[3]) if l.startswith(('Identical', 'Fresh', 'Generated', 'Precompiled'))]), open(sys.argv[4], 'w'), indent=1)" \
    "$precompile" "$(stat -c %s "$PUBLISH/aioros.plans.json")" "$OUT/precompile.log" "$OUT/precompile.json"
  echo "   precompile: $precompile ms, plans $(stat -c %s "$PUBLISH/aioros.plans.json") B"
  dotnet build bench/LoadGen -c Release -v q -nologo > /dev/null
  [[ -d $SHADCN/node_modules ]] || (cd "$SHADCN" && npm ci --no-audit --no-fund > /dev/null)
  (cd "$SHADCN" && node node_modules/next/dist/bin/next build next > /dev/null && NEXT_STATIC=1 node node_modules/next/dist/bin/next build next > /dev/null)
fi

python3 - "$OUT/env.json" "$SERVER_CPUS" "$CLIENT_CPUS" "$NEXT_WORKERS" <<'PY'
import json, os, platform, subprocess, sys
sh = lambda c: subprocess.run(c, shell=True, capture_output=True, text=True).stdout.strip() or None
cpu = next((l.split(":", 1)[1].strip() for l in open("/proc/cpuinfo") if l.startswith("model name")), None)
json.dump(dict(cpu=cpu, cores=os.cpu_count(), memGb=round(os.sysconf("SC_PAGE_SIZE") * os.sysconf("SC_PHYS_PAGES") / 2**30, 1),
    kernel=platform.release(), dotnet=sh("dotnet --version"), node=sh("node --version"), chromium=sh("${CHROMIUM:-/usr/bin/chromium} --version"),
    next=sh("node -p \"require('./tools/shadcn/node_modules/next/package.json').version\""), git=sh("git rev-parse --short HEAD"),
    aioros=sh("sed -n 's/.*<AiorosVersion>\\([^<]*\\)<.*/\\1/p' Directory.Packages.props"),
    serverCpus=sys.argv[2], clientCpus=sys.argv[3], nextWorkers=int(sys.argv[4]), http="HTTP/1.1 over loopback, no TLS"), open(sys.argv[1], "w"), indent=1)
PY

tree_rss_mb() {
  local total=0 stack=("$1")
  while ((${#stack[@]})); do
    local p=${stack[-1]}; unset 'stack[-1]'
    [[ -r /proc/$p/status ]] || continue
    total=$((total + $(awk '/VmRSS/{print $2}' "/proc/$p/status" 2>/dev/null || echo 0)))
    for c in $(cat /proc/"$p"/task/*/children 2>/dev/null); do stack+=("$c"); done
  done
  echo $((total / 1024))
}
kill_tree() { for c in $(cat /proc/"$1"/task/*/children 2>/dev/null); do kill_tree "$c"; done; kill "$1" 2>/dev/null || true; }

variant() { # name -> "port dir stack pages command..."
  case $1 in
    aioros)        echo "5331 $PUBLISH aioros AIOROS dotnet Gallery.dll --urls http://127.0.0.1:5331" ;;
    aioros-base)   echo "5332 ${AIOROS_BASE_DIR:?the base publish to compare with} aioros AIOROS dotnet Gallery.dll --urls http://127.0.0.1:5332" ;;
    aioros-engine) echo "5333 $ENGINE aioros AIOROS dotnet Gallery.dll --urls http://127.0.0.1:5333" ;;
    nextjs)        echo "5334 $SHADCN/next nextjs NEXT env NEXT_STATIC=0 node cluster.mjs 5334 127.0.0.1" ;;
    nextjs-static) echo "5335 $SHADCN/next nextjs NEXT env NEXT_STATIC=1 node cluster.mjs 5335 127.0.0.1" ;;
    *) echo "unknown variant $1" >&2; exit 1 ;;
  esac
}

server=0
trap '[[ $server != 0 ]] && kill_tree $server; wait 2>/dev/null || true' EXIT

run() {
  local name=$1 iteration=$2 port dir stack kind cmd
  read -r port dir stack kind cmd <<< "$(variant "$name")"
  local pages; [[ $kind == AIOROS ]] && pages=$AIOROS_PAGES || pages=$NEXT_PAGES
  local base=http://127.0.0.1:$port first=${pages%% *}; first=${first#*=}
  echo "== $name (run $iteration/$RUNS, load $(cut -d' ' -f1-3 /proc/loadavg))"
  local log=$ROOT/$OUT/$name-$iteration.log t0; t0=$(date +%s%N)
  (cd "$dir" && exec env Logging__LogLevel__Default=Warning Logging__LogLevel__Aioros.Hosting.AiorosHost.PlanWarmup=Information \
     NODE_ENV=production NEXT_WORKERS="$NEXT_WORKERS" taskset -c "$SERVER_CPUS" $cmd > "$log" 2>&1) &
  local pid=$!; server=$pid
  until curl -sf -o /dev/null "$base$first"; do
    kill -0 $pid 2>/dev/null || { echo "   $name did not start: $log" >&2; server=0; return 0; }
    sleep 0.02
  done
  local cold=$(( ($(date +%s%N) - t0) / 1000000 )) warm=null
  if [[ $stack == aioros ]]; then
    until grep -q "Warm-up finished" "$log"; do kill -0 $pid 2>/dev/null || break; sleep 0.2; done
    warm=$(( ($(date +%s%N) - t0) / 1000000 ))
  fi
  local entry key path
  for entry in $pages; do path=${entry#*=}; curl -s -o /dev/null "$base$path"; curl -s -o /dev/null "$base$path"; done
  local idle; idle=$(tree_rss_mb $pid)
  local measured=()
  for entry in $pages; do
    key=${entry%%=*}; path=${entry#*=}
    local status raw gz served enc c64
    status=$(curl -s -o /dev/null -w '%{http_code}' "$base$path")
    raw=$(curl -s "$base$path" | wc -c); gz=$(curl -s "$base$path" | gzip -c | wc -c)
    enc=$(curl -s -o /dev/null -D - -H 'Accept-Encoding: br, gzip' "$base$path" | awk -F': ' 'tolower($1)=="content-encoding"{print $2}' | tr -d '\r')
    served=$(curl -s -H 'Accept-Encoding: br, gzip' "$base$path" | wc -c)
    c64=$(timeout $((SECS + 120)) $LOADGEN "$base$path" "$CONNECTIONS" "$SECS" $pid 2> "$ROOT/$OUT/$name-$iteration-$key.loadgen.err" | tail -1) || true
    [[ $c64 == \{* ]] || c64=null
    measured+=("$(python3 -c "import json,sys; print(json.dumps(dict(key=sys.argv[1], page=sys.argv[2], status=int(sys.argv[3]), html=int(sys.argv[4]), gzip=int(sys.argv[5]), encoding=sys.argv[6] or None, served=int(sys.argv[7]), c64=json.loads(sys.argv[8]))))" "$key" "$path" "$status" "$raw" "$gz" "$enc" "$served" "$c64")")
    echo "   $key $path: $status $(echo "$c64" | python3 -c 'import json,sys; d=json.load(sys.stdin); print("load failed" if d is None else "{rps} req/s, p50 {p50} ms, p99 {p99} ms, {peakRssMb} MB, errors {errors}".format(**d))')"
  done
  local browser=null
  if ((iteration == 1)); then
    local dashboard='' paths=()
    for entry in $pages; do paths+=("${entry#*=}"); [[ ${entry%%=*} == dashboard-01 ]] && dashboard=${entry#*=}; done
    browser=$(taskset -c "$CLIENT_CPUS" node bench/browser.mjs "$base" "$stack" "$BROWSER_RUNS" "$dashboard" "${paths[@]}" || echo null)
    echo "   browser: $(echo "$browser" | python3 -c "import json,sys; d=json.load(sys.stdin) or {}; print(json.dumps(d.get('interactions')))")"
  fi
  local after; after=$(tree_rss_mb $pid)
  kill_tree $pid; wait $pid 2>/dev/null || true; server=0
  python3 -c "import json,sys; print(json.dumps(dict(name=sys.argv[1], iteration=int(sys.argv[2]), coldStartMs=int(sys.argv[3]), warmupMs=json.loads(sys.argv[4]), idleRssMb=int(sys.argv[5]), afterRssMb=int(sys.argv[6]), browser=json.loads(sys.argv[7]), pages=[json.loads(x) for x in sys.argv[8:]])))" \
    "$name" "$iteration" "$cold" "$warm" "$idle" "$after" "$browser" "${measured[@]}" >> "$OUT/gallery.jsonl"
}

read -ra list <<< "$VARIANTS"
for ((i = 1; i <= RUNS; i++)); do
  for ((k = 0; k < ${#list[@]}; k++)); do
    name=${list[$(( (k + i - 1) % ${#list[@]} ))]}
    [[ $name == aioros-engine && $i -gt 1 ]] && continue
    run "$name" $i
  done
done

find "$OUT" -name "*.loadgen.err" -empty -delete
python3 bench/report.py "$OUT" > "$OUT/GALLERY.md"
cp "$OUT/gallery.jsonl" bench/results/gallery.jsonl; cp "$OUT/GALLERY.md" bench/results/GALLERY.md
cat "$OUT/GALLERY.md"
