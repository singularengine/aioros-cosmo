import type { ReactNode } from "react"

import Login01 from "@/registry/new-york-v4/blocks/login-01/page"
import Login02 from "@/registry/new-york-v4/blocks/login-02/page"
import Login03 from "@/registry/new-york-v4/blocks/login-03/page"
import Login04 from "@/registry/new-york-v4/blocks/login-04/page"
import Login05 from "@/registry/new-york-v4/blocks/login-05/page"
import Signup01 from "@/registry/new-york-v4/blocks/signup-01/page"
import Signup02 from "@/registry/new-york-v4/blocks/signup-02/page"
import Signup03 from "@/registry/new-york-v4/blocks/signup-03/page"
import Signup04 from "@/registry/new-york-v4/blocks/signup-04/page"
import Signup05 from "@/registry/new-york-v4/blocks/signup-05/page"
import Dashboard01 from "@/registry/new-york-v4/blocks/dashboard-01/page"
import Sidebar01 from "@/registry/new-york-v4/blocks/sidebar-01/page"
import Sidebar02 from "@/registry/new-york-v4/blocks/sidebar-02/page"
import Sidebar03 from "@/registry/new-york-v4/blocks/sidebar-03/page"
import Sidebar04 from "@/registry/new-york-v4/blocks/sidebar-04/page"
import Sidebar05 from "@/registry/new-york-v4/blocks/sidebar-05/page"
import Sidebar06 from "@/registry/new-york-v4/blocks/sidebar-06/page"
import Sidebar07 from "@/registry/new-york-v4/blocks/sidebar-07/page"
import Sidebar08 from "@/registry/new-york-v4/blocks/sidebar-08/page"
import Sidebar09 from "@/registry/new-york-v4/blocks/sidebar-09/page"
import Sidebar10 from "@/registry/new-york-v4/blocks/sidebar-10/page"
import Sidebar11 from "@/registry/new-york-v4/blocks/sidebar-11/page"
import Sidebar12 from "@/registry/new-york-v4/blocks/sidebar-12/page"
import Sidebar13 from "@/registry/new-york-v4/blocks/sidebar-13/page"
import Sidebar14 from "@/registry/new-york-v4/blocks/sidebar-14/page"
import Sidebar15 from "@/registry/new-york-v4/blocks/sidebar-15/page"
import Sidebar16 from "@/registry/new-york-v4/blocks/sidebar-16/page"

/** A block's page as it is, at its content's height instead of the viewport's (`min-h-svh` released), in the 640 px fixture. */
function Block({ children }: { children: ReactNode }) {
  return <div className="[&_.min-h-svh]:min-h-0">{children}</div>
}

/** shadcn/ui's login and signup blocks (new-york-v4), their pages unchanged; `/placeholder.svg` is the app's own placeholder. */
export const blockFixtures: Record<string, () => ReactNode> = {
  "login-01": () => <Block><Login01 /></Block>,
  "login-02": () => <Block><Login02 /></Block>,
  "login-03": () => <Block><Login03 /></Block>,
  "login-04": () => <Block><Login04 /></Block>,
  "login-05": () => <Block><Login05 /></Block>,
  "signup-01": () => <Block><Signup01 /></Block>,
  "signup-02": () => <Block><Signup02 /></Block>,
  "signup-03": () => <Block><Signup03 /></Block>,
  "signup-04": () => <Block><Signup04 /></Block>,
  "signup-05": () => <Block><Signup05 /></Block>,
}

/** The app shell blocks' viewport: shadcn/ui's sidebar and dashboard pages fill it (`h-svh`, `min-h-svh`), captured at 1280 × 800. */
export const shellViewport = { width: 1280, height: 800 }

/** An app shell block's page as it is, in a 1280 × 800 frame that contains its fixed sidebar (a transform makes the frame the sidebar's containing block). */
function Shell({ children }: { children: ReactNode }) {
  return (
    <div className="relative h-[800px] overflow-hidden" style={{ transform: "translateZ(0)" }}>
      {children}
    </div>
  )
}

/** shadcn/ui's app shell blocks (new-york-v4 dashboard-01, sidebar-01 to sidebar-16), their pages unchanged. */
export const shellFixtures: Record<string, () => ReactNode> = {
  "dashboard-01": () => <Shell><Dashboard01 /></Shell>,
  "sidebar-01": () => <Shell><Sidebar01 /></Shell>,
  "sidebar-02": () => <Shell><Sidebar02 /></Shell>,
  "sidebar-03": () => <Shell><Sidebar03 /></Shell>,
  "sidebar-04": () => <Shell><Sidebar04 /></Shell>,
  "sidebar-05": () => <Shell><Sidebar05 /></Shell>,
  "sidebar-06": () => <Shell><Sidebar06 /></Shell>,
  "sidebar-07": () => <Shell><Sidebar07 /></Shell>,
  "sidebar-08": () => <Shell><Sidebar08 /></Shell>,
  "sidebar-09": () => <Shell><Sidebar09 /></Shell>,
  "sidebar-10": () => <Shell><Sidebar10 /></Shell>,
  "sidebar-11": () => <Shell><Sidebar11 /></Shell>,
  "sidebar-12": () => <Shell><Sidebar12 /></Shell>,
  "sidebar-13": () => <Shell><Sidebar13 /></Shell>,
  "sidebar-14": () => <Shell><Sidebar14 /></Shell>,
  "sidebar-15": () => <Shell><Sidebar15 /></Shell>,
  "sidebar-16": () => <Shell><Sidebar16 /></Shell>,
}
