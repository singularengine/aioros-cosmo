# Domains

Aioros guarantees the web render of what the ecosystem controls, Cosmo's themes among it (Aioros's decision "Guaranteed
scope, closed per control type", 2026-10-31): an input whose value is the page's own state is served from its plan, never
from Avalonia, so each input type Cosmo themes is closed over its domain against Avalonia's own render with the walk of
the `Aioros.Testing` package (`LocalDomains`), the one Aioros's suite runs on Fluent.

| AC | Criterion |
| --- | --- |
| DOMAIN-01 | Given each input type Cosmo themes whose value the plan holds as the page's own state, in its variants (a checkbox, a three-state checkbox, a switch, a toggle, an outline toggle, a small toggle, a single-choice toggle group and its outline variant, a select and a small select, tabs, a horizontal and a vertical slider, an input and a textarea) under Fluent and Cosmo's theme in the light and the dark theme, when `LocalDomains` sets each sampled value of its domain (each flag, each index and none, the two ends of a range and its middle, an empty, a short and a long text), then it reports nothing: the plan's render of every value is byte for byte Avalonia's, and no type is left unwalked (a refused plan, a page with no local input or a domain of one value is reported). |

Cosmo's date picker is not among them: its theme is not Fluent's, so Aioros writes it through its template (a popover
holding a calendar whose day buttons post), it holds no local input, and Avalonia answers its posts on the server
(Aioros's roadmap, 2026-10-31); its behavior on the web is covered by DATEPICKER-05 and CALENDAR-10.

