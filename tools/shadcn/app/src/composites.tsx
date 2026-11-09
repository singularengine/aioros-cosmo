import type { ReactNode } from "react"
import { BookOpen, Bot, ChevronRight, ChevronsUpDown, Frame, GalleryVerticalEnd, Map, PieChart, Settings2, SquareTerminal } from "lucide-react"

import CarouselApi from "@/registry/new-york-v4/examples/carousel-api"
import CarouselDemo from "@/registry/new-york-v4/examples/carousel-demo"
import CarouselOrientation from "@/registry/new-york-v4/examples/carousel-orientation"
import ComboboxDemo from "@/registry/new-york-v4/examples/combobox-demo"
import CommandDemo from "@/registry/new-york-v4/examples/command-demo"
import CommandDialogDemo from "@/registry/new-york-v4/examples/command-dialog"
import DataTableDemo from "@/registry/new-york-v4/examples/data-table-demo"
import ResizableDemo from "@/registry/new-york-v4/examples/resizable-demo"
import ResizableHandleDemo from "@/registry/new-york-v4/examples/resizable-handle"
import ResizableVertical from "@/registry/new-york-v4/examples/resizable-vertical"
import { Avatar, AvatarFallback } from "@/registry/new-york-v4/ui/avatar"
import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from "@/registry/new-york-v4/ui/breadcrumb"
import { Separator } from "@/registry/new-york-v4/ui/separator"
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarInset,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarMenuSub,
  SidebarMenuSubButton,
  SidebarMenuSubItem,
  SidebarProvider,
  SidebarRail,
  SidebarTrigger,
} from "@/registry/new-york-v4/ui/sidebar"

const platform = [
  { title: "Playground", icon: SquareTerminal, open: true, items: ["History", "Starred", "Settings"] },
  { title: "Models", icon: Bot },
  { title: "Documentation", icon: BookOpen },
  { title: "Settings", icon: Settings2 },
]

const projects = [
  { name: "Design Engineering", icon: Frame },
  { name: "Sales & Marketing", icon: PieChart },
  { name: "Travel", icon: Map },
]

function SidebarFixture({ open = true, variant = "sidebar" }: { open?: boolean; variant?: "sidebar" | "floating" | "inset" }) {
  return (
    <div className="relative h-[600px] overflow-hidden" style={{ transform: "translateZ(0)" }}>
      <SidebarProvider defaultOpen={open} className="h-full min-h-0">
        <Sidebar collapsible="icon" variant={variant} className="h-full">
          <SidebarHeader>
            <SidebarMenu>
              <SidebarMenuItem>
                <SidebarMenuButton size="lg">
                  <div className="flex aspect-square size-8 items-center justify-center rounded-lg bg-sidebar-primary text-sidebar-primary-foreground">
                    <GalleryVerticalEnd className="size-4" />
                  </div>
                  <div className="grid flex-1 text-left text-sm leading-tight">
                    <span className="truncate font-medium">Acme Inc</span>
                    <span className="truncate text-xs">Enterprise</span>
                  </div>
                  <ChevronsUpDown className="ml-auto" />
                </SidebarMenuButton>
              </SidebarMenuItem>
            </SidebarMenu>
          </SidebarHeader>
          <SidebarContent>
            <SidebarGroup>
              <SidebarGroupLabel>Platform</SidebarGroupLabel>
              <SidebarMenu>
                {platform.map((item) => (
                  <SidebarMenuItem key={item.title}>
                    <SidebarMenuButton tooltip={item.title} isActive={item.open}>
                      <item.icon />
                      <span>{item.title}</span>
                      <ChevronRight className={"ml-auto" + (item.open ? " rotate-90" : "")} />
                    </SidebarMenuButton>
                    {item.items && (
                      <SidebarMenuSub>
                        {item.items.map((sub) => (
                          <SidebarMenuSubItem key={sub}>
                            <SidebarMenuSubButton href="#">
                              <span>{sub}</span>
                            </SidebarMenuSubButton>
                          </SidebarMenuSubItem>
                        ))}
                      </SidebarMenuSub>
                    )}
                  </SidebarMenuItem>
                ))}
              </SidebarMenu>
            </SidebarGroup>
            <SidebarGroup className="group-data-[collapsible=icon]:hidden">
              <SidebarGroupLabel>Projects</SidebarGroupLabel>
              <SidebarMenu>
                {projects.map((item) => (
                  <SidebarMenuItem key={item.name}>
                    <SidebarMenuButton>
                      <item.icon />
                      <span>{item.name}</span>
                    </SidebarMenuButton>
                  </SidebarMenuItem>
                ))}
              </SidebarMenu>
            </SidebarGroup>
          </SidebarContent>
          <SidebarFooter>
            <SidebarMenu>
              <SidebarMenuItem>
                <SidebarMenuButton size="lg">
                  <Avatar className="h-8 w-8 rounded-lg">
                    <AvatarFallback className="rounded-lg">CN</AvatarFallback>
                  </Avatar>
                  <div className="grid flex-1 text-left text-sm leading-tight">
                    <span className="truncate font-medium">shadcn</span>
                    <span className="truncate text-xs">m@example.com</span>
                  </div>
                  <ChevronsUpDown className="ml-auto size-4" />
                </SidebarMenuButton>
              </SidebarMenuItem>
            </SidebarMenu>
          </SidebarFooter>
          <SidebarRail />
        </Sidebar>
        <SidebarInset>
          <header className="flex h-16 shrink-0 items-center gap-2">
            <div className="flex items-center gap-2 px-4">
              <SidebarTrigger className="-ml-1" />
              <Separator orientation="vertical" className="mr-2 data-[orientation=vertical]:h-4" />
              <Breadcrumb>
                <BreadcrumbList>
                  <BreadcrumbItem>
                    <BreadcrumbLink href="#">Platform</BreadcrumbLink>
                  </BreadcrumbItem>
                  <BreadcrumbSeparator />
                  <BreadcrumbItem>
                    <BreadcrumbPage>Playground</BreadcrumbPage>
                  </BreadcrumbItem>
                </BreadcrumbList>
              </Breadcrumb>
            </div>
          </header>
          <div className="flex flex-1 flex-col gap-4 p-4 pt-0">
            <div className="grid auto-rows-min gap-4 md:grid-cols-3">
              <div data-cosmo="tile" className="aspect-video rounded-xl bg-muted/50" />
              <div data-cosmo="tile" className="aspect-video rounded-xl bg-muted/50" />
              <div data-cosmo="tile" className="aspect-video rounded-xl bg-muted/50" />
            </div>
            <div data-cosmo="panel" className="flex-1 rounded-xl bg-muted/50" />
          </div>
        </SidebarInset>
      </SidebarProvider>
    </div>
  )
}

export const compositeFixtures: Record<string, () => ReactNode> = {
  "sidebar-demo": () => <SidebarFixture />,
  "sidebar-icon": () => <SidebarFixture open={false} />,
  "sidebar-floating": () => <SidebarFixture variant="floating" />,
  "sidebar-inset": () => <SidebarFixture variant="inset" />,
  "command-demo": () => <CommandDemo />,
  "command-filtered": () => <CommandDemo />,
  "command-dialog-open": () => (
    <div className="h-[432px]">
      <CommandDialogDemo />
    </div>
  ),
  "combobox-demo": () => <ComboboxDemo />,
  "combobox-open": () => (
    <div className="h-[300px]">
      <ComboboxDemo />
    </div>
  ),
  "data-table-demo": () => <DataTableDemo />,
  "data-table-selected": () => <DataTableDemo />,
  "resizable-demo": () => <ResizableDemo />,
  "resizable-handle": () => <ResizableHandleDemo />,
  "resizable-vertical": () => <ResizableVertical />,
  "carousel-demo": () => (
    <div className="flex justify-center px-12">
      <CarouselDemo />
    </div>
  ),
  "carousel-api": () => (
    <div className="px-12">
      <CarouselApi />
    </div>
  ),
  "carousel-orientation": () => (
    <div className="flex justify-center py-12">
      <CarouselOrientation />
    </div>
  ),
}
