import type { ReactNode } from "react"

import { ChartAreaDefault } from "@/registry/new-york-v4/charts/chart-area-default"
import { ChartAreaGradient } from "@/registry/new-york-v4/charts/chart-area-gradient"
import { ChartAreaStacked } from "@/registry/new-york-v4/charts/chart-area-stacked"
import { ChartAreaInteractive } from "@/registry/new-york-v4/charts/chart-area-interactive"
import { ChartBarDefault } from "@/registry/new-york-v4/charts/chart-bar-default"
import { ChartBarMultiple } from "@/registry/new-york-v4/charts/chart-bar-multiple"
import { ChartBarHorizontal } from "@/registry/new-york-v4/charts/chart-bar-horizontal"
import { ChartBarLabel } from "@/registry/new-york-v4/charts/chart-bar-label"
import { ChartBarInteractive } from "@/registry/new-york-v4/charts/chart-bar-interactive"
import { ChartLineDefault } from "@/registry/new-york-v4/charts/chart-line-default"
import { ChartLineMultiple } from "@/registry/new-york-v4/charts/chart-line-multiple"
import { ChartLineDots } from "@/registry/new-york-v4/charts/chart-line-dots"
import { ChartLineInteractive } from "@/registry/new-york-v4/charts/chart-line-interactive"
import { ChartPieSimple } from "@/registry/new-york-v4/charts/chart-pie-simple"
import { ChartPieDonut } from "@/registry/new-york-v4/charts/chart-pie-donut"
import { ChartPieDonutText } from "@/registry/new-york-v4/charts/chart-pie-donut-text"
import { ChartPieLegend } from "@/registry/new-york-v4/charts/chart-pie-legend"
import { ChartRadarDefault } from "@/registry/new-york-v4/charts/chart-radar-default"
import { ChartRadarDots } from "@/registry/new-york-v4/charts/chart-radar-dots"
import { ChartRadarMultiple } from "@/registry/new-york-v4/charts/chart-radar-multiple"
import { ChartRadialSimple } from "@/registry/new-york-v4/charts/chart-radial-simple"
import { ChartRadialText } from "@/registry/new-york-v4/charts/chart-radial-text"
import { ChartRadialStacked } from "@/registry/new-york-v4/charts/chart-radial-stacked"
import { ChartTooltipDefault } from "@/registry/new-york-v4/charts/chart-tooltip-default"
import { ChartTooltipIndicatorLine } from "@/registry/new-york-v4/charts/chart-tooltip-indicator-line"
import { ChartTooltipIndicatorNone } from "@/registry/new-york-v4/charts/chart-tooltip-indicator-none"

/** shadcn/ui's chart examples as they are, drawn by Recharts 3.8.0 (or 2.15.4 in the `recharts2` build). */
export const chartFixtures: Record<string, () => ReactNode> = {
  "chart-area-default": () => <ChartAreaDefault />,
  "chart-area-gradient": () => <ChartAreaGradient />,
  "chart-area-stacked": () => <ChartAreaStacked />,
  "chart-area-interactive": () => <ChartAreaInteractive />,
  "chart-bar-default": () => <ChartBarDefault />,
  "chart-bar-multiple": () => <ChartBarMultiple />,
  "chart-bar-horizontal": () => <ChartBarHorizontal />,
  "chart-bar-label": () => <ChartBarLabel />,
  "chart-bar-interactive": () => <ChartBarInteractive />,
  "chart-line-default": () => <ChartLineDefault />,
  "chart-line-multiple": () => <ChartLineMultiple />,
  "chart-line-dots": () => <ChartLineDots />,
  "chart-line-interactive": () => <ChartLineInteractive />,
  "chart-pie-simple": () => <ChartPieSimple />,
  "chart-pie-donut": () => <ChartPieDonut />,
  "chart-pie-donut-text": () => <ChartPieDonutText />,
  "chart-pie-legend": () => <ChartPieLegend />,
  "chart-radar-default": () => <ChartRadarDefault />,
  "chart-radar-dots": () => <ChartRadarDots />,
  "chart-radar-multiple": () => <ChartRadarMultiple />,
  "chart-radial-simple": () => <ChartRadialSimple />,
  "chart-radial-text": () => <ChartRadialText />,
  "chart-radial-stacked": () => <ChartRadialStacked />,
  "chart-tooltip-default": () => <ChartTooltipDefault />,
  "chart-tooltip-indicator-line": () => <ChartTooltipIndicatorLine />,
  "chart-tooltip-indicator-none": () => <ChartTooltipIndicatorNone />,
}
