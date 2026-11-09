import { Fragment, type ReactNode } from "react"

import {
  Menubar,
  MenubarCheckboxItem,
  MenubarContent,
  MenubarItem,
  MenubarMenu,
  MenubarRadioGroup,
  MenubarRadioItem,
  MenubarSeparator,
  MenubarShortcut,
  MenubarSub,
  MenubarSubContent,
  MenubarSubTrigger,
  MenubarTrigger,
} from "@/registry/new-york-v4/ui/menubar"
import {
  NavigationMenu,
  NavigationMenuContent,
  NavigationMenuItem,
  NavigationMenuLink,
  NavigationMenuList,
  NavigationMenuTrigger,
  navigationMenuTriggerStyle,
} from "@/registry/new-york-v4/ui/navigation-menu"
import { ScrollArea, ScrollBar } from "@/registry/new-york-v4/ui/scroll-area"
import { Separator } from "@/registry/new-york-v4/ui/separator"
import { Table, TableBody, TableCaption, TableCell, TableFooter, TableHead, TableHeader, TableRow } from "@/registry/new-york-v4/ui/table"
import binni from "../../../../samples/Gallery/Gallery/Assets/art-binni.png"
import byrom from "../../../../samples/Gallery/Gallery/Assets/art-byrom.png"
import malyavko from "../../../../samples/Gallery/Gallery/Assets/art-malyavko.png"

const tags = Array.from({ length: 50 }).map((_, i, a) => `v1.2.0-beta.${a.length - i}`)

const works = [
  { artist: "Ornella Binni", art: binni },
  { artist: "Tom Byrom", art: byrom },
  { artist: "Vladimir Malyavko", art: malyavko },
]

const invoices = [
  ["INV001", "Paid", "$250.00", "Credit Card"],
  ["INV002", "Pending", "$150.00", "PayPal"],
  ["INV003", "Unpaid", "$350.00", "Bank Transfer"],
  ["INV004", "Paid", "$450.00", "Credit Card"],
  ["INV005", "Paid", "$550.00", "PayPal"],
  ["INV006", "Pending", "$200.00", "Bank Transfer"],
  ["INV007", "Unpaid", "$300.00", "Credit Card"],
]

const components = [
  ["Alert Dialog", "A modal dialog that interrupts the user with important content and expects a response."],
  ["Hover Card", "For sighted users to preview content available behind a link."],
  ["Progress", "Displays an indicator showing the completion progress of a task, typically displayed as a progress bar."],
  ["Scroll-area", "Visually or semantically separates content."],
  ["Tabs", "A set of layered sections of content—known as tab panels—that are displayed one at a time."],
  ["Tooltip", "A popup that displays information related to an element when the element receives keyboard focus or the mouse hovers over it."],
]

function ListItem({ title, children }: { title: string; children: ReactNode }) {
  return (
    <li>
      <NavigationMenuLink asChild>
        <a href="#">
          <div className="text-sm leading-none font-medium">{title}</div>
          <p className="line-clamp-2 text-sm leading-snug text-muted-foreground">{children}</p>
        </a>
      </NavigationMenuLink>
    </li>
  )
}

function NavigationMenuFixture() {
  return (
    <div className="h-[340px]">
      <NavigationMenu viewport={false}>
        <NavigationMenuList className="flex-wrap">
          <NavigationMenuItem>
            <NavigationMenuTrigger>Home</NavigationMenuTrigger>
            <NavigationMenuContent>
              <ul className="grid gap-2 md:w-[400px] lg:w-[500px] lg:grid-cols-[.75fr_1fr]">
                <ListItem title="Introduction">Re-usable components built using Radix UI and Tailwind CSS.</ListItem>
                <ListItem title="Installation">How to install dependencies and structure your app.</ListItem>
                <ListItem title="Typography">Styles for headings, paragraphs, lists...etc</ListItem>
              </ul>
            </NavigationMenuContent>
          </NavigationMenuItem>
          <NavigationMenuItem>
            <NavigationMenuTrigger>Components</NavigationMenuTrigger>
            <NavigationMenuContent>
              <ul className="grid gap-2 sm:w-[400px] md:w-[500px] md:grid-cols-2 lg:w-[600px]">
                {components.map(([title, description]) => (
                  <ListItem key={title} title={title}>
                    {description}
                  </ListItem>
                ))}
              </ul>
            </NavigationMenuContent>
          </NavigationMenuItem>
          <NavigationMenuItem>
            <NavigationMenuLink asChild className={navigationMenuTriggerStyle()}>
              <a href="#">Docs</a>
            </NavigationMenuLink>
          </NavigationMenuItem>
          <NavigationMenuItem>
            <NavigationMenuTrigger>List</NavigationMenuTrigger>
            <NavigationMenuContent>
              <ul className="grid w-[300px] gap-4">
                <li>
                  {[
                    ["Components", "Browse all components in the library."],
                    ["Documentation", "Learn how to use the library."],
                    ["Blog", "Read our latest blog posts."],
                  ].map(([title, description]) => (
                    <NavigationMenuLink key={title} asChild>
                      <a href="#">
                        <div className="font-medium">{title}</div>
                        <div className="text-muted-foreground">{description}</div>
                      </a>
                    </NavigationMenuLink>
                  ))}
                </li>
              </ul>
            </NavigationMenuContent>
          </NavigationMenuItem>
        </NavigationMenuList>
      </NavigationMenu>
    </div>
  )
}

function MenubarFixture() {
  return (
    <div className="h-[300px]">
      <Menubar>
        <MenubarMenu>
          <MenubarTrigger>File</MenubarTrigger>
          <MenubarContent>
            <MenubarItem>
              New Tab <MenubarShortcut>⌘T</MenubarShortcut>
            </MenubarItem>
            <MenubarItem>
              New Window <MenubarShortcut>⌘N</MenubarShortcut>
            </MenubarItem>
            <MenubarItem disabled>New Incognito Window</MenubarItem>
            <MenubarSeparator />
            <MenubarSub>
              <MenubarSubTrigger>Share</MenubarSubTrigger>
              <MenubarSubContent>
                <MenubarItem>Email link</MenubarItem>
                <MenubarItem>Messages</MenubarItem>
                <MenubarItem>Notes</MenubarItem>
              </MenubarSubContent>
            </MenubarSub>
            <MenubarSeparator />
            <MenubarItem>
              Print... <MenubarShortcut>⌘P</MenubarShortcut>
            </MenubarItem>
          </MenubarContent>
        </MenubarMenu>
        <MenubarMenu>
          <MenubarTrigger>Edit</MenubarTrigger>
          <MenubarContent>
            <MenubarItem>
              Undo <MenubarShortcut>⌘Z</MenubarShortcut>
            </MenubarItem>
            <MenubarItem>
              Redo <MenubarShortcut>⇧⌘Z</MenubarShortcut>
            </MenubarItem>
            <MenubarSeparator />
            <MenubarItem>Cut</MenubarItem>
            <MenubarItem>Copy</MenubarItem>
            <MenubarItem>Paste</MenubarItem>
          </MenubarContent>
        </MenubarMenu>
        <MenubarMenu>
          <MenubarTrigger>View</MenubarTrigger>
          <MenubarContent>
            <MenubarCheckboxItem>Always Show Bookmarks Bar</MenubarCheckboxItem>
            <MenubarCheckboxItem checked>Always Show Full URLs</MenubarCheckboxItem>
            <MenubarSeparator />
            <MenubarItem inset>
              Reload <MenubarShortcut>⌘R</MenubarShortcut>
            </MenubarItem>
            <MenubarItem inset>Toggle Fullscreen</MenubarItem>
          </MenubarContent>
        </MenubarMenu>
        <MenubarMenu>
          <MenubarTrigger>Profiles</MenubarTrigger>
          <MenubarContent>
            <MenubarRadioGroup value="benoit">
              <MenubarRadioItem value="andy">Andy</MenubarRadioItem>
              <MenubarRadioItem value="benoit">Benoit</MenubarRadioItem>
              <MenubarRadioItem value="luis">Luis</MenubarRadioItem>
            </MenubarRadioGroup>
            <MenubarSeparator />
            <MenubarItem inset>Add Profile...</MenubarItem>
          </MenubarContent>
        </MenubarMenu>
      </Menubar>
    </div>
  )
}

export const structureFixtures: Record<string, () => ReactNode> = {
  "scroll-area-demo": () => (
    <ScrollArea className="h-72 w-48 rounded-md border">
      <div className="p-4">
        <h4 className="mb-4 text-sm leading-none font-medium">Tags</h4>
        {tags.map((tag) => (
          <Fragment key={tag}>
            <div className="text-sm">{tag}</div>
            <Separator className="my-2" />
          </Fragment>
        ))}
      </div>
    </ScrollArea>
  ),
  "scroll-area-hover": () => (
    <ScrollArea className="h-72 w-48 rounded-md border">
      <div className="p-4">
        <h4 className="mb-4 text-sm leading-none font-medium">Tags</h4>
        {tags.map((tag) => (
          <Fragment key={tag}>
            <div className="text-sm">{tag}</div>
            <Separator className="my-2" />
          </Fragment>
        ))}
      </div>
    </ScrollArea>
  ),
  "scroll-area-horizontal": () => (
    <ScrollArea className="w-96 rounded-md border whitespace-nowrap">
      <div className="flex w-max space-x-4 p-4">
        {works.map((work) => (
          <figure key={work.artist} className="shrink-0">
            <div className="overflow-hidden rounded-md">
              <img src={work.art} alt={`Photo by ${work.artist}`} className="aspect-[3/4] h-fit w-fit object-cover" width={300} height={400} />
            </div>
            <figcaption className="pt-2 text-xs text-muted-foreground">
              Photo by <span className="font-semibold text-foreground">{work.artist}</span>
            </figcaption>
          </figure>
        ))}
      </div>
      <ScrollBar orientation="horizontal" />
    </ScrollArea>
  ),
  "table-demo": () => (
    <Table>
      <TableCaption>A list of your recent invoices.</TableCaption>
      <TableHeader>
        <TableRow>
          <TableHead className="w-[100px]">Invoice</TableHead>
          <TableHead>Status</TableHead>
          <TableHead>Method</TableHead>
          <TableHead className="text-right">Amount</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {invoices.map(([invoice, status, amount, method]) => (
          <TableRow key={invoice}>
            <TableCell className="font-medium">{invoice}</TableCell>
            <TableCell>{status}</TableCell>
            <TableCell>{method}</TableCell>
            <TableCell className="text-right">{amount}</TableCell>
          </TableRow>
        ))}
      </TableBody>
      <TableFooter>
        <TableRow>
          <TableCell colSpan={3}>Total</TableCell>
          <TableCell className="text-right">$2,500.00</TableCell>
        </TableRow>
      </TableFooter>
    </Table>
  ),
  "navigation-menu-demo": () => <NavigationMenuFixture />,
  "navigation-menu-open": () => <NavigationMenuFixture />,
  "menubar-demo": () => <MenubarFixture />,
  "menubar-open": () => <MenubarFixture />,
}
