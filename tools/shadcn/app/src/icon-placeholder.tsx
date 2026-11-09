import type { ComponentProps } from "react"
import { CheckIcon, ChevronDownIcon, ChevronRightIcon, CopyIcon } from "lucide-react"

/** shadcn/ui's `IconPlaceholder` (apps/v4/app/(create)/components): the radix-rhea preset draws Lucide's icon. */
const icons = { CheckIcon, ChevronDownIcon, ChevronRightIcon, CopyIcon }

type Props = ComponentProps<"svg"> & { lucide: keyof typeof icons; tabler?: string; hugeicons?: string; phosphor?: string; remixicon?: string }

export function IconPlaceholder({ lucide, tabler: _tabler, hugeicons: _hugeicons, phosphor: _phosphor, remixicon: _remixicon, ...svg }: Props) {
  const Icon = icons[lucide]
  return <Icon {...svg} />
}
