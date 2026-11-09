import type { ReactNode } from "react"

import { CodeBlockActionsExample } from "@/registry/radix/examples/code-block-actions"
import { CodeBlockAnsi } from "@/registry/radix/examples/code-block-ansi"
import { CodeBlockChat } from "@/registry/radix/examples/code-block-chat"
import { CodeBlockFloatingActions } from "@/registry/radix/examples/code-block-floating-actions"
import { CodeBlockHeight } from "@/registry/radix/examples/code-block-height"
import { CodeBlockLanguages } from "@/registry/radix/examples/code-block-languages"
import { CodeBlockMarkdown } from "@/registry/radix/examples/code-block-markdown"
import { CodeBlockRtl } from "@/registry/radix/examples/code-block-rtl"
import { CodeBlockShowMore } from "@/registry/radix/examples/code-block-show-more"
import { CodeBlockShowMoreScroll } from "@/registry/radix/examples/code-block-show-more-scroll"
import { CodeBlockStartLine } from "@/registry/radix/examples/code-block-start-line"
import { CodeBlockStreaming } from "@/registry/radix/examples/code-block-streaming"
import { CodeBlockTabs } from "@/registry/radix/examples/code-block-tabs"
import { CodeBlockTrace } from "@/registry/radix/examples/code-block-trace"
import { CodeBlockTypeset } from "@/registry/radix/examples/code-block-typeset"
import { CodeBlockWrap } from "@/registry/radix/examples/code-block-wrap"
import { QuestionnaireAnimated } from "@/registry/radix/examples/questionnaire-animated"
import { QuestionnaireCard } from "@/registry/radix/examples/questionnaire-card"
import { QuestionnaireConditional } from "@/registry/radix/examples/questionnaire-conditional"
import { QuestionnaireControlled } from "@/registry/radix/examples/questionnaire-controlled"
import { QuestionnaireDialog } from "@/registry/radix/examples/questionnaire-dialog"
import { QuestionnaireNavigationState } from "@/registry/radix/examples/questionnaire-navigation-state"
import { QuestionnaireProgressExample } from "@/registry/radix/examples/questionnaire-progress"
import { QuestionnaireResume } from "@/registry/radix/examples/questionnaire-resume"
import { QuestionnaireShortcuts } from "@/registry/radix/examples/questionnaire-shortcuts"
import { QuestionnaireSkipExample } from "@/registry/radix/examples/questionnaire-skip"
import { QuestionnaireValidation } from "@/registry/radix/examples/questionnaire-validation"
import { TraceCommit } from "@/registry/radix/examples/trace-commit"
import { TraceDemo } from "@/registry/radix/examples/trace-demo"
import { TraceDialog } from "@/registry/radix/examples/trace-dialog"
import { TraceLink } from "@/registry/radix/examples/trace-link"
import { TraceOpen } from "@/registry/radix/examples/trace-open"
import { TraceSheet } from "@/registry/radix/examples/trace-sheet"
import { TraceSources } from "@/registry/radix/examples/trace-sources"
import { TraceThinking } from "@/registry/radix/examples/trace-thinking"
import { TraceToolCalls } from "@/registry/radix/examples/trace-tool-calls"
import { TraceTurn } from "@/registry/radix/examples/trace-turn"
import { CodeBlockBasic } from "@/registry/radix/examples/code-block-basic"
import { CodeBlockDemo } from "@/registry/radix/examples/code-block-demo"
import { CodeBlockDestructive } from "@/registry/radix/examples/code-block-destructive"
import { CodeBlockDiff } from "@/registry/radix/examples/code-block-diff"
import { CodeBlockHeaderExample } from "@/registry/radix/examples/code-block-header"
import { CodeBlockHighlightedLines } from "@/registry/radix/examples/code-block-highlighted-lines"
import { CodeBlockHighlighting } from "@/registry/radix/examples/code-block-highlighting"
import { CodeBlockLineNumbers } from "@/registry/radix/examples/code-block-line-numbers"
import { CodeBlockSize } from "@/registry/radix/examples/code-block-size"
import { QuestionnaireDemo } from "@/registry/radix/examples/questionnaire-demo"
import { QuestionnaireFreeform } from "@/registry/radix/examples/questionnaire-freeform"
import { QuestionnaireMultiple } from "@/registry/radix/examples/questionnaire-multiple"
import { TraceCollapsibleDemo } from "@/registry/radix/examples/trace-collapsible"
import { TraceDescriptionDemo } from "@/registry/radix/examples/trace-description"
import { TraceGroupDemo } from "@/registry/radix/examples/trace-group"
import { TraceStatic } from "@/registry/radix/examples/trace-static"
import { TraceStatus } from "@/registry/radix/examples/trace-status"
import { TraceSteps } from "@/registry/radix/examples/trace-steps"

/** The radix base's code-block, trace and questionnaire examples in the radix-rhea style (`.style-rhea`), on new-york-v4's tokens. */
function Rhea({ children }: { children: ReactNode }) {
  return <div className="style-rhea">{children}</div>
}

const examples: Record<string, () => ReactNode> = {
  "code-block-demo": CodeBlockDemo,
  "code-block-basic": CodeBlockBasic,
  "code-block-header": CodeBlockHeaderExample,
  "code-block-line-numbers": CodeBlockLineNumbers,
  "code-block-highlighted-lines": CodeBlockHighlightedLines,
  "code-block-diff": CodeBlockDiff,
  "code-block-destructive": CodeBlockDestructive,
  "code-block-size": CodeBlockSize,
  "code-block-highlighting": CodeBlockHighlighting,
  "code-block-actions": CodeBlockActionsExample,
  "code-block-ansi": CodeBlockAnsi,
  "code-block-chat": CodeBlockChat,
  "code-block-floating-actions": CodeBlockFloatingActions,
  "code-block-height": CodeBlockHeight,
  "code-block-languages": CodeBlockLanguages,
  "code-block-markdown": CodeBlockMarkdown,
  "code-block-rtl": CodeBlockRtl,
  "code-block-show-more-scroll": CodeBlockShowMoreScroll,
  "code-block-show-more": CodeBlockShowMore,
  "code-block-start-line": CodeBlockStartLine,
  "code-block-streaming": CodeBlockStreaming,
  "code-block-tabs": CodeBlockTabs,
  "code-block-trace": CodeBlockTrace,
  "code-block-typeset": CodeBlockTypeset,
  "code-block-wrap": CodeBlockWrap,
  "trace-static": TraceStatic,
  "trace-status": TraceStatus,
  "trace-description": TraceDescriptionDemo,
  "trace-group": TraceGroupDemo,
  "trace-collapsible": TraceCollapsibleDemo,
  "trace-steps": TraceSteps,
  "questionnaire-demo": QuestionnaireDemo,
  "questionnaire-multiple": QuestionnaireMultiple,
  "questionnaire-freeform": QuestionnaireFreeform,
  "questionnaire-animated": QuestionnaireAnimated,
  "questionnaire-card": QuestionnaireCard,
  "questionnaire-conditional": QuestionnaireConditional,
  "questionnaire-controlled": QuestionnaireControlled,
  "questionnaire-dialog": QuestionnaireDialog,
  "questionnaire-navigation-state": QuestionnaireNavigationState,
  "questionnaire-progress": QuestionnaireProgressExample,
  "questionnaire-resume": QuestionnaireResume,
  "questionnaire-shortcuts": QuestionnaireShortcuts,
  "questionnaire-skip": QuestionnaireSkipExample,
  "questionnaire-validation": QuestionnaireValidation,
  "trace-commit": TraceCommit,
  "trace-demo": TraceDemo,
  "trace-dialog": TraceDialog,
  "trace-link": TraceLink,
  "trace-open": TraceOpen,
  "trace-sheet": TraceSheet,
  "trace-sources": TraceSources,
  "trace-thinking": TraceThinking,
  "trace-tool-calls": TraceToolCalls,
  "trace-turn": TraceTurn,
}

export const rheaFixtures: Record<string, () => ReactNode> = Object.fromEntries(
  Object.entries(examples).map(([name, Example]) => [name, () => <Rhea><Example /></Rhea>])
)
