import type { ReactNode } from "react"
import { IconCloud, IconFolderCode } from "@tabler/icons-react"
import { AlertCircleIcon, ArrowUpRightIcon, BadgeCheckIcon, CheckCircle2Icon, ChevronRightIcon, PlusIcon, PopcornIcon } from "lucide-react"

import { Alert, AlertDescription, AlertTitle } from "@/registry/new-york-v4/ui/alert"
import { AspectRatio } from "@/registry/new-york-v4/ui/aspect-ratio"
import { Avatar, AvatarBadge, AvatarFallback, AvatarGroup, AvatarGroupCount, AvatarImage } from "@/registry/new-york-v4/ui/avatar"
import { Badge } from "@/registry/new-york-v4/ui/badge"
import { Button } from "@/registry/new-york-v4/ui/button"
import { Empty, EmptyContent, EmptyDescription, EmptyHeader, EmptyMedia, EmptyTitle } from "@/registry/new-york-v4/ui/empty"
import { Item, ItemActions, ItemContent, ItemDescription, ItemFooter, ItemHeader, ItemMedia, ItemTitle } from "@/registry/new-york-v4/ui/item"
import { Kbd, KbdGroup } from "@/registry/new-york-v4/ui/kbd"
import { Separator } from "@/registry/new-york-v4/ui/separator"
import { Skeleton } from "@/registry/new-york-v4/ui/skeleton"
import { Spinner } from "@/registry/new-york-v4/ui/spinner"

import shadcn from "../../../../samples/Gallery/Gallery/Assets/avatar-shadcn.png"
import evilrabbit from "../../../../samples/Gallery/Gallery/Assets/avatar-evilrabbit.png"
import maxleiter from "../../../../samples/Gallery/Gallery/Assets/avatar-maxleiter.png"
import landscape from "../../../../samples/Gallery/Gallery/Assets/photo-landscape.png"

export const displayFixtures: Record<string, () => ReactNode> = {
  "separator-demo": () => (
    <div>
      <div className="space-y-1">
        <h4 data-cosmo="h4" className="text-sm leading-none font-medium">Radix Primitives</h4>
        <p className="text-sm text-muted-foreground">An open-source UI component library.</p>
      </div>
      <Separator className="my-4" />
      <div className="flex h-5 items-center space-x-4 text-sm">
        <div>Blog</div>
        <Separator orientation="vertical" />
        <div>Docs</div>
        <Separator orientation="vertical" />
        <div>Source</div>
      </div>
    </div>
  ),
  "skeleton-demo": () => (
    <div className="flex flex-col gap-8">
      <div className="flex items-center space-x-4">
        <Skeleton className="h-12 w-12 rounded-full" />
        <div className="space-y-2">
          <Skeleton className="h-4 w-[250px]" />
          <Skeleton className="h-4 w-[200px]" />
        </div>
      </div>
      <div className="flex flex-col space-y-3">
        <Skeleton className="h-[125px] w-[250px] rounded-xl" />
        <div className="space-y-2">
          <Skeleton className="h-4 w-[250px]" />
          <Skeleton className="h-4 w-[200px]" />
        </div>
      </div>
    </div>
  ),
  "spinner-demo": () => (
    <div className="flex flex-col gap-6">
      <div className="flex items-center gap-6">
        <Spinner data-cosmo="spinner" className="size-3" />
        <Spinner data-cosmo="spinner" className="size-4" />
        <Spinner data-cosmo="spinner" className="size-6" />
        <Spinner data-cosmo="spinner" className="size-8" />
      </div>
      <div className="flex items-center gap-4">
        <Button disabled size="sm">
          <Spinner />
          Loading...
        </Button>
        <Button variant="outline" disabled size="sm">
          <Spinner />
          Please wait
        </Button>
      </div>
      <div className="flex items-center gap-4">
        <Badge>
          <Spinner />
          Syncing
        </Badge>
        <Badge variant="secondary">
          <Spinner />
          Updating
        </Badge>
        <Badge variant="outline">
          <Spinner />
          Processing
        </Badge>
      </div>
    </div>
  ),
  "kbd-demo": () => (
    <div className="flex flex-col items-start gap-4">
      <KbdGroup>
        <Kbd>⌘</Kbd>
        <Kbd>⇧</Kbd>
        <Kbd>⌥</Kbd>
        <Kbd>⌃</Kbd>
      </KbdGroup>
      <KbdGroup>
        <Kbd>Ctrl</Kbd>
        <span>+</span>
        <Kbd>B</Kbd>
      </KbdGroup>
      <div className="flex items-center gap-1 text-sm text-muted-foreground">
        <span>Use</span>
        <KbdGroup>
          <Kbd>Ctrl + B</Kbd>
          <Kbd>Ctrl + K</Kbd>
        </KbdGroup>
        <span>to open the command palette</span>
      </div>
      <div className="flex flex-wrap items-center gap-4">
        <Button variant="outline" size="sm" className="pr-2">
          Accept <Kbd>⏎</Kbd>
        </Button>
        <Button variant="outline" size="sm" className="pr-2">
          Cancel <Kbd>Esc</Kbd>
        </Button>
      </div>
    </div>
  ),
  "avatar-demo": () => (
    <div className="flex flex-col gap-8">
      <div className="flex flex-row flex-wrap items-center gap-12">
        <Avatar>
          <AvatarImage src={shadcn} alt="@shadcn" />
          <AvatarFallback>CN</AvatarFallback>
        </Avatar>
        <Avatar className="rounded-lg">
          <AvatarImage src={evilrabbit} alt="@evilrabbit" />
          <AvatarFallback>ER</AvatarFallback>
        </Avatar>
        <div className="flex -space-x-2 *:data-[slot=avatar]:ring-2 *:data-[slot=avatar]:ring-background">
          <Avatar>
            <AvatarImage src={shadcn} alt="@shadcn" />
            <AvatarFallback>CN</AvatarFallback>
          </Avatar>
          <Avatar>
            <AvatarImage src={maxleiter} alt="@maxleiter" />
            <AvatarFallback>LR</AvatarFallback>
          </Avatar>
          <Avatar>
            <AvatarImage src={evilrabbit} alt="@evilrabbit" />
            <AvatarFallback>ER</AvatarFallback>
          </Avatar>
        </div>
      </div>
      <div className="flex flex-row flex-wrap items-center gap-12">
        <Avatar size="sm">
          <AvatarFallback>CN</AvatarFallback>
        </Avatar>
        <Avatar>
          <AvatarFallback>CN</AvatarFallback>
        </Avatar>
        <Avatar size="lg">
          <AvatarFallback>CN</AvatarFallback>
        </Avatar>
      </div>
    </div>
  ),
  "avatar-badge": () => (
    <div className="flex flex-row flex-wrap items-center gap-12">
      <Avatar size="sm">
        <AvatarImage src={shadcn} alt="@shadcn" />
        <AvatarFallback>CN</AvatarFallback>
        <AvatarBadge />
      </Avatar>
      <Avatar>
        <AvatarImage src={shadcn} alt="@shadcn" />
        <AvatarFallback>CN</AvatarFallback>
        <AvatarBadge />
      </Avatar>
      <Avatar size="lg">
        <AvatarImage src={shadcn} alt="@shadcn" />
        <AvatarFallback>CN</AvatarFallback>
        <AvatarBadge />
      </Avatar>
      <Avatar>
        <AvatarFallback>ER</AvatarFallback>
        <AvatarBadge>
          <PlusIcon />
        </AvatarBadge>
      </Avatar>
      <Avatar size="lg">
        <AvatarFallback>ER</AvatarFallback>
        <AvatarBadge>
          <PlusIcon />
        </AvatarBadge>
      </Avatar>
    </div>
  ),
  "avatar-group-count": () => (
    <div className="flex flex-col gap-8">
      <AvatarGroup>
        <Avatar>
          <AvatarImage src={shadcn} alt="@shadcn" />
          <AvatarFallback>CN</AvatarFallback>
        </Avatar>
        <Avatar>
          <AvatarImage src={maxleiter} alt="@maxleiter" />
          <AvatarFallback>LR</AvatarFallback>
        </Avatar>
        <Avatar>
          <AvatarImage src={evilrabbit} alt="@evilrabbit" />
          <AvatarFallback>ER</AvatarFallback>
        </Avatar>
        <AvatarGroupCount>+3</AvatarGroupCount>
      </AvatarGroup>
      <AvatarGroup>
        <Avatar size="sm">
          <AvatarFallback>CN</AvatarFallback>
        </Avatar>
        <Avatar size="sm">
          <AvatarFallback>LR</AvatarFallback>
        </Avatar>
        <AvatarGroupCount>+2</AvatarGroupCount>
      </AvatarGroup>
      <AvatarGroup>
        <Avatar size="lg">
          <AvatarFallback>CN</AvatarFallback>
        </Avatar>
        <Avatar size="lg">
          <AvatarFallback>LR</AvatarFallback>
        </Avatar>
        <AvatarGroupCount>
          <PlusIcon />
        </AvatarGroupCount>
      </AvatarGroup>
    </div>
  ),
  "alert-demo": () => (
    <div className="grid w-full max-w-xl items-start gap-4">
      <Alert>
        <CheckCircle2Icon />
        <AlertTitle>Success! Your changes have been saved</AlertTitle>
        <AlertDescription>This is an alert with icon, title and description.</AlertDescription>
      </Alert>
      <Alert>
        <PopcornIcon />
        <AlertTitle>This Alert has a title and an icon. No description.</AlertTitle>
      </Alert>
      <Alert variant="destructive">
        <AlertCircleIcon />
        <AlertTitle>Unable to process your payment.</AlertTitle>
        <AlertDescription>
          <p>Please verify your billing information and try again.</p>
          <ul data-cosmo="ul" className="list-inside list-disc text-sm">
            <li>Check your card details</li>
            <li>Ensure sufficient funds</li>
            <li>Verify billing address</li>
          </ul>
        </AlertDescription>
      </Alert>
    </div>
  ),
  "aspect-ratio-demo": () => (
    <div className="w-[450px]">
      <AspectRatio ratio={16 / 9} className="rounded-lg bg-muted">
        <img src={landscape} alt="A landscape at dusk" className="h-full w-full rounded-lg object-cover" />
      </AspectRatio>
    </div>
  ),
  "typography-headings": () => (
    <div>
      <h1 data-cosmo="h1" className="scroll-m-20 text-4xl font-extrabold tracking-tight text-balance">Taxing Laughter: The Joke Tax Chronicles</h1>
      <h2 data-cosmo="h2" className="mt-10 scroll-m-20 border-b pb-2 text-3xl font-semibold tracking-tight first:mt-0">The People of the Kingdom</h2>
      <h3 data-cosmo="h3" className="mt-8 scroll-m-20 text-2xl font-semibold tracking-tight">The Joke Tax</h3>
      <h4 className="mt-6 scroll-m-20 text-xl font-semibold tracking-tight">People stopped telling jokes</h4>
    </div>
  ),
  "typography-text": () => (
    <div>
      <p data-cosmo="lead" className="text-xl text-muted-foreground">A modal dialog that interrupts the user with important content and expects a response.</p>
      <p data-cosmo="p" className="leading-7 [&:not(:first-child)]:mt-6">
        The king, seeing how much happier his subjects were, realized the error of his ways and repealed the joke tax.
      </p>
      <blockquote data-cosmo="blockquote" className="mt-6 border-l-2 pl-6 italic">
        &quot;After all,&quot; he said, &quot;everyone enjoys a good joke, so it&apos;s only fair that they should pay for the privilege.&quot;
      </blockquote>
      <ul className="my-6 ml-6 list-disc [&>li]:mt-2">
        <li>1st level of puns: 5 gold coins</li>
        <li>2nd level of jokes: 10 gold coins</li>
        <li>3rd level of one-liners : 20 gold coins</li>
      </ul>
      <code data-cosmo="code" className="relative rounded bg-muted px-[0.3rem] py-[0.2rem] font-mono text-sm font-semibold">@radix-ui/react-alert-dialog</code>
      <div data-cosmo="large" className="mt-6 text-lg font-semibold">Are you absolutely sure?</div>
      <small data-cosmo="small" className="mt-4 block text-sm leading-none font-medium">Email address</small>
      <p data-cosmo="muted" className="mt-4 text-sm text-muted-foreground">Enter your email address.</p>
    </div>
  ),
  "empty-demo": () => (
    <Empty>
      <EmptyHeader>
        <EmptyMedia variant="icon">
          <IconFolderCode />
        </EmptyMedia>
        <EmptyTitle>No Projects Yet</EmptyTitle>
        <EmptyDescription>You haven&apos;t created any projects yet. Get started by creating your first project.</EmptyDescription>
      </EmptyHeader>
      <EmptyContent>
        <div className="flex gap-2">
          <Button>Create Project</Button>
          <Button variant="outline">Import Project</Button>
        </div>
      </EmptyContent>
      <Button variant="link" asChild className="text-muted-foreground" size="sm">
        <a href="#">
          Learn More <ArrowUpRightIcon />
        </a>
      </Button>
    </Empty>
  ),
  "empty-outline": () => (
    <Empty className="border border-dashed">
      <EmptyHeader>
        <EmptyMedia variant="icon">
          <IconCloud />
        </EmptyMedia>
        <EmptyTitle>Cloud Storage Empty</EmptyTitle>
        <EmptyDescription>Upload files to your cloud storage to access them anywhere.</EmptyDescription>
      </EmptyHeader>
      <EmptyContent>
        <Button variant="outline" size="sm">
          Upload Files
        </Button>
      </EmptyContent>
    </Empty>
  ),
  "item-header-footer": () => (
    <div className="flex w-full max-w-md flex-col gap-6">
      <Item variant="outline">
        <ItemHeader>
          <span className="text-sm font-medium">Invoice #1042</span>
          <Badge variant="secondary">Paid</Badge>
        </ItemHeader>
        <ItemContent>
          <ItemTitle>Acme Inc.</ItemTitle>
          <ItemDescription>Design system subscription, October.</ItemDescription>
        </ItemContent>
        <ItemActions>
          <Button variant="outline" size="sm">View</Button>
        </ItemActions>
        <ItemFooter>
          <span className="text-muted-foreground text-sm">Due Oct 31</span>
          <span className="text-sm font-medium">$249.00</span>
        </ItemFooter>
      </Item>
    </div>
  ),
  "item-demo": () => (
    <div className="flex w-full max-w-md flex-col gap-6">
      <Item variant="outline">
        <ItemContent>
          <ItemTitle>Basic Item</ItemTitle>
          <ItemDescription>A simple item with title and description.</ItemDescription>
        </ItemContent>
        <ItemActions>
          <Button variant="outline" size="sm">
            Action
          </Button>
        </ItemActions>
      </Item>
      <Item variant="outline" size="sm" asChild>
        <a href="#">
          <ItemMedia>
            <BadgeCheckIcon className="size-5" />
          </ItemMedia>
          <ItemContent>
            <ItemTitle>Your profile has been verified.</ItemTitle>
          </ItemContent>
          <ItemActions>
            <ChevronRightIcon className="size-4" />
          </ItemActions>
        </a>
      </Item>
    </div>
  ),
  "item-variant": () => (
    <div className="flex flex-col gap-6">
      <Item>
        <ItemContent>
          <ItemTitle>Default Variant</ItemTitle>
          <ItemDescription>Standard styling with subtle background and borders.</ItemDescription>
        </ItemContent>
        <ItemActions>
          <Button variant="outline" size="sm">
            Open
          </Button>
        </ItemActions>
      </Item>
      <Item variant="outline">
        <ItemContent>
          <ItemTitle>Outline Variant</ItemTitle>
          <ItemDescription>Outlined style with clear borders and transparent background.</ItemDescription>
        </ItemContent>
        <ItemActions>
          <Button variant="outline" size="sm">
            Open
          </Button>
        </ItemActions>
      </Item>
      <Item variant="muted">
        <ItemContent>
          <ItemTitle>Muted Variant</ItemTitle>
          <ItemDescription>Subdued appearance with muted colors for secondary content.</ItemDescription>
        </ItemContent>
        <ItemActions>
          <Button variant="outline" size="sm">
            Open
          </Button>
        </ItemActions>
      </Item>
    </div>
  ),
}
