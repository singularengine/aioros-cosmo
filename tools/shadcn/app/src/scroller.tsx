import type { ReactNode } from "react"

import { Bubble, BubbleContent } from "@/registry/new-york-v4/ui/bubble"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/registry/new-york-v4/ui/card"
import { Message, MessageContent } from "@/registry/new-york-v4/ui/message"
import {
  MessageScroller,
  MessageScrollerButton,
  MessageScrollerContent,
  MessageScrollerItem,
  MessageScrollerProvider,
  MessageScrollerViewport,
} from "@/registry/new-york-v4/ui/message-scroller"

/** A static transcript: shadcn/ui's scroller examples stream theirs from a mock chat (`@/lib/ai`), which is not in the registry. */
const transcript: { id: string; mine: boolean; text: string }[] = [
  { id: "m1", mine: true, text: "Deploying to prod real quick." },
  { id: "m2", mine: false, text: "It's 4:55 PM. On a Friday." },
  { id: "m3", mine: true, text: "It's a one-line change." },
  { id: "m4", mine: false, text: "It's always a one-line change. Alright, let me take a look." },
  { id: "m5", mine: true, text: "The build failed during dependency installation." },
  { id: "m6", mine: false, text: "Can you share the exact error?" },
  { id: "m7", mine: true, text: "Something went wrong with the build. The libraries are not installed correctly." },
  { id: "m8", mine: false, text: "Try running the build again. I cleared the cache." },
  { id: "m9", mine: true, text: "It worked. Thanks!" },
]

function Scroller({ position }: { position: "start" | "end" }) {
  return (
    <div className="mx-auto flex w-full max-w-sm flex-col">
      <Card className="h-100 w-full gap-0 overflow-hidden py-0">
        <CardHeader className="gap-1 border-b py-4">
          <CardTitle>Support</CardTitle>
          <CardDescription>A conversation about a failed deploy.</CardDescription>
        </CardHeader>
        <MessageScrollerProvider defaultScrollPosition={position}>
          <CardContent className="flex-1 overflow-hidden p-0">
            <MessageScroller>
              <MessageScrollerViewport>
                <MessageScrollerContent className="gap-4 p-6">
                  {transcript.map((message) => (
                    <MessageScrollerItem key={message.id} messageId={message.id} scrollAnchor={message.mine}>
                      <Message align={message.mine ? "end" : "start"}>
                        <MessageContent>
                          <Bubble variant={message.mine ? "default" : "muted"} align={message.mine ? "end" : "start"}>
                            <BubbleContent>{message.text}</BubbleContent>
                          </Bubble>
                        </MessageContent>
                      </Message>
                    </MessageScrollerItem>
                  ))}
                </MessageScrollerContent>
              </MessageScrollerViewport>
              <MessageScrollerButton />
            </MessageScroller>
          </CardContent>
        </MessageScrollerProvider>
      </Card>
    </div>
  )
}

/** The message scroller opened at its end (the newest message, no button) and at its start (the scroll-to-end button shown). */
export const scrollerFixtures: Record<string, () => ReactNode> = {
  "message-scroller-end": () => <Scroller position="end" />,
  "message-scroller-start": () => <Scroller position="start" />,
}
