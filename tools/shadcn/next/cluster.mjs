// `next start` on every server core for Cosmo's lab bench (bench/gallery.sh), as Aioros's bench/next-lumen/cluster.mjs
// does: one Next.js production server per worker of Node's cluster module, which shares the listening port between them.
// usage: NEXT_WORKERS=4 node cluster.mjs <port> <host>   (NEXT_STATIC=1 serves the prerendered build, see next.config.mjs)
import cluster from "node:cluster"
import http from "node:http"
import next from "next"

const port = +process.argv[2], hostname = process.argv[3] ?? "127.0.0.1"
const workers = +(process.env.NEXT_WORKERS ?? 1)

if (cluster.isPrimary && workers > 1) {
  for (let i = 0; i < workers; i++) cluster.fork()
} else {
  const app = next({ dev: false, hostname, port, dir: import.meta.dirname })
  await app.prepare()
  const handle = app.getRequestHandler()
  http.createServer((req, res) => handle(req, res)).listen(port, hostname)
}
