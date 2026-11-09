import type { ReactNode } from "react"

import { MessageDemo } from "@/registry/new-york-v4/examples/message-demo"
import { MessageGroupDemo } from "@/registry/new-york-v4/examples/message-group"
import { MessageHeaderFooterDemo } from "@/registry/new-york-v4/examples/message-header-footer"
import { MessageAvatarDemo } from "@/registry/new-york-v4/examples/message-avatar"
import { BubbleDemo } from "@/registry/new-york-v4/examples/bubble-demo"
import { BubbleVariantsDemo } from "@/registry/new-york-v4/examples/bubble-variants"
import { BubbleAlignmentDemo } from "@/registry/new-york-v4/examples/bubble-alignment"
import { BubbleGroupDemo } from "@/registry/new-york-v4/examples/bubble-group-demo"
import { MarkerDemo } from "@/registry/new-york-v4/examples/marker-demo"
import { MarkerVariantsDemo } from "@/registry/new-york-v4/examples/marker-variants"
import { MarkerIconDemo } from "@/registry/new-york-v4/examples/marker-icon"
import { MarkerBorderDemo } from "@/registry/new-york-v4/examples/marker-border"
import { AttachmentDemo } from "@/registry/new-york-v4/examples/attachment-demo"
import { AttachmentStates } from "@/registry/new-york-v4/examples/attachment-states"
import { AttachmentSizes } from "@/registry/new-york-v4/examples/attachment-sizes"
import { AttachmentGroupDemo } from "@/registry/new-york-v4/examples/attachment-group"
import { MarkerShimmerDemo } from "@/registry/new-york-v4/examples/marker-shimmer"
import { MarkerStatusDemo } from "@/registry/new-york-v4/examples/marker-status"
import { MarkerLinkButtonDemo } from "@/registry/new-york-v4/examples/marker-link-button"
import { BubbleLinkButtonDemo } from "@/registry/new-york-v4/examples/bubble-link-button"
import { BubbleCollapsible } from "@/registry/new-york-v4/examples/bubble-collapsible"
import { AttachmentTriggerDemo } from "@/registry/new-york-v4/examples/attachment-trigger"
import { AttachmentImage } from "@/registry/new-york-v4/examples/attachment-image"
import { BubbleMarkdownDemo } from "@/registry/new-york-v4/examples/bubble-markdown"

/** shadcn/ui's chat components (message, bubble, marker, attachment) in their examples, adapted to the new-york-v4 components. */
export const chatFixtures: Record<string, () => ReactNode> = {
  "message-demo": () => <MessageDemo />,
  "message-group": () => <MessageGroupDemo />,
  "message-header-footer": () => <MessageHeaderFooterDemo />,
  "message-avatar": () => <MessageAvatarDemo />,
  "bubble-demo": () => <BubbleDemo />,
  "bubble-variants": () => <BubbleVariantsDemo />,
  "bubble-alignment": () => <BubbleAlignmentDemo />,
  "bubble-group-demo": () => <BubbleGroupDemo />,
  "marker-demo": () => <MarkerDemo />,
  "marker-variants": () => <MarkerVariantsDemo />,
  "marker-icon": () => <MarkerIconDemo />,
  "marker-border": () => <MarkerBorderDemo />,
  "marker-shimmer": () => <MarkerShimmerDemo />,
  "marker-status": () => <MarkerStatusDemo />,
  "marker-link-button": () => <MarkerLinkButtonDemo />,
  "bubble-link-button": () => <BubbleLinkButtonDemo />,
  "bubble-collapsible": () => <BubbleCollapsible />,
  "attachment-trigger": () => <AttachmentTriggerDemo />,
  "attachment-image": () => <AttachmentImage />,
  "bubble-markdown": () => <BubbleMarkdownDemo />,
  "attachment-demo": () => <AttachmentDemo />,
  "attachment-states": () => <AttachmentStates />,
  "attachment-sizes": () => <AttachmentSizes />,
  "attachment-group": () => <AttachmentGroupDemo />,
}
