import type { ReactNode } from "react"
import { BellIcon, CheckIcon, ChevronRightIcon, HeartIcon, MailIcon, PlusIcon, SearchIcon, StarIcon } from "lucide-react"

import { Badge } from "@/registry/new-york-v4/ui/badge"
import { Button } from "@/registry/new-york-v4/ui/button"
import {
  Card,
  CardAction,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/registry/new-york-v4/ui/card"
import { Checkbox } from "@/registry/new-york-v4/ui/checkbox"
import {
  Field,
  FieldContent,
  FieldDescription,
  FieldError,
  FieldGroup,
  FieldLabel,
  FieldLegend,
  FieldSet,
  FieldTitle,
} from "@/registry/new-york-v4/ui/field"
import { Input } from "@/registry/new-york-v4/ui/input"
import {
  InputGroup,
  InputGroupAddon,
  InputGroupButton,
  InputGroupInput,
  InputGroupText,
  InputGroupTextarea,
} from "@/registry/new-york-v4/ui/input-group"
import { InputOTP, InputOTPGroup, InputOTPSeparator, InputOTPSlot } from "@/registry/new-york-v4/ui/input-otp"
import { Label } from "@/registry/new-york-v4/ui/label"
import { NativeSelect, NativeSelectOption } from "@/registry/new-york-v4/ui/native-select"
import { Progress } from "@/registry/new-york-v4/ui/progress"
import { RadioGroup, RadioGroupItem } from "@/registry/new-york-v4/ui/radio-group"
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectLabel,
  SelectSeparator,
  SelectTrigger,
  SelectValue,
} from "@/registry/new-york-v4/ui/select"
import { Slider } from "@/registry/new-york-v4/ui/slider"
import { Switch } from "@/registry/new-york-v4/ui/switch"
import { Textarea } from "@/registry/new-york-v4/ui/textarea"
import { Toggle } from "@/registry/new-york-v4/ui/toggle"
import { ToggleGroup, ToggleGroupItem } from "@/registry/new-york-v4/ui/toggle-group"

import { blockFixtures, shellFixtures } from "./blocks"
import { chartFixtures } from "./charts"
import { chatFixtures } from "./chat"
import { compositeFixtures } from "./composites"
import { dateFixtures } from "./dates"
import { displayFixtures } from "./display"
import { navigationFixtures } from "./navigation"
import { overlayFixtures } from "./overlays"
import { rheaFixtures } from "./rhea"
import { scrollerFixtures } from "./scroller"
import { structureFixtures } from "./structure"

const variants = ["default", "destructive", "outline", "secondary", "ghost", "link"] as const
const badges = ["default", "secondary", "destructive", "outline", "ghost", "link"] as const

function Row({ children }: { children: ReactNode }) {
  return <div className="flex flex-wrap items-center gap-2">{children}</div>
}

const tokens = ["background","foreground","card","card-foreground","popover","popover-foreground","primary","primary-foreground","secondary","secondary-foreground","muted","muted-foreground","accent","accent-foreground","destructive","border","input","ring","chart-1","chart-2","chart-3","chart-4","chart-5","sidebar","sidebar-foreground","sidebar-primary","sidebar-primary-foreground","sidebar-accent","sidebar-accent-foreground","sidebar-border","sidebar-ring"]

export const fixtures: Record<string, () => ReactNode> = {
  ...overlayFixtures,
  ...displayFixtures,
  ...navigationFixtures,
  ...structureFixtures,
  ...compositeFixtures,
  ...dateFixtures,
  ...chartFixtures,
  ...blockFixtures,
  ...shellFixtures,
  ...chatFixtures,
  ...scrollerFixtures,
  ...rheaFixtures,
  tokens: () => (
    <div className="flex flex-wrap gap-1">
      {tokens.map((token) => (
        <div key={token} data-token={token} style={{ width: 16, height: 16, background: `var(--${token})` }} />
      ))}
    </div>
  ),
  "button-variants": () => (
    <Row>
      {variants.map((variant) => (
        <Button key={variant} variant={variant}>
          Button
        </Button>
      ))}
    </Row>
  ),
  "button-sizes": () => (
    <Row>
      <Button size="sm">Small</Button>
      <Button>Default</Button>
      <Button size="lg">Large</Button>
      <Button size="icon" aria-label="Next">
        <ChevronRightIcon />
      </Button>
      <Button size="sm" variant="outline">Small</Button>
      <Button variant="outline">Default</Button>
      <Button size="lg" variant="outline">Large</Button>
      <Button size="icon" variant="outline" aria-label="Next">
        <ChevronRightIcon />
      </Button>
    </Row>
  ),
  "button-extra-sizes": () => (
    <Row>
      <Button size="xs">Extra small</Button>
      <Button size="xs">
        <ChevronRightIcon /> Next
      </Button>
      <Button size="icon-xs" aria-label="Next">
        <ChevronRightIcon />
      </Button>
      <Button size="icon-sm" aria-label="Next">
        <ChevronRightIcon />
      </Button>
      <Button size="icon-lg" aria-label="Next">
        <ChevronRightIcon />
      </Button>
      <Button size="xs" variant="outline">Extra small</Button>
      <Button size="icon-xs" variant="outline" aria-label="Next">
        <ChevronRightIcon />
      </Button>
      <Button size="icon-sm" variant="outline" aria-label="Next">
        <ChevronRightIcon />
      </Button>
      <Button size="icon-lg" variant="outline" aria-label="Next">
        <ChevronRightIcon />
      </Button>
    </Row>
  ),
  "button-disabled": () => (
    <Row>
      {variants.map((variant) => (
        <Button key={variant} variant={variant} disabled>
          Button
        </Button>
      ))}
    </Row>
  ),
  "button-icons": () => (
    <Row>
      <Button>
        <MailIcon /> Login with Email
      </Button>
      <Button variant="outline" size="sm">
        <PlusIcon /> New
      </Button>
      <Button variant="secondary" size="lg">
        Continue <ChevronRightIcon />
      </Button>
    </Row>
  ),
  "badge-variants": () => (
    <Row>
      {badges.map((variant) => (
        <Badge key={variant} variant={variant}>
          Badge
        </Badge>
      ))}
    </Row>
  ),
  "badge-icons": () => (
    <Row>
      <Badge variant="secondary">
        <CheckIcon /> Verified
      </Badge>
      <Badge className="h-5 min-w-5 rounded-full px-1 font-mono tabular-nums">8</Badge>
      <Badge className="h-5 min-w-5 rounded-full px-1 font-mono tabular-nums" variant="destructive">
        99
      </Badge>
      <Badge className="h-5 min-w-5 rounded-full px-1 font-mono tabular-nums" variant="outline">
        20+
      </Badge>
    </Row>
  ),
  "badge-links": () => (
    <Row>
      {badges.map((variant) => (
        <Badge key={variant} variant={variant} asChild>
          <a href="#">Badge</a>
        </Badge>
      ))}
    </Row>
  ),
  "card-separated": () => (
    <Card className="w-full max-w-sm">
      <CardHeader className="border-b">
        <CardTitle>Team members</CardTitle>
        <CardDescription>Invite your team to collaborate.</CardDescription>
      </CardHeader>
      <CardContent>
        <p className="text-sm">Members can edit every project.</p>
      </CardContent>
      <CardFooter className="border-t">
        <Button>Invite</Button>
      </CardFooter>
    </Card>
  ),
  "card-basic": () => (
    <Card className="w-full max-w-sm">
      <CardHeader>
        <CardTitle>Login to your account</CardTitle>
        <CardDescription>Enter your email below to login to your account</CardDescription>
        <CardAction>
          <Button variant="link">Sign Up</Button>
        </CardAction>
      </CardHeader>
      <CardContent>
        <p className="text-sm">Your session expires in 30 days on this device.</p>
      </CardContent>
      <CardFooter className="flex-col gap-2">
        <Button type="submit" className="w-full">
          Login
        </Button>
        <Button variant="outline" className="w-full">
          Login with Google
        </Button>
      </CardFooter>
    </Card>
  ),
  "card-plain": () => (
    <Card className="w-full max-w-sm">
      <CardHeader>
        <CardTitle>Notifications</CardTitle>
        <CardDescription>You have 3 unread messages.</CardDescription>
      </CardHeader>
      <CardContent>
        <p className="text-sm">Push notifications are sent to your phone.</p>
      </CardContent>
    </Card>
  ),

  "input-file": () => (
    <div className="grid w-full max-w-sm items-center gap-3">
      <Label htmlFor="picture">Picture</Label>
      <Input id="picture" type="file" />
    </div>
  ),
  "input-states": () => (
    <div className="flex w-full max-w-sm flex-col gap-4">
      <div className="grid w-full max-w-sm items-center gap-3">
        <Label htmlFor="email">Email</Label>
        <Input type="email" id="email" placeholder="Email" />
      </div>
      <Input defaultValue="shadcn@example.com" />
      <Input disabled placeholder="Disabled" />
    </div>
  ),
  "textarea-states": () => (
    <div className="flex w-full max-w-sm flex-col gap-4">
      <Textarea placeholder="Type your message here." />
      <Textarea disabled placeholder="Disabled" />
    </div>
  ),
  "field-form": () => (
    <div className="w-full max-w-md">
      <FieldGroup>
        <Field>
          <FieldLabel htmlFor="username">Username</FieldLabel>
          <Input id="username" placeholder="evilrabbit" />
          <FieldDescription>Choose a unique username for your account.</FieldDescription>
        </Field>
        <Field data-invalid="true">
          <FieldLabel htmlFor="mail">Email</FieldLabel>
          <Input id="mail" aria-invalid="true" defaultValue="not-an-email" />
          <FieldError>Enter a valid email address.</FieldError>
        </Field>
        <FieldSet>
          <FieldLegend>Notifications</FieldLegend>
          <FieldDescription>Choose what we email you about.</FieldDescription>
          <Field orientation="horizontal">
            <Checkbox id="news" defaultChecked />
            <FieldLabel htmlFor="news" className="font-normal">Product news</FieldLabel>
          </Field>
        </FieldSet>
      </FieldGroup>
    </div>
  ),
  "field-content": () => (
    <div className="w-full max-w-md">
      <FieldGroup>
        <Field orientation="horizontal">
          <FieldContent>
            <FieldLabel htmlFor="2fa">Multi-factor authentication</FieldLabel>
            <FieldDescription>
              Enable multi-factor authentication. If you do not have a two-factor device, you can use a one-time code sent to your email.
            </FieldDescription>
          </FieldContent>
          <Switch id="2fa" />
        </Field>
        <Field orientation="horizontal">
          <FieldContent>
            <FieldTitle>Kubernetes</FieldTitle>
            <FieldDescription>Run GPU workloads on a K8s configured cluster.</FieldDescription>
          </FieldContent>
          <Checkbox id="k8s" />
        </Field>
      </FieldGroup>
    </div>
  ),
  "field-responsive": () => (
    <div className="flex flex-col items-start gap-6">
      {[560, 400].map((width) => (
        <FieldGroup key={width} style={{ width }}>
          <Field orientation="responsive">
            <FieldContent>
              <FieldLabel htmlFor={`name-${width}`}>Name</FieldLabel>
              <FieldDescription>Provide your full name for identification.</FieldDescription>
            </FieldContent>
            <Input id={`name-${width}`} placeholder="Evil Rabbit" className="@md/field-group:w-[200px]!" />
          </Field>
        </FieldGroup>
      ))}
    </div>
  ),
  "button-invalid": () => (
    <div className="flex flex-col items-start gap-4">
      <Button variant="outline" aria-invalid="true">Pick a date</Button>
      <Field data-invalid="true" className="w-fit">
        <Button variant="outline" aria-invalid="true">Choose a plan</Button>
      </Field>
      <Button aria-invalid="true">Submit</Button>
    </div>
  ),
  "checkbox-states": () => (
    <div className="flex flex-col gap-6">
      <div className="flex items-center gap-3">
        <Checkbox id="terms" />
        <Label htmlFor="terms">Accept terms and conditions</Label>
      </div>
      <div className="flex items-center gap-3">
        <Checkbox id="notify" defaultChecked />
        <Label htmlFor="notify">Enable notifications</Label>
      </div>
      <div className="flex items-center gap-3">
        <Checkbox id="all" checked="indeterminate" />
        <Label htmlFor="all">Select all</Label>
      </div>
      <div className="flex items-center gap-3">
        <Checkbox id="off" disabled />
        <Label htmlFor="off">Disabled</Label>
      </div>
    </div>
  ),
  "radio-group": () => (
    <RadioGroup defaultValue="comfortable">
      <div className="flex items-center gap-3">
        <RadioGroupItem value="default" id="r1" />
        <Label htmlFor="r1">Default</Label>
      </div>
      <div className="flex items-center gap-3">
        <RadioGroupItem value="comfortable" id="r2" />
        <Label htmlFor="r2">Comfortable</Label>
      </div>
      <div className="flex items-center gap-3">
        <RadioGroupItem value="compact" id="r3" disabled />
        <Label htmlFor="r3">Compact</Label>
      </div>
    </RadioGroup>
  ),
  "switch-states": () => (
    <div className="flex flex-col gap-6">
      <div className="flex items-center gap-2">
        <Switch id="airplane" />
        <Label htmlFor="airplane">Airplane Mode</Label>
      </div>
      <div className="flex items-center gap-2">
        <Switch id="wifi" defaultChecked />
        <Label htmlFor="wifi">Wi-Fi</Label>
      </div>
      <div className="flex items-center gap-2">
        <Switch id="locked" disabled />
        <Label htmlFor="locked">Disabled</Label>
      </div>
      <div className="flex items-center gap-2">
        <Switch id="small" size="sm" defaultChecked />
        <Label htmlFor="small">Small</Label>
      </div>
    </div>
  ),
  "native-select": () => (
    <div className="flex flex-col gap-4">
      <NativeSelect defaultValue="">
        <NativeSelectOption value="">Select status</NativeSelectOption>
        <NativeSelectOption value="todo">Todo</NativeSelectOption>
        <NativeSelectOption value="done">Done</NativeSelectOption>
      </NativeSelect>
      <NativeSelect size="sm" defaultValue="todo">
        <NativeSelectOption value="todo">Todo</NativeSelectOption>
        <NativeSelectOption value="done">Done</NativeSelectOption>
      </NativeSelect>
      <NativeSelect disabled defaultValue="todo">
        <NativeSelectOption value="todo">Todo</NativeSelectOption>
      </NativeSelect>
    </div>
  ),
  "select-trigger": () => (
    <div className="flex flex-col gap-4">
      <Select>
        <SelectTrigger className="w-[180px]">
          <SelectValue placeholder="Select a fruit" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="apple">Apple</SelectItem>
        </SelectContent>
      </Select>
      <Select defaultValue="banana">
        <SelectTrigger className="w-[180px]">
          <SelectValue />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="banana">Banana</SelectItem>
        </SelectContent>
      </Select>
      <Select defaultValue="banana">
        <SelectTrigger size="sm" className="w-[180px]">
          <SelectValue />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="banana">Banana</SelectItem>
        </SelectContent>
      </Select>
      <Select disabled defaultValue="banana">
        <SelectTrigger className="w-[180px]">
          <SelectValue />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="banana">Banana</SelectItem>
        </SelectContent>
      </Select>
    </div>
  ),
  "select-open": () => (
    <div className="h-[320px]">
      <Select defaultValue="banana" open>
        <SelectTrigger className="w-[180px]">
          <SelectValue />
        </SelectTrigger>
        <SelectContent position="popper">
          <SelectGroup>
            <SelectLabel>Fruits</SelectLabel>
            <SelectItem value="apple">Apple</SelectItem>
            <SelectItem value="banana">Banana</SelectItem>
            <SelectItem value="grapes" disabled>Grapes</SelectItem>
          </SelectGroup>
          <SelectSeparator />
          <SelectGroup>
            <SelectLabel>Vegetables</SelectLabel>
            <SelectItem value="carrot">Carrot</SelectItem>
          </SelectGroup>
        </SelectContent>
      </Select>
    </div>
  ),
  "select-scrollable": () => (
    <div className="h-[440px]">
      <Select open>
        <SelectTrigger className="w-[280px]">
          <SelectValue placeholder="Select a timezone" />
        </SelectTrigger>
        <SelectContent position="popper" className="max-h-96">
          <SelectGroup>
            <SelectLabel>North America</SelectLabel>
            <SelectItem value="est">Eastern Standard Time (EST)</SelectItem>
            <SelectItem value="cst">Central Standard Time (CST)</SelectItem>
            <SelectItem value="mst">Mountain Standard Time (MST)</SelectItem>
            <SelectItem value="pst">Pacific Standard Time (PST)</SelectItem>
            <SelectItem value="akst">Alaska Standard Time (AKST)</SelectItem>
            <SelectItem value="hst">Hawaii Standard Time (HST)</SelectItem>
          </SelectGroup>
          <SelectGroup>
            <SelectLabel>Europe &amp; Africa</SelectLabel>
            <SelectItem value="gmt">Greenwich Mean Time (GMT)</SelectItem>
            <SelectItem value="cet">Central European Time (CET)</SelectItem>
            <SelectItem value="eet">Eastern European Time (EET)</SelectItem>
            <SelectItem value="west">Western European Summer Time (WEST)</SelectItem>
            <SelectItem value="cat">Central Africa Time (CAT)</SelectItem>
            <SelectItem value="eat">East Africa Time (EAT)</SelectItem>
          </SelectGroup>
          <SelectGroup>
            <SelectLabel>Asia</SelectLabel>
            <SelectItem value="msk">Moscow Time (MSK)</SelectItem>
            <SelectItem value="ist">India Standard Time (IST)</SelectItem>
            <SelectItem value="cst_china">China Standard Time (CST)</SelectItem>
            <SelectItem value="jst">Japan Standard Time (JST)</SelectItem>
            <SelectItem value="kst">Korea Standard Time (KST)</SelectItem>
            <SelectItem value="ist_indonesia">Indonesia Central Standard Time (WITA)</SelectItem>
          </SelectGroup>
          <SelectGroup>
            <SelectLabel>Australia &amp; Pacific</SelectLabel>
            <SelectItem value="awst">Australian Western Standard Time (AWST)</SelectItem>
            <SelectItem value="acst">Australian Central Standard Time (ACST)</SelectItem>
            <SelectItem value="aest">Australian Eastern Standard Time (AEST)</SelectItem>
            <SelectItem value="nzst">New Zealand Standard Time (NZST)</SelectItem>
            <SelectItem value="fjt">Fiji Time (FJT)</SelectItem>
          </SelectGroup>
          <SelectGroup>
            <SelectLabel>South America</SelectLabel>
            <SelectItem value="art">Argentina Time (ART)</SelectItem>
            <SelectItem value="bot">Bolivia Time (BOT)</SelectItem>
            <SelectItem value="brt">Brasilia Time (BRT)</SelectItem>
            <SelectItem value="clt">Chile Standard Time (CLT)</SelectItem>
          </SelectGroup>
        </SelectContent>
      </Select>
    </div>
  ),
  "select-scrolled": () => (
    <div className="h-[440px]">
      <Select defaultValue="nzst" open>
        <SelectTrigger className="w-[280px]">
          <SelectValue />
        </SelectTrigger>
        <SelectContent position="popper" className="max-h-96">
          <SelectGroup>
            <SelectLabel>North America</SelectLabel>
            <SelectItem value="est">Eastern Standard Time (EST)</SelectItem>
            <SelectItem value="cst">Central Standard Time (CST)</SelectItem>
            <SelectItem value="mst">Mountain Standard Time (MST)</SelectItem>
            <SelectItem value="pst">Pacific Standard Time (PST)</SelectItem>
            <SelectItem value="akst">Alaska Standard Time (AKST)</SelectItem>
            <SelectItem value="hst">Hawaii Standard Time (HST)</SelectItem>
          </SelectGroup>
          <SelectGroup>
            <SelectLabel>Europe &amp; Africa</SelectLabel>
            <SelectItem value="gmt">Greenwich Mean Time (GMT)</SelectItem>
            <SelectItem value="cet">Central European Time (CET)</SelectItem>
            <SelectItem value="eet">Eastern European Time (EET)</SelectItem>
            <SelectItem value="west">Western European Summer Time (WEST)</SelectItem>
            <SelectItem value="cat">Central Africa Time (CAT)</SelectItem>
            <SelectItem value="eat">East Africa Time (EAT)</SelectItem>
          </SelectGroup>
          <SelectGroup>
            <SelectLabel>Asia</SelectLabel>
            <SelectItem value="msk">Moscow Time (MSK)</SelectItem>
            <SelectItem value="ist">India Standard Time (IST)</SelectItem>
            <SelectItem value="cst_china">China Standard Time (CST)</SelectItem>
            <SelectItem value="jst">Japan Standard Time (JST)</SelectItem>
            <SelectItem value="kst">Korea Standard Time (KST)</SelectItem>
            <SelectItem value="ist_indonesia">Indonesia Central Standard Time (WITA)</SelectItem>
          </SelectGroup>
          <SelectGroup>
            <SelectLabel>Australia &amp; Pacific</SelectLabel>
            <SelectItem value="awst">Australian Western Standard Time (AWST)</SelectItem>
            <SelectItem value="acst">Australian Central Standard Time (ACST)</SelectItem>
            <SelectItem value="aest">Australian Eastern Standard Time (AEST)</SelectItem>
            <SelectItem value="nzst">New Zealand Standard Time (NZST)</SelectItem>
            <SelectItem value="fjt">Fiji Time (FJT)</SelectItem>
          </SelectGroup>
          <SelectGroup>
            <SelectLabel>South America</SelectLabel>
            <SelectItem value="art">Argentina Time (ART)</SelectItem>
            <SelectItem value="bot">Bolivia Time (BOT)</SelectItem>
            <SelectItem value="brt">Brasilia Time (BRT)</SelectItem>
            <SelectItem value="clt">Chile Standard Time (CLT)</SelectItem>
          </SelectGroup>
        </SelectContent>
      </Select>
    </div>
  ),
  "slider-range": () => (
    <div className="flex items-start gap-12">
      <div className="w-[360px]">
        <Field>
          <FieldTitle>Price Range</FieldTitle>
          <FieldDescription>
            Set your budget range ($<span className="font-medium tabular-nums">200</span> - 
            <span className="font-medium tabular-nums">800</span>).
          </FieldDescription>
          <Slider defaultValue={[200, 800]} max={1000} min={0} step={10} className="mt-2 w-full" aria-label="Price Range" />
        </Field>
      </div>
      <Slider defaultValue={[20, 60]} max={100} step={1} orientation="vertical" />
    </div>
  ),
  "slider-states": () => (
    <div className="flex items-start gap-12">
      <div className="flex w-[360px] flex-col gap-8">
        <Slider defaultValue={[50]} max={100} step={1} />
        <Slider defaultValue={[25, 75]} max={100} step={1} />
        <Slider defaultValue={[33]} max={100} step={1} disabled />
      </div>
      <Slider defaultValue={[40]} max={100} step={1} orientation="vertical" />
    </div>
  ),
  progress: () => (
    <div className="flex w-[360px] flex-col gap-4">
      <Progress value={33} />
      <Progress value={66} />
      <Progress value={100} />
    </div>
  ),
  "toggle-states": () => (
    <Row>
      <Toggle aria-label="Like">
        <HeartIcon />
      </Toggle>
      <Toggle aria-label="Star" defaultPressed>
        <StarIcon />
      </Toggle>
      <Toggle variant="outline" aria-label="Bell">
        <BellIcon />
      </Toggle>
      <Toggle size="sm" aria-label="Like">
        <HeartIcon />
      </Toggle>
      <Toggle size="lg" aria-label="Like">
        <HeartIcon />
      </Toggle>
      <Toggle aria-label="Like" disabled>
        <HeartIcon />
      </Toggle>
      <Toggle variant="outline" aria-label="Favorite">
        <StarIcon />
        Favorite
      </Toggle>
    </Row>
  ),
  "toggle-group": () => (
    <div className="flex flex-col items-start gap-6">
      <ToggleGroup variant="outline" type="multiple" defaultValue={["star"]}>
        <ToggleGroupItem value="heart" aria-label="Heart">
          <HeartIcon />
        </ToggleGroupItem>
        <ToggleGroupItem value="star" aria-label="Star">
          <StarIcon />
        </ToggleGroupItem>
        <ToggleGroupItem value="bell" aria-label="Bell">
          <BellIcon />
        </ToggleGroupItem>
      </ToggleGroup>
      <ToggleGroup type="single" defaultValue="center">
        <ToggleGroupItem value="left">Left</ToggleGroupItem>
        <ToggleGroupItem value="center">Center</ToggleGroupItem>
        <ToggleGroupItem value="right">Right</ToggleGroupItem>
      </ToggleGroup>
      <ToggleGroup type="single" size="sm" variant="outline" spacing={2} defaultValue="day">
        <ToggleGroupItem value="day">Day</ToggleGroupItem>
        <ToggleGroupItem value="week">Week</ToggleGroupItem>
        <ToggleGroupItem value="month">Month</ToggleGroupItem>
      </ToggleGroup>
    </div>
  ),
  "input-group": () => (
    <div className="grid w-full max-w-sm gap-6">
      <InputGroup>
        <InputGroupInput placeholder="Search..." />
        <InputGroupAddon>
          <SearchIcon />
        </InputGroupAddon>
        <InputGroupAddon align="inline-end">12 results</InputGroupAddon>
      </InputGroup>
      <InputGroup>
        <InputGroupInput placeholder="example.com" />
        <InputGroupAddon>
          <InputGroupText>https://</InputGroupText>
        </InputGroupAddon>
      </InputGroup>
      <InputGroup>
        <InputGroupTextarea placeholder="Ask, Search or Chat..." />
        <InputGroupAddon align="block-end">
          <InputGroupText>52% used</InputGroupText>
          <InputGroupButton variant="default" className="ml-auto rounded-full" size="icon-xs" aria-label="Send">
            <PlusIcon />
          </InputGroupButton>
        </InputGroupAddon>
      </InputGroup>
    </div>
  ),
  "input-otp": () => (
    <div className="flex flex-col gap-6">
      <InputOTP maxLength={6} value="123" onChange={() => {}}>
        <InputOTPGroup>
          <InputOTPSlot index={0} />
          <InputOTPSlot index={1} />
          <InputOTPSlot index={2} />
        </InputOTPGroup>
        <InputOTPSeparator />
        <InputOTPGroup>
          <InputOTPSlot index={3} />
          <InputOTPSlot index={4} />
          <InputOTPSlot index={5} />
        </InputOTPGroup>
      </InputOTP>
      <InputOTP maxLength={4} disabled>
        <InputOTPGroup>
          <InputOTPSlot index={0} />
          <InputOTPSlot index={1} />
          <InputOTPSlot index={2} />
          <InputOTPSlot index={3} />
        </InputOTPGroup>
      </InputOTP>
    </div>
  ),
}
