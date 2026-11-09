import { useState, type ReactNode } from "react"
import { addDays, format } from "date-fns"
import { CalendarIcon } from "lucide-react"
import type { DateRange } from "react-day-picker"

import { cn } from "cn"
import { Button } from "@/registry/new-york-v4/ui/button"
import { Calendar } from "@/registry/new-york-v4/ui/calendar"
import { Input } from "@/registry/new-york-v4/ui/input"
import { Label } from "@/registry/new-york-v4/ui/label"
import { Popover, PopoverContent, PopoverTrigger } from "@/registry/new-york-v4/ui/popover"
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/registry/new-york-v4/ui/select"

const today = new Date(2026, 9, 16)
const june12 = new Date(2025, 5, 12)
const block = "rounded-lg border shadow-sm"

function SinglePicker({ open }: { open?: boolean }) {
  const [date, setDate] = useState<Date | undefined>(open ? june12 : undefined)
  return (
    <Popover>
      <PopoverTrigger asChild>
        <Button variant="outline" className={cn("w-[240px] justify-start text-left font-normal", !date && "text-muted-foreground")}>
          <CalendarIcon />
          {date ? format(date, "PPP") : <span>Pick a date</span>}
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-auto p-0" align="start">
        <Calendar mode="single" today={today} defaultMonth={june12} selected={date} onSelect={setDate} />
      </PopoverContent>
    </Popover>
  )
}

function RangePicker() {
  const [date, setDate] = useState<DateRange | undefined>({ from: new Date(2022, 0, 20), to: addDays(new Date(2022, 0, 20), 20) })
  return (
    <div className="grid gap-2">
      <Popover>
        <PopoverTrigger asChild>
          <Button id="date" variant="outline" className={cn("w-[300px] justify-start text-left font-normal", !date && "text-muted-foreground")}>
            <CalendarIcon />
            {date?.from ? (date.to ? <>{format(date.from, "LLL dd, y")} - {format(date.to, "LLL dd, y")}</> : format(date.from, "LLL dd, y")) : <span>Pick a date</span>}
          </Button>
        </PopoverTrigger>
        <PopoverContent className="w-auto p-0" align="start">
          <Calendar mode="range" today={today} defaultMonth={date?.from} selected={date} onSelect={setDate} numberOfMonths={2} />
        </PopoverContent>
      </Popover>
    </div>
  )
}

function PresetsPicker() {
  const [date, setDate] = useState<Date | undefined>(june12)
  return (
    <Popover>
      <PopoverTrigger asChild>
        <Button variant="outline" className={cn("w-[240px] justify-start text-left font-normal", !date && "text-muted-foreground")}>
          <CalendarIcon />
          {date ? format(date, "PPP") : <span>Pick a date</span>}
        </Button>
      </PopoverTrigger>
      <PopoverContent align="start" className="flex w-auto flex-col space-y-2 p-2">
        <Select onValueChange={(value) => setDate(addDays(today, parseInt(value)))}>
          <SelectTrigger>
            <SelectValue placeholder="Select" />
          </SelectTrigger>
          <SelectContent position="popper">
            <SelectItem value="0">Today</SelectItem>
            <SelectItem value="1">Tomorrow</SelectItem>
            <SelectItem value="3">In 3 days</SelectItem>
            <SelectItem value="7">In a week</SelectItem>
          </SelectContent>
        </Select>
        <div className="rounded-md border">
          <Calendar mode="single" today={today} defaultMonth={june12} selected={date} onSelect={setDate} />
        </div>
      </PopoverContent>
    </Popover>
  )
}

function InputPicker() {
  const [date, setDate] = useState<Date | undefined>(new Date(2025, 5, 1))
  const [open, setOpen] = useState(false)
  const value = date ? date.toLocaleDateString("en-US", { day: "2-digit", month: "long", year: "numeric" }) : ""
  return (
    <div className="flex flex-col gap-3">
      <Label htmlFor="date" className="px-1">Subscription Date</Label>
      <div className="relative flex gap-2">
        <Input id="date" value={value} placeholder="June 01, 2025" className="bg-background pr-10" readOnly />
        <Popover open={open} onOpenChange={setOpen}>
          <PopoverTrigger asChild>
            <Button id="date-picker" variant="ghost" size="icon" className="absolute top-1/2 right-2 h-6 w-6 -translate-y-1/2">
              <CalendarIcon className="size-3" />
              <span className="sr-only">Select date</span>
            </Button>
          </PopoverTrigger>
          <PopoverContent className="w-auto overflow-hidden p-0" align="end" alignOffset={-8} sideOffset={10}>
            <Calendar mode="single" today={today} selected={date} captionLayout="dropdown" defaultMonth={date} onSelect={(next) => { setDate(next); setOpen(false) }} />
          </PopoverContent>
        </Popover>
      </div>
    </div>
  )
}

function Stateful({ children }: { children: (state: { dates: Date[]; setDates: (dates: Date[] | undefined) => void }) => ReactNode }) {
  const [dates, setDates] = useState<Date[]>([new Date(2025, 5, 3), new Date(2025, 5, 12), new Date(2025, 5, 13), new Date(2025, 5, 24)])
  return <>{children({ dates, setDates: (next) => setDates(next ?? []) })}</>
}

export const dateFixtures: Record<string, () => ReactNode> = {
  "calendar-single": () => <Calendar mode="single" today={today} defaultMonth={june12} selected={june12} className={block} />,
  "calendar-range": () => <Calendar mode="range" today={today} defaultMonth={new Date(2025, 5, 9)} selected={{ from: new Date(2025, 5, 9), to: new Date(2025, 5, 26) }} className={block} />,
  "calendar-multiple": () => <Stateful>{({ dates, setDates }) => <Calendar mode="multiple" today={today} defaultMonth={june12} selected={dates} onSelect={setDates} className={block} />}</Stateful>,
  "calendar-dropdown": () => <Calendar mode="single" today={today} defaultMonth={june12} selected={june12} captionLayout="dropdown" className={block} />,
  "calendar-week-numbers": () => <Calendar mode="single" today={today} defaultMonth={june12} selected={june12} showWeekNumber className={block} />,
  "calendar-disabled": () => <Calendar mode="single" today={today} defaultMonth={june12} selected={june12} disabled={{ before: june12 }} className={block} />,
  "calendar-outside-hidden": () => <Calendar mode="single" today={today} defaultMonth={june12} selected={june12} showOutsideDays={false} className={block} />,
  "date-picker-demo": () => <SinglePicker />,
  "date-picker-open": () => <div className="h-[372px]"><SinglePicker open /></div>,
  "date-picker-range-open": () => <div className="h-[372px]"><RangePicker /></div>,
  "date-picker-presets-open": () => <div className="h-[420px]"><PresetsPicker /></div>,
  "date-picker-input": () => <InputPicker />,
}
