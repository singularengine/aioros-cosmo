# Coverage

Every component shadcn/ui documents, at the registry pinned in `tools/shadcn` (new-york-v4, commit `c2477f3`), with its
status in Cosmo and the criteria that confirm its fidelity. `tests/Aioros.Cosmo.ArchitectureTests` checks this document:
every `done` row names criteria that exist, every documented component has exactly one row
(`tools/shadcn/registry/components.txt`), and the counts below match the tables.

Statuses: `done` (ported, with criteria that compare it with shadcn/ui), `partial` (ported with a gap named in the row),
`missing` (not ported yet), `n/a` (with the reason).

Finished: false

Counts: done 453 · partial 0 · missing 0 · n/a 4 · fidelity matrix 536 of 536 cells filled

## Components

| Component | Avalonia / Cosmo | Web | Status | Criteria |
| --- | --- | --- | --- | --- |
| `accordion` | `c:Accordion` (an `ItemsControl` of Avalonia `Expander`s: `IsMultiple`, `IsCollapsible`, `ContentTemplate`; Cosmo's `Expander` theme and `HeightTransition`) | `<details>` written through its theme | done — on the web written through its theme, a single accordion's items one exclusive group (`a:Web.Exclusive`), the height animated | ACCORDION-01..05 |
| `alert-dialog` | `c:AlertDialog` (the dialog's surface with the `alert` class and `sm`; no close button, no light dismiss, the focus on `IsCancel`) | `<dialog role="alertdialog">` shown modal for a pending command (Aioros's prompts; Escape closes it) | done — opened and closed in the browser with its entrance and exit on the web, its exit kept on the desktop while it plays; the 16 px side margin on phones | ALERTDIALOG-01..08 |
| `alert` | `c:Alert` (a `ContentControl`: `Icon`, `Title`, the content as description; `destructive`) | `<div role="alert">` boxes | done — a bound `Title` is planned on the web; the title's tracking and `role="alert"` | ALERT-01..04 |
| `aspect-ratio` | `c:AspectRatio` (a `Decorator`: `Ratio`) | the box with CSS `aspect-ratio` (`a:Web.AspectRatio`) | done — the height follows the width in any viewport | ASPECTRATIO-01..03 |
| `attachment` | `c:Attachment` (a `TemplatedControl`: `Media`, `Title`, `Description`, `Actions`, `State` as pseudo-classes; `sm`, `xs`, `vertical`, `image`) and `c:AttachmentGroup` (an `ItemsControl` scrolling sideways) | `<div>`s written through its template | done — on both targets at shadcn/ui's boxes, the trigger (`c:AttachmentTrigger`, a button over the card) and the vertical image actions included; the uploading and processing titles shimmer on both targets (THEME-09, Aioros TEXT-54) and the group fades its edges as it scrolls (THEME-10); an image covers its media on both targets | ATTACHMENT-01..05, FIDELITY-17, FIDELITY-18 |
| `avatar` | `c:Avatar` (a `ContentControl`: `Source`, the content as fallback, `Badge`; `sm`, `lg`), `c:AvatarBadge`, `c:AvatarGroup` (an `ItemsControl` overlapping its avatars) and `c:AvatarGroupCount` | `<img alt>` / fallback `<div>` | done — a bound `Source` or content is planned on the web; the image is clipped to the circle | AVATAR-01..06 |
| `badge` | `c:Badge` (`ContentControl`), and a `HyperlinkButton` with the `CosmoBadgeLink` theme as a link | `<div>` box, `<a href>` for a link | done | BADGE-01..05 |
| `breadcrumb` | `c:Breadcrumb` (an `ItemsControl` of `c:BreadcrumbItem`s with `SeparatorTemplate`), `HyperlinkButton` links, `c:BreadcrumbPage`, `c:BreadcrumbEllipsis` | `<nav>` of an `<ol>` of `<li>` items, `<a href>` links | done — the page carries `aria-current="page"` on the web (`a:Web.Current`); bound crumbs are links with their bound addresses and their separators by position | BREADCRUMB-01..04 |
| `bubble` | `c:Bubble` (a `ContentControl`: `Reactions`; `secondary`, `muted`, `tinted`, `outline`, `ghost`, `destructive`, `end`, `reactions-start`) and `c:BubbleGroup` | `<div>`s, the 80% limit as a CSS grid of star columns | done — the variants, alignment, groups and reactions on both targets; a bubble whose content is a link or a button (shadcn/ui's `asChild`) with its hover look, the collapsible bubble (`c:Collapsible`, switched in the browser with no request) and the markdown bubble (`c:Markdown`, in the `Aioros.Cosmo.Markdown` package) | BUBBLE-01..05, CODEBLOCK-08, CODEBLOCK-09, FIDELITY-17, FIDELITY-18 |
| `button-group` | `c:ButtonGroup` (a `StackPanel` joining its children by styles; `Orientation`), `c:ButtonGroupText`, `Separator` | a flex row | done | BUTTONGROUP-01..04 |
| `button` | Avalonia `Button`, `HyperlinkButton` (Cosmo control theme) | `<button>`, `<a href>` | done | BUTTON-01..15 |
| `calendar` | Avalonia `Calendar` with Cosmo's themes (`Calendar`, `CalendarItem`, `CalendarDayButton`, `CalendarButton`), `c:CalendarDays` (only the month's weeks, the range's ends and band, `outside-hidden`) and `c:CalendarSelection` (`RangeStart`, `RangeEnd`, `Dates` two way) | a `role="grid"` month written through Cosmo's theme | done — at shadcn/ui's boxes within 1 px on the desktop and 98% to 99.7% of pixels on the web, navigation and selection posted on the web; a range with two clicks, several days toggled, week numbers and the dropdown caption on both targets; a bound calendar planned on the web, in the visitor's culture | CALENDAR-01..13, FIDELITY-11 |
| `card` | `c:Card`, `c:CardHeader`, `c:CardContent`, `c:CardFooter` | `<div>` boxes | done | CARD-01..06 |
| `carousel` | Avalonia `Carousel` with Cosmo's implicit theme (the previous and next buttons, `vertical`, `align-start`, `indicators`, Left and Right, `WrapSelection` as the loop, swipe) and `c:CarouselIndicators` | a WAI-ARIA carousel: a scroll-snap strip along its axis with its template's buttons and indicators | done — on the web the bound example's `Slide N of M` follows the slides, a click on an indicator and the added slides; the buttons, indicators, vertical orientation, loop, keys and square slides are written | CAROUSEL-01..07 |
| `chart` | `Aioros.Cosmo.Charts`: Aioros.Pictor's charts (Recharts 3.8.0's behavior where shadcn/ui's examples show it) in Cosmo's themes, `c:ChartConfig`, `c:ChartTooltipContent`, `c:ChartLegendContent` and `c:ChartLegendItem`, `c:CosmoChartsTheme` | SVG with HTML tooltips and legends | done — the 26 examples on both targets (96.5% to 98.9% of pixels on the desktop and 97.2% to 99.9% on the web against Recharts 3.8.0), a tooltip row per line, the dashed indicator dashed, formatter converters | CHART-01..11, FIDELITY-13, FIDELITY-14 |
| `checkbox` | Avalonia `CheckBox` (Cosmo control theme on Fluent's lightweight-styling keys; `IsThreeState` is `indeterminate`) | `<input type=checkbox>` in a `<label>` (three-state: Aioros's three radios) | done | CHECKBOX-01..05 |
| `code-block` | `c:CodeBlock` (a `ContentControl`: `Title`, `Actions`, `Footer`; `destructive`, `sm`), `c:CodeBlockContent` (`LineNumbers`, `StartLine`, `Wrap`), `c:CodeBlockLine` (`Number`; `highlighted`, `added`, `removed`) and `c:CodeBlockCopyButton` (`Value`), from the radix base in the radix-rhea style; the code highlighted by shiki when the XAML is written, its tokens `Run`s on Cosmo's code brushes (github-light, github-dark) | `<div>`s written through their templates, the tokens colored spans; no highlighter in the browser | done — the 25 examples on both targets (93.6% to 98.5% of pixels on the desktop, 99.8% to 100% on the web): floating actions, show more, wrap toggle, start line, tabs, ANSI, right to left, typeset, markdown (`c:Markdown`, highlighted at build time by `ShikiTokens`), heights and scroll fades; the copy button copies on both targets; the streaming example is shown in its finished state (decisions: Streaming examples shown at rest) | CODEBLOCK-01..09, FIDELITY-19, FIDELITY-20 |
| `collapsible` | Avalonia `Expander` with the `collapsible` class (`CosmoCollapsible`) | `<details>` | done — on the web a `<details>` written through its theme: the header's 32 px ghost toggle with its icon and the 8 px gap | COLLAPSIBLE-01..03 |
| `combobox` | `c:Combobox` (a sealed `SelectingItemsControl`: the outline trigger, a `Flyout` with a `c:Command` over its items; `SelectedItem`, `SelectedItems`, `SelectionMode`, `IsDropDownOpen` two way) | a button opening a popover written from the flyout's presenter | done — on the web the popover 200 px as its trigger with the options' texts at the items' padding (the command sized from the trigger's declared width); the roles, filtering, ranking, keys and a posted choice are written, chosen in the browser with no request, its index a field of the form | COMBOBOX-01..06 |
| `command` | `c:Command` (a sealed `ItemsControl`: cmdk's scoring, ranking, selection and keys; `Text`, `SelectedValue` two way), `c:CommandGroup`, `c:CommandItem` (a sealed `Button`), `c:CommandSeparator`, `c:CommandDialog` (`IsOpen` two way, `Gesture`) | an `<input>` over the items' buttons (written from their template: padding, selection, shortcuts) | done — on the web the WAI-ARIA combobox over a listbox, filtered, ranked and selected in the browser with cmdk's keys (`a:Web.Filter`), the dialog's `Gesture` pressing its `Trigger` or, without one, opening the dialog in the browser, and the chosen item posted with the text | COMMAND-01..12 |
| `context-menu` | Avalonia `ContextMenu` (Cosmo's `ContextMenu` and `MenuItem` themes; Shift+F10) | a `role="menu"` popover opened at the pointer by a right click, Shift+F10 or the menu key | done | CONTEXTMENU-01..07 |
| `data-table` | Avalonia `TableView` with columns in XAML (header controls, cell templates, bound `IsVisible`) over `DataTableSource<T>` (filter, sort, paging, selection, column visibility for the view model; `IDataTableRow`, `DataTableColumn`) and `c:DataTable.SelectsRows` | a `<table>` with its header controls and checkbox inputs, posting on change | done — on the web the header controls, selected rows, header look and row borders, column visibility (through `Source.Columns[n]`) and posting on change with a delayed filter are written, the header row at its 40 px | DATATABLE-01..06 |
| `date-picker` | Avalonia `CalendarDatePicker` with Cosmo's theme: the outline button with the calendar icon and the date, a popover with Cosmo's calendar, and the `input` variant | Cosmo's theme through its template: the button opens the popover in the browser, a chosen day or typed text is posted | done — on the desktop at shadcn/ui's boxes within 1 px (the popover and its days within 1.5 px), on the web 95% to 99.6% of pixels; the date as date-fns's `PPP` (`June 12th, 2025`), the two-month range and the presets on both targets | DATEPICKER-01..06, FIDELITY-11 |
| `dialog` | `c:Dialog` (`IsOpen` two way; `c:DialogSurface`, `c:DialogHeader`, `c:DialogFooter`, `c:Dialog.Closes`; the mask and surface in the nearest `c:OverlayHost`; focus trap and return, Escape, light dismiss) | `<dialog>` shown modal for a pending command (Aioros's prompts; Escape and a click outside close it) | done — opened and closed in the browser with its entrance and exit on the web, its exit kept on the desktop while it plays; the 16 px side margin on phones | DIALOG-01..09 |
| `direction` | — | — | n/a — Avalonia's `FlowDirection` is the direction provider; there is nothing to port |  |
| `drawer` | `c:Drawer` (the dialog's surface with the `drawer` class: bottom, rounded top, the handle, 80% at most) | `<dialog>` shown modal for a pending command | done — on both targets it slides up and down and is dragged down to close as vaul's (on the web since) | DRAWER-01..11 |
| `dropdown-menu` | Avalonia `MenuFlyout` on `Button.Flyout` (Cosmo's `MenuFlyoutPresenter`, `MenuItem` and menu `Separator` themes: labels, checkbox and radio items, `inset`, `destructive`, shortcuts, submenus; Radix's keyboard and typeahead) | `<div popover role="menu">` of `menuitem`s with the APG keys, nested popovers for submenus | done | DROPDOWN-01..08 |
| `empty` | `c:Empty` (a `ContentControl`: `Media`, `Title`, `Description`, the content as actions; `outline`) and `c:EmptyMedia` (`icon`) | `<div>` boxes | done — `text-balance` on both targets (CSS's `text-wrap:balance` on the web, Cosmo's balancing on the desktop); the dashed outline and the title's tracking are written | EMPTY-01..03, TYPOGRAPHY-05, TYPOGRAPHY-06 |
| `field` | `c:Field` (a sealed `Grid` of its shown parts: `Orientation`, `Spacing`), `c:FieldGroup` (a container), `c:FieldSet`, `c:FieldContent` (sealed `StackPanel`s), `c:FieldLabel` (a `Label`), `c:FieldTitle`, `c:FieldLegend`, `c:FieldDescription`, `c:FieldError` (sealed `TextBlock`s); `DataValidationErrors` themed as `FieldError` | `<div>`s, `<fieldset>` and `<legend>`, `<label for>`, `role=alert` errors | done — the content column, title, responsive orientation and field set element; `c:FieldSeparator` | FIELD-01..16 |
| `hover-card` | `c:HoverCard.Card` on any control (a `Flyout` with the `hover-card` presenter class, opened on hover and keyboard focus after Radix's delays) | a `popover="hint"` opened on hover and focus after Radix's delays | done | HOVERCARD-01..07 |
| `input-group` | `c:InputGroup` (an `ItemsControl` in a grid) and `c:InputGroupAddon` (`inline-end`, `block-end`) | `<div>`s with an `<input>` or `<textarea>` | done | INPUTGROUP-01..04 |
| `input` | Avalonia `TextBox` (Cosmo control theme); Aioros's `a:FilePicker` in Cosmo's file input theme | `<input>`; a `<label>` around a hidden file input | done — the file input | INPUT-01..08 |
| `input-otp` | `c:InputOtp` (`Length`, `GroupSize`, two-way `Text`; boxes over a hidden `TextBox`) | box `<div>`s over one `<input>`, filled and marked by the framework's script while typing | done | INPUTOTP-01..04 |
| `item` | `c:Item` (a `ContentControl`: `Media`, `Title`, `Description`, `Actions`, `Header`, `Footer`; `outline`, `muted`, `sm`, `link`), `c:ItemMedia` (`icon`, `image`), `c:ItemGroup`, `c:ItemHeader`, `c:ItemFooter`, `c:ItemSeparator` | `<div>` boxes (`<a>` with `a:Web.Href`) | done | ITEM-01..08 |
| `kbd` | `c:Kbd` (a `ContentControl`) and `c:KbdGroup` (a `StackPanel`) | `<kbd>` | done | KBD-01..06 |
| `label` | Avalonia `Label` (Cosmo control theme) | `<label for>` | done | LABEL-01..05 |
| `marker` | `c:Marker` (a `ContentControl`: `Icon`; `separator`, `border`, `vertical`) | `<div>`s written through its template | done — the variants and icons on both targets; a status marker's text shimmers (THEME-09, Aioros TEXT-54) and a marker can be a link or a button (`c:MarkerLink`, `c:MarkerButton`, shadcn/ui's `asChild`), on the web an `<a href>` through its template (Aioros BUTTONS-120) | MARKER-01..04, FIDELITY-17, FIDELITY-18 |
| `menubar` | Avalonia `Menu` with Cosmo's themes (the bar, `CosmoMenubarItem` top-level items, Cosmo's menu items) | `<menu>` toolbar of popover menus | done — the items' padding and open look, the menus' offsets, padding, width and shadow and the bound checkable items on the web | MENUBAR-01..08 |
| `message` | `c:Message` (a `ContentControl`: `Avatar`; `end`, `:footer`), `c:MessageContent`, `c:MessageHeader`, `c:MessageFooter`, `c:MessageGroup` | `<div>`s written through its template | done — the avatar, header, footer, alignment and groups on both targets | MESSAGE-01, MESSAGE-02, FIDELITY-17, FIDELITY-18 |
| `message-scroller` | `c:MessageScroller` (a `ContentControl` on `@shadcn/react`'s `MessageScroller` 0.3.1: `DefaultScrollPosition`, `ScrollToEnd()`, `ScrollToStart()`, `:end`, `:following`; a Cosmo scroll area and the `icon-sm` scroll-to-end button, `CosmoMessageScrollerButton`, in its template) | a scroll container `<div>` and the button, written through its template | done — on both targets it opens at its end or its start, follows new messages from its end and its button scrolls to the end, hidden there (on the web); its bottom fades while there is more below on both targets (MESSAGESCROLLER-05, MESSAGESCROLLER-06); the `last-anchor` opening, `scrollToMessage` and the visibility hooks are not ported | MESSAGESCROLLER-01..06, FIDELITY-17, FIDELITY-18 |
| `native-select` | Avalonia `ComboBox` with the `native` class | `<select>` | done | NATIVESELECT-01..02, SELECT-01 |
| `navigation-menu` | `c:NavigationMenu` (a sealed `StackPanel`), `c:NavigationMenuTrigger` (a sealed `Button` whose `Flyout` is the content), links with the `navigation-menu-link` and `navigation-menu-trigger` classes | a `<nav>` of buttons (with their padding and chevrons) opening anchored popovers | done — the open trigger's look, the links' alignment and clamp, hover opening with Radix's delays and an item template's link posting on the web | NAVMENU-01..09 |
| `pagination` | `c:Pagination` (an `ItemsControl` of `c:PaginationItem`s), `c:PaginationPrevious`, `c:PaginationNext`, `c:PaginationEllipsis`, page buttons as `ghost icon` / `outline icon` buttons | a `<nav>` holding a `<ul>` of `<li>` items, buttons posting the model's commands | done — the list and the current page's `aria-current="page"` on the web | PAGINATION-01..06 |
| `popover` | Avalonia `Flyout` (`Button.Flyout`, Cosmo's `FlyoutPresenter` theme; `c:Popover.IsOpen` two way) | `<div popover>` anchored to a `<button popovertarget>`, written from the presenter's theme | done — a bound `c:Popover.IsOpen` is planned on the web | POPOVER-01..08 |
| `progress` | Avalonia `ProgressBar` (Cosmo control theme) | `<progress>` | done | PROGRESS-01..03 |
| `questionnaire` | `c:Questionnaire` (an `ItemsControl`: `CurrentIndex` two way, `ShowProgress`, `Shortcuts`, the actions' contents, `Submitted`, `SubmitCommand`), `c:QuestionnaireItem` (`Title`, `Description`, `IsRequired`, `Status`, `IsInvalid`), `c:QuestionnaireChoice` (a `RadioButton`), `c:QuestionnaireCheck` (a `CheckBox`) and `c:QuestionnaireInput` (a `TextBox`), from the radix base in the radix-rhea style on `@shadcn/react`'s primitive | the progress and the question as `<div>`s, the choices native `<input type=radio>` and `<input type=checkbox>` in their labels, the actions buttons that post | done — the 14 examples on both targets (94.7% to 97.5% of pixels on the desktop, 99.3% to 100% on the web): progress formats and segments, `RequiresAnswer` (in the browser with no request), reset, conditional, controlled, resumed, validated, animated, card and dialog questionnaires, and shadcn/ui's keys (Ctrl+Enter, the arrows, Enter on an answer; on the web through Aioros's key scopes, ArrowRight on an unanswered question posting Next, which shows its error); every step posts on the web, its place and answers carried in hidden inputs | QUESTIONNAIRE-01..10, FIDELITY-19, FIDELITY-20 |
| `radio-group` | Avalonia `RadioButton`s in a panel (Cosmo control theme on Fluent's keys; Radix's arrow keys) | `<input type=radio>`s in `<label>`s | done | RADIO-01..05 |
| `resizable` | `c:ResizablePanelGroup` (a sealed `Grid`), `c:ResizablePanel` (percentages: `DefaultSize`, `Size` two way, `MinSize`, `MaxSize`, `IsCollapsible`), `c:ResizableHandle` (Avalonia's `GridSplitter`: `WithHandle`, the library's keys) | a CSS grid with WAI-ARIA window splitters | done — on the web a drag or a key on a handle next to a bound panel posts its position to the bound `Size` once it ends (`a:Web.Delay` from Cosmo); the grip, dragging, shadcn's 5% steps (`a:Web.ResizeStep`), Enter collapsing, the limits and `aria-valuenow` | RESIZABLE-01..06 |
| `scroll-area` | Avalonia `ScrollViewer` with the `scroll-area` class (Cosmo's `CosmoScrollArea` theme, `c:ScrollAreaBars`) | one framed box that scrolls, with thin bars | done — the area scrolls with its frame, its content at its width and thin bars on the web; the bars show under the pointer and hide 600 ms after it leaves on both targets | SCROLLAREA-01..05 |
| `select` | Avalonia `ComboBox` (trigger, drop-down, `label` and `separator` items, `c:SelectScrollUpButton` and `c:SelectScrollDownButton`) | `<select>` | done — the list enters and leaves on both targets; on the web the drop-down is the customizable select where the browser supports it (shown, ITEMS-136), the browser's own list elsewhere | SELECT-01..13, NATIVESELECT-01..02 |
| `separator` | Avalonia `Separator` (Cosmo control theme; `vertical`) | `<hr>` | done | SEPARATOR-01..03 |
| `sheet` | `c:Sheet` (`Side`: right, left, top, bottom; the dialog's surface with the `sheet` class) | `<dialog>` shown modal for a pending command, the surface at its side | done — each side slides in and out on both targets, opened and closed in the browser on the web | SHEET-01..08 |
| `sidebar` | `c:SidebarProvider` (a sealed `Panel`: `IsOpen`, `IsMobileOpen` two way, `MobileBreakpoint`, `ToggleGesture`, `StateStore`), `c:Sidebar` (`Side`, `Variant`, `Collapsible`; the mobile sheet), `c:SidebarHeader`, `c:SidebarContent`, `c:SidebarGroup`, `c:SidebarGroupLabel`, `c:SidebarMenu`, `c:SidebarMenuItem`, `c:SidebarMenuButton`, `c:SidebarMenuSub`, `c:SidebarMenuSubButton`, `c:SidebarMenuAction`, `c:SidebarMenuBadge`, `c:SidebarFooter`, `c:SidebarTrigger`, `c:SidebarRail`, `c:SidebarSeparator`, `c:SidebarInset` | a dock of the sidebar beside its inset, folded in the browser | done — on both targets: on the web the layout, collapsing with the trigger and the rail, the icon mode and Ctrl+B, the icon tooltips and a bound `IsOpen` posted, the state kept in the `sidebar_state` cookie and the mobile sheet | SIDEBAR-01..11, SIDEBAR-13, SIDEBAR-15 |
| `skeleton` | `c:Skeleton` (a `TemplatedControl` pulsing in `--accent`) | `<div>` box with a CSS animation | done | SKELETON-01..03 |
| `slider` | Avalonia `Slider` (Cosmo control theme; vertical) and `c:RangeSlider` (two thumbs: `LowerValue`, `UpperValue`) | `<input type=range>`, two in a named group for a range | done — the range with two thumbs (RANGES-30 to RANGES-33) | SLIDER-01..08 |
| `sonner` | `c:Toaster` (an Avalonia `WindowNotificationManager` with Cosmo's `NotificationCard` theme and `c:ToastDeck`, sonner's stack) and `c:Toast` (kinds, action, `Resolve` for promises) | notices from Cosmo's toast theme at the toaster's corner, fading after 4 s, the action posting its command | done — on the web a deck the browser keeps as sonner's stack: expanding under the pointer, pausing, moving as toasts come and go, swiped and closed by its close button (since) | SONNER-01..13 |
| `spinner` | `c:Spinner` (Lucide's `loader-circle`, turning, named Loading, polite) | inline SVG named Loading, `role="status"` | done | SPINNER-01..03 |
| `switch` | Avalonia `ToggleSwitch` (Cosmo control theme on Fluent's keys; `sm`) | `<input type=checkbox role=switch>` | done | SWITCH-01..05 |
| `table` | Avalonia `TableView` with Cosmo's themes, `c:TableFooter`, `c:TableCaption` | `<table>` | done — on the web the header's look, height and line, the rows' borders and the star columns' shares (as percentages) are shadcn/ui's | TABLE-01..04 |
| `tabs` | Avalonia `TabControl` and `TabItem` (Cosmo control themes; `TabsKeyboard`: loop, Home, End, automatic activation) | a WAI-ARIA tablist in its strip, with its panels | done — the strip, the selected tab's look, the disabled tab's opacity, the roles and the keys on the web | TABS-01..11 |
| `textarea` | Avalonia `TextBox` with `AcceptsReturn` | `<textarea>` | done | TEXTAREA-01..02, INPUT-04, INPUT-05 |
| `toast` | — | — | n/a — deprecated by shadcn/ui in favor of Sonner (`sonner`) |  |
| `toggle-group` | Avalonia `ListBox` with the `toggle-group` class (`outline`, `sm`, `lg`, `spacing`) | `role=group`/`radiogroup` of `<label>`s | done | TOGGLEGROUP-01..05 |
| `toggle` | Avalonia `ToggleButton` (Cosmo control theme; `outline`, `sm`, `lg`) | `<label>` with a hidden checkbox | done | TOGGLE-01..04 |
| `tooltip` | Avalonia `ToolTip.Tip` (Cosmo's `ToolTip` theme with the arrow; Radix's delays, keyboard focus and Escape) | `role=tooltip` from Cosmo's template with its arrow, placement, `aria-describedby`, Escape and the skip delay | done | TOOLTIP-01..07 |
| `trace` | `c:Trace` (a `StackPanel`), `c:TraceLine` (a `ContentControl`: `Icon`, `Description`, `Trailing`, `Status` as pseudo-classes) and Avalonia's `Expander` with the `trace` class (Cosmo's `CosmoTraceCollapsible` theme), from the radix base in the radix-rhea style | `<div>`s and a `<details>` per collapsible step | done — the 16 examples on both targets (96.2% to 99.8% of pixels on the desktop, 99.0% to 100% on the web): links, dialogs and sheets opened in the browser, sources, commits, turns with error steps, an outside button opening a step; the running title shimmers (TRACE-04, TRACE-05); an open step's content and summary since; the thinking, tool-call and demo examples are shown in their first state (decisions: Streaming examples shown at rest) | TRACE-01..07, FIDELITY-19, FIDELITY-20 |
| `typography` | `TextBlock` classes with `typography` (`h1`…`h4`, `p`, `lead`, `large`, `small`, `muted`), `Border` classes (`blockquote`, `inline-code`), `c:List` | `<h1>`…`<h4>`, `<p>`, `<small>`, `<ul>` of `<li>`, `<blockquote>`, `<code>` | done — `p`, `lead` and `muted` are `<p>`s, `small` a `<small>` and `c:List` a `<ul>` (`a:Web.Tag`); `a:Web.TextWrap="balance"` balances lines on both targets; tracking, italic and the other elements | TYPOGRAPHY-01..06 |

## Button

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Variants | `default`, `destructive`, `outline`, `secondary`, `ghost`, `link` as classes | done | BUTTON-02, BUTTON-03, BUTTON-05 |
| Sizes | default (36), `sm` (32), `lg` (40), `icon` (36 × 36) | done | BUTTON-03, BUTTON-04 |
| Extra sizes | `xs` (24), `icon-xs`, `icon-sm`, `icon-lg` | done — the `button-extra-sizes` reference fixture on both targets, an xs button's icon 12 px (`a:IconStyle.Size`) | BUTTON-04, BUTTON-08, BUTTON-14, BUTTON-15 |
| Icon and text | `gap-2` (`gap-1.5` sm, `gap-1` xs), `has-[>svg]:px-3` as `has-icon` | done | BUTTON-04, BUTTON-08 |
| Hover | `hover:bg-primary/90`, `hover:bg-accent`, `dark:hover:bg-input/50`… | done | BUTTON-02, BUTTON-05 |
| Focus ring | `focus-visible:ring-[3px] ring-ring/50 border-ring` | done | BUTTON-06, BUTTON-10 |
| Disabled | `disabled:opacity-50 pointer-events-none` | done | BUTTON-03, BUTTON-07 |
| Link hover underline | `link` variant: `hover:underline` | done | BUTTON-11 |
| Outline shadow | `shadow-xs` on `outline` | done | BUTTON-06, BUTTON-11 |
| Transition | `transition-all` (150 ms) | done | BUTTON-11 |
| Invalid state | `aria-invalid:border-destructive ring-destructive/20` | done — a button with an error (`:error`) or that is the control of an invalid `c:Field` takes the destructive border (kept `--input` on dark outlines, as shadcn/ui's `dark:border-input` wins) and ring on both targets | BUTTON-12, BUTTON-13 |
| As a link | `asChild` around a link | done | BUTTON-09 |
| Command | `onClick` | done | BINDING-01, BINDING-02, BINDING-04 |
| Real element | `<button>` | done | BUTTON-01 |

## Badge

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Variants | `default`, `secondary`, `destructive`, `outline`, `ghost`, `link` | done | BADGE-01, BADGE-02 |
| Box | `px-2 py-0.5 text-xs font-medium rounded-full border` | done | BADGE-01, BADGE-02 |
| Icon and text | `gap-1`, `[&>svg]:size-3` | done | BADGE-03 |
| Customization | `className` (count badges, custom colors) | done | BADGE-01 |
| Link badge hover | `[a&]:hover:bg-primary/90` | done — a `HyperlinkButton` with the `CosmoBadgeLink` theme is an `<a href>` with the badge's look in each variant, its hover (90% fills, `--accent` for `outline` and `ghost`, an underline for `link`) and focus ring on both targets | BADGE-04, BADGE-05 |
| Bound variant | (React props) | done | BINDING-03, BINDING-06 |

## Card

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Card | `flex flex-col gap-6 rounded-xl border bg-card py-6 shadow-sm` | done | CARD-01, CARD-02 |
| CardHeader, CardTitle, CardDescription | `grid gap-2 px-6`, `leading-none font-semibold`, `text-sm text-muted-foreground` | done | CARD-02, CARD-03 |
| CardAction | `col-start-2 row-span-2 self-start justify-self-end` | done | CARD-01, CARD-02 |
| CardContent, CardFooter | `px-6`, `flex items-center px-6` | done | CARD-01, CARD-02 |
| Separated header and footer | `[.border-b]:pb-6`, `[.border-t]:pt-6` | done — the `border` class on `c:CardHeader` and `c:CardFooter` rules them off with a 1 px `--border` line 24 px from their content, on both targets | CARD-04, CARD-05 |
| Bound parts | (React props) | done | BINDING-01, BINDING-02 |

## Label

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Look | `text-sm leading-none font-medium select-none` | done | LABEL-01, LABEL-02 |
| Line height | `leading-none` (14 px) | done — both targets (on the web) | LABEL-02, LABEL-05 |
| `htmlFor` | the label's control | done | LABEL-01, LABEL-02 |
| Disabled | `peer-disabled:opacity-50`, `group-data-[disabled=true]:opacity-50` | done | LABEL-02 |
| Bound content | (React children) | done | LABEL-03, LABEL-04 |

## Input

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Box | `h-9 rounded-md border border-input px-3 py-1 md:text-sm`, `dark:bg-input/30` | done | INPUT-02, INPUT-04 |
| Placeholder and selection | `placeholder:text-muted-foreground`, `selection:bg-primary selection:text-primary-foreground` | done | INPUT-02, INPUT-04 |
| Shadow | `shadow-xs` | done — on the web (TEXT-34) | INPUT-04 |
| Focus ring | `focus-visible:border-ring focus-visible:ring-[3px] ring-ring/50` | done | INPUT-03, INPUT-04 |
| Invalid | `aria-invalid:border-destructive ring-destructive/20` | done — `aria-invalid` and `aria-describedby` on the web (Aioros TEXT-37) | INPUT-04, FIELD-04 |
| Disabled | `disabled:opacity-50` | done | INPUT-02, INPUT-04 |
| Transparent background | `bg-transparent` | done | INPUT-02 |
| Types | `type="email"`, `password`… | done | INPUT-01 |
| File input | `file:` button styles | done — Aioros's `a:FilePicker` takes Cosmo's implicit file input theme on both targets: the input's box, a medium "Choose File" and the chosen name in place of "No file chosen" | INPUT-07, INPUT-08 |
| Two-way text | `value` / `onChange` | done | INPUT-05, INPUT-06 |

## Textarea

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Box | `field-sizing-content min-h-16 px-3 py-2` | done | TEXTAREA-01, TEXTAREA-02 |
| Growing with content | `field-sizing-content` | done | TEXTAREA-01, TEXTAREA-02 |
| States | focus, invalid, disabled as Input | done | INPUT-04, TEXTAREA-01 |

## Field

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Field, vertical and horizontal | `flex-col gap-3`, `flex-row items-center` | done | FIELD-01, FIELD-02 |
| FieldGroup, nested groups | `gap-7`, `[&>[data-slot=field-group]]:gap-4` | done | FIELD-01 |
| FieldSet and FieldLegend | `gap-6`, `mb-3 font-medium text-base`, `variant="label"` | done | FIELD-01, FIELD-02 |
| FieldDescription | `text-sm leading-normal text-muted-foreground`, `nth-last-2:-mt-1`, `last:mt-0` | done — a description before a field's last part stands 4 px closer to the part above it, on both targets | FIELD-01, FIELD-02, FIELD-15, FIELD-16 |
| FieldError | `text-sm text-destructive`, `role=alert` | done | FIELD-01, FIELD-02, FIELD-03 |
| Errors of Avalonia's data validation | (`errors` of the form library) | done — the theme's 14 px message 12 px below the field on both targets (Aioros TEXT-38) | FIELD-03, FIELD-04 |
| Invalid field | `data-invalid` → `text-destructive` | done | FIELD-01, FIELD-04, FIELD-05 |
| Responsive orientation | `orientation="responsive"` (container queries) | done — `c:Field.responsive` turns into a row when its `c:FieldGroup` (a container) is at least 448 px wide, on both targets | FIELD-09, FIELD-12 |
| FieldSeparator | `-my-2 h-5`, a `Separator` at its middle, the content `bg-background px-2 text-muted-foreground` | done — `c:FieldSeparator` (`card` for `bg-card`) on both targets | FIELD-06, FIELD-07 |
| FieldContent, FieldTitle | content column, title | done — `c:Field` lays its shown parts out as a grid, so a `c:FieldContent` (or else a label) takes the row its control leaves and a taller field keeps its parts at its top; the control stands at the row's top beside a content column | FIELD-08, FIELD-11 |
| Field set element | `<fieldset>`, `<legend>` | done — `c:FieldSet` is a `<fieldset>` and its `c:FieldLegend` a `<legend>` through `a:Web.Tag` (Aioros SEO-47, SEO-48) | FIELD-10 |

## Select

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Placeholder | `data-[placeholder]:text-muted-foreground` | done — an empty first option in `--muted-foreground` on the web (Aioros ITEMS-94) | SELECT-01 |
| Content | `bg-popover border rounded-md shadow-md p-1`, 4 px from the trigger, 10 px from the viewport's edges (Radix's `collisionPadding`) | done — the customizable select (`appearance: base-select`) styled from the popup on the web where the browser supports it (Aioros ITEMS-97), the browser's list elsewhere; `c:Collision.Padding` keeps the list 10 px from the window's edges on the desktop and `a:Web.CollisionPadding` from the viewport's on the web (Aioros ITEMS-142) | SELECT-02, SELECT-11, SELECT-12 |
| Items, check indicator | `py-1.5 pr-8 pl-2 rounded-sm`, `CheckIcon` at `right-2` | done — as the content (Aioros ITEMS-97); on the web the check is the item template's, drawn where it stands (Aioros ITEMS-139) | SELECT-02, SELECT-09 |
| Label and separator | `px-2 py-1.5 text-xs text-muted-foreground`, `-mx-1 my-1 h-px bg-border` | done — a disabled option and an `<hr>` on the web (Aioros ITEMS-96) | SELECT-02 |
| Keyboard and typeahead | Radix Select | done | SELECT-03 |
| Scroll buttons | `SelectScrollUpButton`, `SelectScrollDownButton` | done — `c:SelectScrollUpButton` and `c:SelectScrollDownButton` in the popup's template, the list 384 px high at most with no scroll bar: each shown only while the list can scroll its way and scrolling by an item every 50 ms while pointed at, on both targets (on the web sticky elements of the customizable select's picker, Aioros ITEMS-134, ITEMS-135; the list opened with its selected item scrolled into view the nearest way, ITEMS-141) | SELECT-07..10 |
| Native select | `NativeSelect`, `NativeSelectOption` | done | NATIVESELECT-01, NATIVESELECT-02 |
| Disabled | `disabled:opacity-50` | done | SELECT-01, NATIVESELECT-02 |
| Items from the model | (React children) | done — templates on the desktop, the template's text in each option on the web (Aioros ITEMS-95) | SELECT-04, NATIVESELECT-01 |
| Animation | `fade-in-0 zoom-in-95` (150 ms), `animate-out` on close | done — the list enters on both targets and leaves on the web (the customizable select's picker) and on the desktop as a picture of it in the window's overlay layer, passing clicks through (`c:Motion`) | SELECT-05, SELECT-06, SELECT-13 |

## Checkbox

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Box | `size-4 rounded-[4px] border border-input shadow-xs`, `dark:bg-input/30` | done | CHECKBOX-02, CHECKBOX-03 |
| Checked | `bg-primary border-primary text-primary-foreground`, `CheckIcon size-3.5` | done — on the web from the theme (Aioros BUTTONS-86) | CHECKBOX-03 |
| Indeterminate | Radix `checked="indeterminate"` | done | CHECKBOX-01, CHECKBOX-02, CHECKBOX-03 |
| Focus ring | `focus-visible:ring-[3px] ring-ring/50 border-ring` | done — on the web from the theme (Aioros BUTTONS-86) | CHECKBOX-03 |
| Disabled | `disabled:opacity-50` | done | CHECKBOX-02, CHECKBOX-03 |
| Label beside it | `Label` with `gap-3` | done | CHECKBOX-02, CHECKBOX-03 |
| Two-way checked, command | `checked` / `onCheckedChange` | done | CHECKBOX-04, CHECKBOX-05 |

## Radio Group

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Item | `size-4 rounded-full border border-input shadow-xs`, `CircleIcon size-2 fill-primary` | done | RADIO-02, RADIO-03 |
| Group | `grid gap-3` | done | RADIO-01, RADIO-03 |
| Focus ring | `focus-visible:ring-[3px]` | done — on the web from the theme (Aioros BUTTONS-87) | RADIO-03 |
| Keyboard | arrows check the next and previous item | done | RADIO-04 |
| Disabled | `disabled:opacity-50` | done | RADIO-02, RADIO-03 |
| Two-way value | `value` / `onValueChange` | done | RADIO-04, RADIO-05 |

## Switch

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Track and thumb | `h-[1.15rem] w-8`, `size-4` thumb, `sm` | done | SWITCH-02, SWITCH-03 |
| Colors | `bg-primary` / `bg-input`, thumb `bg-background` | done | SWITCH-02, SWITCH-03 |
| Focus ring | `focus-visible:ring-[3px]` | done — on the web from the theme (Aioros BUTTONS-88) | SWITCH-03 |
| Disabled | `disabled:opacity-50` | done | SWITCH-02, SWITCH-03 |
| Two-way checked | `checked` / `onCheckedChange` | done | SWITCH-04, SWITCH-05 |

## Toggle

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Variants and sizes | default, `outline`; `sm`, default, `lg` | done | TOGGLE-02, TOGGLE-03 |
| On, hover | `data-[state=on]:bg-accent`, `hover:bg-muted hover:text-muted-foreground` | done | TOGGLE-02, TOGGLE-03 |
| Focus ring | `focus-visible:ring-[3px]` | done — on the web from the theme (Aioros BUTTONS-89) | TOGGLE-03 |
| Disabled | `disabled:opacity-50` | done | TOGGLE-02, TOGGLE-03 |
| Two-way pressed, command | `pressed` / `onPressedChange` | done | TOGGLE-04 |

## Toggle Group

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Single and multiple | `type="single"`, `type="multiple"` | done | TOGGLEGROUP-01, TOGGLEGROUP-04 |
| Items | toggle look, `px-3`, first and last rounded, outline borders, `spacing` | done — the inner items square on the web | TOGGLEGROUP-02, TOGGLEGROUP-03 |
| Selected | `data-[state=on]:bg-accent` | done | TOGGLEGROUP-02, TOGGLEGROUP-03 |
| Items from the model | (React children) | done | TOGGLEGROUP-04, TOGGLEGROUP-05 |

## Slider

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Track, range, thumb | `h-1.5 bg-muted`, `bg-primary`, `size-4 border-primary bg-white shadow-sm` | done | SLIDER-02, FIDELITY-04 |
| Hover and focus ring | `hover:ring-4 focus-visible:ring-4 ring-ring/50` | done — on the web from the theme (Aioros RANGES-16) | SLIDER-02 |
| Step, keyboard, pointer | `step`, arrows, Home, End | done | SLIDER-01, SLIDER-03 |
| Vertical | `orientation="vertical"`, `min-h-44` | done | SLIDER-02 |
| Range (two thumbs) | `defaultValue={[25, 75]}` | done — `c:RangeSlider`, two overlaid sliders whose values stay ordered: a press moves the closest thumb, a thumb dragged or stepped past the other becomes the upper one, Shift and the page keys step by ten steps, Home and End move the first and the last thumb, as Radix Slider; on the web two range inputs in a group (Aioros `a:Web.Range`, RANGES-30 to RANGES-32) posting their values in order | SLIDER-05..08, FIDELITY-04 |
| Two-way value | `value` / `onValueChange` | done | SLIDER-03, SLIDER-04 |

## Progress

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Track and indicator | `h-2 rounded-full bg-primary/20`, `bg-primary` | done | PROGRESS-01, PROGRESS-02 |
| Bound value | `value` | done | PROGRESS-02, PROGRESS-03 |

## Input Group

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Group | `h-9 rounded-md border border-input shadow-xs`, `dark:bg-input/30` | done | INPUTGROUP-01, INPUTGROUP-03 |
| Addons | `inline-start`, `inline-end`, `block-end`; `text-sm font-medium text-muted-foreground` | done | INPUTGROUP-01, INPUTGROUP-03 |
| Focus ring on the group | `has-[[data-slot=input-group-control]:focus-visible]:ring-[3px]` | done — `:focus-within` on both targets (Aioros TEXT-34, LAYOUT-55) | INPUTGROUP-02 |
| Textarea, buttons, text | `InputGroupTextarea`, `InputGroupButton`, `InputGroupText` | done | INPUTGROUP-01 |
| Two-way text | (React state) | done | INPUTGROUP-04 |

## Input OTP

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Slots and groups | `h-9 w-9 border-y border-r first:rounded-l-md last:rounded-r-md`, `InputOTPGroup` | done | INPUTOTP-01, INPUTOTP-02 |
| Separator | `MinusIcon` | done | INPUTOTP-02 |
| Active slot ring | `data-[active=true]:ring-[3px] border-ring` | done — both targets; on the web the framework's script fills the slots and marks the active one while the input has the focus (`a:Web.Slots`, `a:Web.SlotActive`) | INPUTOTP-02, INPUTOTP-04 |
| Two-way code | `value` / `onChange` | done | INPUTOTP-02, INPUTOTP-03 |

## Popover

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Content | `w-72 rounded-md border bg-popover p-4 text-popover-foreground shadow-md` | done — both targets (Aioros resolved needs 32 to 39) | POPOVER-01, POPOVER-04 |
| Placement | `side="bottom"`, `align="center"`, `sideOffset={4}` | done — both targets (Aioros resolved needs 32 to 39) | POPOVER-01, POPOVER-02 |
| Focus and dismissal | first tabbable focused, Escape and outside click close, focus back on the trigger | done | POPOVER-02, POPOVER-04 |
| Animation | `fade-in-0 zoom-in-95 slide-in-from-top-2`, `animate-out` on close | done — on both targets the surface enters from opacity 0, 95% and 8 px up over 150 ms with `ease` from its top center and leaves with a fade and zoom only (on the web its `:closing` look, on the desktop a picture of it in the window's overlay layer, passing clicks through); it stands still when motion is reduced (THEME-07, `c:Motion.IsReduced`) | POPOVER-05, POPOVER-06, POPOVER-07, THEME-07, THEME-08 |
| Open state | `open` / `onOpenChange` | done — `c:Popover.IsOpen` two way on the desktop, planned on the web | POPOVER-03 |
| Content from the model | (React children) | done | POPOVER-03, POPOVER-04 |

## Tooltip

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Content | `rounded-md bg-foreground px-3 py-1.5 text-xs text-background` | done | TOOLTIP-01, TOOLTIP-04 |
| Arrow | `TooltipPrimitive.Arrow` (`size-2.5 rotate-45 rounded-[2px]`) | done — both targets (Aioros resolved needs 32 to 39) | TOOLTIP-01 |
| Side and offset | `side="top"`, 10 px above with the arrow | done — both targets (Aioros resolved needs 32 to 39) | TOOLTIP-01, TOOLTIP-02 |
| Delays | `delayDuration={0}`, `skipDelayDuration={300}` | done — both targets (Aioros resolved needs 32 to 39) | TOOLTIP-02, TOOLTIP-04 |
| Keyboard | shown on focus, Escape hides it | done — both targets (Aioros resolved needs 32 to 39) | TOOLTIP-02, TOOLTIP-04 |
| Bound and templated content | (React children) | done | TOOLTIP-03 |
| Animation | `animate-in fade-in-0 zoom-in-95 slide-in-from-bottom-2`, `animate-out` on close | done — on both targets the tip enters over 150 ms with `ease` from opacity 0, 95% and 8 px down from its bottom center and leaves with a fade and zoom (on the web an animation after its delay and transitions out, Aioros MOTION-19; on the desktop a picture of it) | TOOLTIP-05, TOOLTIP-06, TOOLTIP-07 |

## Hover Card

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Content | `w-64 rounded-md border bg-popover p-4 shadow-md` | done — both targets (Aioros resolved needs 32 to 39) | HOVERCARD-01 |
| Delays | `openDelay={700}`, `closeDelay={300}` | done — both targets (Aioros resolved needs 32 to 39) | HOVERCARD-02 |
| Staying open over the card | pointer over the trigger or the content | done — both targets (Aioros resolved needs 32 to 39) | HOVERCARD-02 |
| Keyboard | opens on focus, closes on blur | done — both targets (Aioros resolved needs 32 to 39) | HOVERCARD-02 |
| Content from the model | (React children) | done | HOVERCARD-03 |
| Animation | `fade-in-0 zoom-in-95 slide-in-from-top-2`, `animate-out` on close | done — enters on both targets and leaves with a fade and zoom on the web and, as a picture of it, on the desktop | HOVERCARD-05, HOVERCARD-06, HOVERCARD-07 |

## Dropdown Menu

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Content | `min-w-[8rem] rounded-md border bg-popover p-1 shadow-md`, 4 px from the trigger | done | DROPDOWN-01, DROPDOWN-05 |
| Items | `px-2 py-1.5 rounded-sm text-sm`, `focus:bg-accent`, `data-[disabled]:opacity-50` | done — both targets (Aioros resolved needs 32 to 39) | DROPDOWN-01, DROPDOWN-05 |
| Label | `px-2 py-1.5 text-sm font-medium`, `inset` | done — both targets (Aioros resolved needs 32 to 39) | DROPDOWN-01, DROPDOWN-02 |
| Checkbox and radio items | `pl-8`, `CheckIcon`, `CircleIcon size-2` | done — both targets (Aioros resolved needs 32 to 39) | DROPDOWN-02, DROPDOWN-04 |
| Shortcut | `ml-auto text-xs tracking-widest text-muted-foreground` | done — `InputGesture`, in the platform's notation (Ctrl+B on Linux and Windows, ⌘B on macOS) | DROPDOWN-01, DROPDOWN-05 |
| Submenu | `SubTrigger` with `ChevronRightIcon`, `SubContent` `shadow-lg` | done | DROPDOWN-01, DROPDOWN-02, DROPDOWN-05 |
| Destructive and inset items | `variant="destructive"`, `inset` | done — both targets (Aioros resolved needs 32 to 39) | DROPDOWN-02 |
| Keyboard | arrows without looping, Home, End, typeahead, Right and Left for submenus, Escape | done — both targets (Aioros resolved needs 32 to 39) | DROPDOWN-03 |
| Items from the model, commands | (React children, `onSelect`) | done | DROPDOWN-04, DROPDOWN-05 |
| Animation | `fade-in-0 zoom-in-95 slide-in-from-top-2`, the sub-menu's `slide-in-from-left-2`, `animate-out` on close | done — the menu and its sub-menus enter on both targets and leave with a fade and zoom on the web and, as pictures of them, on the desktop | DROPDOWN-06, DROPDOWN-07, DROPDOWN-08 |

## Context Menu

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Content and items | the dropdown menu's, `shadow-md` | done — both targets (Aioros resolved needs 32 to 39) | CONTEXTMENU-01 |
| At the pointer | Radix's `alignOffset` 2 | done — both targets (Aioros resolved needs 32 to 39) | CONTEXTMENU-01 |
| Keyboard | Shift+F10 and the context menu key open it, menu keys inside | done — both targets (Aioros resolved needs 32 to 39) | CONTEXTMENU-02 |
| Commands and bound items | (React children) | done — both targets (Aioros resolved needs 32 to 39) | CONTEXTMENU-03 |
| Animation | `fade-in-0 zoom-in-95 slide-in-from-left-2`, `animate-out` on close | done — enters on both targets and leaves with a fade and zoom on the web and, as a picture of it, on the desktop | CONTEXTMENU-05, CONTEXTMENU-06, CONTEXTMENU-07 |

## Dialog

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Content | `max-w-lg rounded-lg border bg-background p-6 shadow-lg`, centered | done | DIALOG-01, DIALOG-04 |
| Overlay | `fixed inset-0 bg-black/50` | done | DIALOG-01, DIALOG-04 |
| Header, title, description, footer | `gap-2`, `text-lg leading-none font-semibold`, `text-sm text-muted-foreground`, `sm:flex-row sm:justify-end gap-2` | done | DIALOG-01 |
| Close button | `absolute top-4 right-4 opacity-70 hover:opacity-100` | done | DIALOG-01, DIALOG-02 |
| Focus trap and return | Radix `FocusScope` | done — both targets (Aioros resolved needs 32 to 39) | DIALOG-02, DIALOG-04 |
| Escape and outside click | `onEscapeKeyDown`, `onPointerDownOutside` | done — both targets (Aioros resolved needs 32 to 39) | DIALOG-02 |
| Animation | `fade-in-0 zoom-in-95` (200 ms), the overlay's `fade-in-0`, `animate-out` on close | done — on both targets the surface fades and zooms in over 200 ms and the mask fades in, and both leave as `fade-out-0 zoom-out-95` and `fade-out-0` (on the web once the dialog closes in the browser; on the desktop they stay, passing clicks through, while their exit plays, the focus back at once) | DIALOG-06, DIALOG-07, DIALOG-09, THEME-08 |
| Open state, awaited answer | `open` / `onOpenChange` | done — `IsOpen` two way; on the web a command awaiting the answer | DIALOG-03, DIALOG-04 |

## Alert Dialog

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Content and sizes | `AlertDialogContent`, `size="sm"` (`max-w-xs`) | done | ALERTDIALOG-01 |
| Header and footer | `gap-1.5`, the title on a 28 px line, `AlertDialogCancel`, `AlertDialogAction` | done | ALERTDIALOG-01, ALERTDIALOG-04 |
| No dismissal outside, focus on cancel | Radix AlertDialog | done | ALERTDIALOG-02 |
| Escape | closes | done — both targets (Aioros resolved needs 32 to 39) | ALERTDIALOG-02 |
| Answer from the model | (React state) | done | ALERTDIALOG-03, ALERTDIALOG-04 |
| Animation | `fade-in-0 zoom-in-95` (200 ms), `animate-out` on close | done — enters and leaves on both targets (on the desktop it stays, passing clicks through, while its exit plays) | ALERTDIALOG-05, ALERTDIALOG-06, ALERTDIALOG-08 |

## Sheet

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Sides | `side="top" \| "right" \| "bottom" \| "left"` | done | SHEET-01, SHEET-02, SHEET-04 |
| Content | `w-3/4 sm:max-w-sm`, `border-l`, `shadow-lg` | done | SHEET-01 |
| Header and footer | `p-4 gap-1.5`, `mt-auto p-4 gap-2` | done | SHEET-01 |
| Behavior | the dialog's (focus, Escape, outside click) | done — both targets (Aioros resolved needs 32 to 39) | SHEET-02, SHEET-03, SHEET-04 |
| Animation | `slide-in-from-{side}` (500 ms `ease-in-out`), `slide-out-to-{side}` (300 ms) | done — on both targets each side slides in over 500 ms with `ease-in-out` and out over 300 ms, and the mask fades in and out (on the desktop the sheet stays, passing clicks through, while it slides out) | SHEET-05, SHEET-06, SHEET-08 |

## Drawer

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Content | `rounded-t-lg border-t max-h-[80vh] mt-24`, bottom | done | DRAWER-01, DRAWER-02, DRAWER-04 |
| Handle | `mx-auto mt-4 h-2 w-[100px] rounded-full bg-muted` | done | DRAWER-01 |
| Header and footer | centered `gap-0.5`, `p-4 gap-2` | done | DRAWER-01 |
| Drag to dismiss | vaul | done — dragged down past a quarter of its height or flicked faster than 0.4 px/ms it closes, the mask fading as it goes, dampened upward, on both targets (on the web since Aioros) | DRAWER-09, DRAWER-10 |
| Behavior | Escape, outside click, focus | done — both targets (Aioros resolved needs 32 to 39) | DRAWER-02, DRAWER-03 |
| Animation | vaul's slide up (`transform .5s cubic-bezier(.32,.72,0,1)`) | done — slides up on both targets and down over vaul's 500 ms on both (on the desktop it stays, passing clicks through, while it slides down) | DRAWER-05, DRAWER-06, DRAWER-08 |

## Sonner

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Toast | 356 px, `p-4`, `--popover`, `--border`, `--radius`, 13 px, sonner's shadow | done — both targets (Aioros resolved needs 32 to 39) | SONNER-01, SONNER-04 |
| Title, description, icons | medium 13 px, sonner's description gray, Lucide icons per type | done — both targets (Aioros resolved needs 32 to 39) | SONNER-01, SONNER-03, SONNER-04 |
| Action | `action: { label, onClick }` | done — both targets (Aioros resolved needs 32 to 39) | SONNER-01, SONNER-02, SONNER-04 |
| Stack and expand | three visible, 14 px lift and 5% scale, expand on hover, `expand` | done — both targets (on the web a deck the browser keeps, since) | SONNER-01, SONNER-02, SONNER-12 |
| Duration and pause | 4 s, paused under the pointer | done — both targets (on the web the expiry pauses while the deck is expanded, pressed or the page hidden, since) | SONNER-02, SONNER-12 |
| Types and promise | `toast.success`, `info`, `warning`, `error`, `promise` | done — the kinds and `Resolve` on both targets' models; the icons on the desktop | SONNER-03, SONNER-04 |
| Swipe to dismiss, close button | sonner's gestures, `closeButton` | done — swiped toward its corner's sides past 45 px or faster than 0.11 px/ms, dampened the other way, on both targets; `CloseButton` on the toaster or the toast shows sonner's close button, which closes the toast (in the browser with no request); a press on a toast no longer closes it (since) | SONNER-08, SONNER-09, SONNER-10, SONNER-11 |
| Animation | a toast rises from `translateY(100%)` and opacity 0, the stack moves and shrinks, 400 ms | done — on the desktop the toast rises, the stack follows, and a closing toast falls and fades over 400 ms while the toast behind moves to the front, then leaves (SONNER-07); on the web the toast rises and falls after its expiration as sonner's, the cards behind stand 14 px up at 95% and 90% and expand under the pointer, and the stack moves over 400 ms as toasts come and go across posts (since) | SONNER-05, SONNER-06, SONNER-07, SONNER-12, SONNER-13 |

## Separator

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Line | `shrink-0 bg-border h-px w-full` | done | SEPARATOR-01, SEPARATOR-02 |
| Vertical | `orientation="vertical"`: `h-full w-px` | done | SEPARATOR-01, SEPARATOR-02 |
| Decorative | Radix `decorative` (no role) | done — an `<hr>`, the menus' own separators kept | SEPARATOR-01, SEPARATOR-02 |
| Bound visibility and orientation | (React props) | done | SEPARATOR-03 |

## Skeleton

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Box | `rounded-md bg-accent`, sized by classes | done | SKELETON-01, SKELETON-02 |
| Pulse | `animate-pulse` | done | SKELETON-01, SKELETON-02 |
| Loading state from the model | (React state) | done | SKELETON-03 |

## Spinner

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Icon and size | `Loader2Icon size-4`, `size-3` … `size-8`, in buttons and badges | done | SPINNER-01, SPINNER-02 |
| Turn | `animate-spin` | done | SPINNER-01, SPINNER-02 |
| Accessibility | `role="status" aria-label="Loading"` | done — named on both targets, `role="status"` on the web | SPINNER-01, SPINNER-02 |
| Busy command | (React state) | done | SPINNER-03 |

## Kbd

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Key | `h-5 min-w-5 rounded-sm bg-muted px-1 text-xs font-medium text-muted-foreground` | done | KBD-01, KBD-02 |
| Group | `KbdGroup`: `gap-1` | done | KBD-01, KBD-02 |
| In buttons and tooltips | `Button` content, `[[data-slot=tooltip-content]_&]:bg-background/20` | done — in buttons, and in tooltips with `--background` at 20% (10% dark) and `--background` text on both targets (on the web) | KBD-01, KBD-02, KBD-05, KBD-06 |
| Element | `<kbd>` | done — `a:Web.Tag` in the key's theme | KBD-01 |
| Bound keys | (React children) | done | KBD-03, KBD-04 |

## Avatar

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Image | `AvatarImage aspect-square size-full` | done — clipped to the circle on both targets | AVATAR-01, AVATAR-02 |
| Fallback | `AvatarFallback bg-muted text-sm text-muted-foreground`, `sm` `text-xs` | done | AVATAR-01, AVATAR-02 |
| Sizes and shapes | `size-8`, `sm` `size-6`, `lg` `size-10`, `rounded-lg` | done | AVATAR-01, AVATAR-02 |
| Group | `AvatarGroup -space-x-2 ring-2 ring-background` | done | AVATAR-01, AVATAR-02, AVATAR-03 |
| Badge, group count | `AvatarBadge`, `AvatarGroupCount` | done — `c:Avatar.Badge` holds a `c:AvatarBadge` in the avatar's corner, inside its circle (8, 10 or 12 px with the avatar's size, its icon hidden in `sm`), and `c:AvatarGroupCount` closes a group as large as its avatars, on both targets | AVATAR-05, AVATAR-06 |
| Bound members | (React children) | done — templates, commands and a bound `Source` or content of `c:Avatar` on both targets (on the web) | AVATAR-03, AVATAR-04 |

## Alert

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Box | `rounded-lg border px-4 py-3 text-sm bg-card` | done | ALERT-01, ALERT-02 |
| Icon, title, description | `grid-cols-[16px_1fr] gap-x-3`, `font-medium tracking-tight`, `text-muted-foreground` | done — the title's tracking on both targets | ALERT-01, ALERT-02 |
| Destructive | `text-destructive`, description `text-destructive/90` | done | ALERT-01, ALERT-02, ALERT-04 |
| Role | `role="alert"` | done — an assertive live setting on the desktop, `role="alert"` on the web | ALERT-01, ALERT-02 |
| Bound content and variant | (React props) | done — content, class and a bound `Title` on both targets (on the web) | ALERT-03, ALERT-04 |

## Aspect Ratio

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Ratio | `ratio={16 / 9}` | done — the desktop lays it out; the web writes CSS `aspect-ratio` | ASPECTRATIO-01, ASPECTRATIO-02 |
| Bound ratio | (React props) | done — on both targets (on the web) | ASPECTRATIO-03 |

## Typography

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Headings | `h1` `text-4xl font-extrabold tracking-tight`, `h2` `text-3xl font-semibold border-b pb-2`, `h3`, `h4` | done — sizes, weights and tracking on both targets (`h1` in Geist Bold, the heaviest weight shipped), `<h1>` to `<h4>` on the web | TYPOGRAPHY-01, TYPOGRAPHY-02 |
| Paragraph, lead, large, small, muted | `leading-7`, `text-xl text-muted-foreground`, `text-lg font-semibold`, `text-sm leading-none font-medium`, `text-sm text-muted-foreground` | done | TYPOGRAPHY-01, TYPOGRAPHY-02 |
| Blockquote | `border-l-2 pl-6 italic` | done — italic on both targets, a `<blockquote>` on the web | TYPOGRAPHY-01, TYPOGRAPHY-02 |
| List | `ml-6 list-disc [&>li]:mt-2`, `list-inside` | done | TYPOGRAPHY-01, TYPOGRAPHY-02, ALERT-02 |
| Inline code | `rounded bg-muted px-[0.3rem] py-[0.2rem] font-mono text-sm font-semibold` | done | TYPOGRAPHY-01, TYPOGRAPHY-02 |
| Balanced lines | `text-balance` | done — `a:Web.TextWrap="balance"`: CSS's `text-wrap:balance` on the web, the narrowest wrap keeping the line count on the desktop; on the empty state, the horizontal field's description, the item's description and the tooltip | TYPOGRAPHY-05, TYPOGRAPHY-06 |
| Bound text and lists | (React children) | done | TYPOGRAPHY-03 |

## Empty

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Layout | `flex-col items-center justify-center gap-6 rounded-lg p-6 md:p-12`, header `max-w-sm gap-2` | done | EMPTY-01, EMPTY-02 |
| Media | `EmptyMedia variant="icon"`: `size-10 rounded-lg bg-muted` | done | EMPTY-01, EMPTY-02 |
| Title and description | `text-lg font-medium tracking-tight`, `text-sm/relaxed text-muted-foreground` | done — tracking on both targets | EMPTY-01, EMPTY-02 |
| Outline | `border border-dashed` | done — dashed on both targets | EMPTY-01, EMPTY-02 |
| Empty collection | (React state) | done | EMPTY-03 |

## Item

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Variants and sizes | default, `outline`, `muted`; `sm` | done | ITEM-01, ITEM-02 |
| Media, content, actions | `ItemMedia` (`icon`, `image`), `ItemTitle leading-snug font-medium`, `ItemDescription`, `ItemActions` | done | ITEM-01, ITEM-02 |
| As a link | `asChild` `<a>`, `[a]:hover:bg-accent/50` | done — the `link` class, `a:Web.Href` | ITEM-02 |
| Header, footer, separator | `ItemHeader`, `ItemFooter`, `ItemSeparator` | done — `Header` and `Footer` rows (`c:ItemHeader`, `c:ItemFooter`, their ends apart) and `c:ItemSeparator` with no margin, on both targets | ITEM-05, ITEM-06, ITEM-07, ITEM-08 |
| Items from the model, actions | (React children) | done — templates, commands and bound `Title` and `Description` on both targets (on the web) | ITEM-03, ITEM-04 |

## Tabs

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| List and triggers | `TabsList bg-muted rounded-lg p-[3px] h-9`, `TabsTrigger rounded-md px-2 py-1 text-sm font-medium` | done — on both targets (on the web) | TABS-01, TABS-02 |
| Selected and hover | `data-[state=active]:bg-background shadow-sm`, dark `border-input bg-input/30`, `hover:text-foreground` | done — on both targets (on the web) | TABS-01, TABS-02 |
| Keyboard | roving arrows, Home, End, loop, automatic activation | done — `TabsKeyboard` on the desktop, Aioros's keyboard module on the web (no request) | TABS-01, TABS-03 |
| Disabled | `disabled:opacity-50` | done — on both targets (on the web) | TABS-02 |
| Roles | `tablist`, `tab`, `tabpanel` | done — `tablist`, `tab` and `tabpanel` on the web, the desktop's automation peers | TABS-01 |
| Line variant and vertical | `variant="line"`, `orientation="vertical"` | done — the `line` class and `TabStripPlacement="Left"` on both targets: the line strip, the selected tab's line below it or beside it, the vertical strip with its page beside it and Up and Down moving; the focus ring with its outline for every tab (on the web, ITEMS-127 to ITEMS-131) | TABS-06 to TABS-11 |
| Bound tabs, two-way selection | (React state) | done | TABS-04, TABS-05 |

## Accordion

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Item, header, content | `border-b`, `py-4 text-sm font-medium hover:underline`, `ChevronDownIcon` turning 180°, `pb-4` | done — on both targets (on the web) | ACCORDION-01, ACCORDION-02 |
| Single, multiple, collapsible | `type`, `collapsible` | done — on both targets (on the web `a:Web.Exclusive` on a single accordion, HTML's `<details name>`, which lets the open item close) | ACCORDION-03 |
| Animation | `animate-accordion-down`, `animate-accordion-up` | done — `HeightTransition` on the desktop, `::details-content` on the web | ACCORDION-04 |
| Items from the model | (React children) | done — templates on both targets (on the web) | ACCORDION-05 |

## Collapsible

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Open state and trigger | `open`, `onOpenChange`, `CollapsibleTrigger` | done — the header toggles, `IsExpanded` two way | COLLAPSIBLE-01, COLLAPSIBLE-03 |
| The demo's toggle | ghost `size-8` button with `ChevronsUpDown` | done — on both targets (on the web written through the toggle's template) | COLLAPSIBLE-01, COLLAPSIBLE-02 |

## Breadcrumb

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Links, page, separators | `text-sm text-muted-foreground gap-2.5`, `hover:text-foreground`, `ChevronRight size-3.5` | done | BREADCRUMB-01, BREADCRUMB-02 |
| Custom separator, ellipsis, menu | `BreadcrumbSeparator` children, `BreadcrumbEllipsis`, a `DropdownMenu` trigger | done — the menu button without the browser's padding on the web | BREADCRUMB-01, BREADCRUMB-02 |
| Semantics | `<nav aria-label="breadcrumb">`, `<ol>`, `aria-current="page"` | done — a named navigation landmark on both targets, `<ol>`/`<li>` with hidden separators and the page's `aria-current="page"` on the web | BREADCRUMB-01, BREADCRUMB-02, BREADCRUMB-04 |
| Bound crumbs | (React children) | done — on both targets (on the web the Gallery's crumbs from the model with their bound addresses) | BREADCRUMB-03 |

## Pagination

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Links | `buttonVariants` ghost and outline (`isActive`), `size-9` | done | PAGINATION-01, PAGINATION-02 |
| Previous and next | `gap-1 px-2.5`, chevrons | done — both targets, the chevrons beside the labels on the web | PAGINATION-01, PAGINATION-02, PAGINATION-06 |
| Ellipsis | `MoreHorizontalIcon size-4` in `size-9` | done | PAGINATION-01, PAGINATION-02 |
| Semantics | `role="navigation" aria-label="pagination"`, `aria-current` | done — the named landmark on both targets; on the web a `<ul>` of `<li>` items and the current page's `aria-current="page"` | PAGINATION-01, PAGINATION-02, PAGINATION-06 |
| Pages from the model | (React state) | done | PAGINATION-03, PAGINATION-04 |

## Button Group

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Joined children | `rounded-l-none border-l-0` / `rounded-r-none`, vertical | done | BUTTONGROUP-01, BUTTONGROUP-02 |
| Nested groups, separator, text | `gap-2`, `ButtonGroupSeparator bg-input`, `ButtonGroupText bg-muted` | done | BUTTONGROUP-01, BUTTONGROUP-02 |
| Focus above neighbors | `focus-visible:z-10` | done | BUTTONGROUP-02 |
| Role | `role="group"` | done — `AutomationProperties.ControlTypeOverride` `Group`, `role="group"` on the web | BUTTONGROUP-01 |
| Commands and bound looks | (React props) | done | BUTTONGROUP-03, BUTTONGROUP-04 |

## Scroll Area

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Viewport | `size-full rounded-[inherit]`, clipped to the root's border | done | SCROLLAREA-01, SCROLLAREA-02 |
| Scroll bars | `w-2.5 border-l border-l-transparent p-px`, thumb `rounded-full bg-border`, `type="hover"` | done — on both targets the bars show under the pointer and hide 600 ms after it leaves (Radix's `scrollHideDelay`; on the web the thumb's color under the pointer) | SCROLLAREA-02, SCROLLAREA-04, SCROLLAREA-05 |
| Horizontal | `ScrollBar orientation="horizontal"`, `w-max` content | done — on both targets (on the web) | SCROLLAREA-01, SCROLLAREA-02 |
| Scrolling | Radix's viewport scrolls natively | done — on both targets (on the web) | SCROLLAREA-01, SCROLLAREA-02 |
| Bound content and offset | (React children) | done — `ItemsSource` and an item template on both targets, `Offset` two way on the desktop | SCROLLAREA-03 |

## Table

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Header | `h-10 px-2 font-medium text-foreground`, `[&_tr]:border-b` | done — on both targets (on the web) | TABLE-01, TABLE-02 |
| Rows | `border-b` but the last, `hover:bg-muted/50`, `data-[state=selected]:bg-muted` | done — on both targets (on the web) | TABLE-01, TABLE-02 |
| Cells | `p-2 align-middle whitespace-nowrap`, `text-right` | done | TABLE-01, TABLE-02 |
| Footer | `border-t bg-muted/50 font-medium` | done — `c:TableFooter` | TABLE-01, TABLE-02 |
| Caption | `mt-4 text-sm text-muted-foreground` | done — `c:TableCaption` | TABLE-01, TABLE-02 |
| Column widths | the browser's automatic table layout | done — pixel and star widths on both targets (on the web each star column's share as a percentage) | TABLE-01, TABLE-02 |
| Rows from the model, selection | (React state) | done — `ItemsSource`, cell templates, `SelectedItem` two way on the desktop, posted commands on the web | TABLE-03, TABLE-04 |

## Navigation Menu

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Trigger | `navigationMenuTriggerStyle` (`h-9 px-4 py-2 font-medium`), chevron `size-3` turning 180° | done — the padding, chevron and open look on both targets (on the web) | NAVMENU-01, NAVMENU-02 |
| Content | `viewport={false}`: `top-full mt-1.5 rounded-md border bg-popover shadow p-2 pr-2.5` | done — 6 px under the trigger's start on both targets | NAVMENU-01, NAVMENU-02 |
| Links | `flex-col gap-1 rounded-sm p-2 hover:bg-accent`, `line-clamp-2` | done — on both targets (on the web) | NAVMENU-01, NAVMENU-02 |
| Hover and delays | `delayDuration` 200 ms, switching while open, a close timer | done — the desktop through a `TimeProvider`, the web through `a:Web.HoverOpenDelay` and the overlay module | NAVMENU-03 |
| Keyboard | arrows between items, Escape | done — Left and Right between the items on both targets, every item kept in the tab sequence (on the web Avalonia's XY keyboard navigation as roving focus) | NAVMENU-03, NAVMENU-10 |
| Semantics | `<nav aria-label="Main">` | done — a navigation landmark on both targets | NAVMENU-01, NAVMENU-02 |
| Links from the model | (React children) | done — item templates and commands on both targets (on the web) | NAVMENU-04, NAVMENU-05 |
| Animation | the viewport's `zoom-in-90` (150 ms), `zoom-out-95` on close; the content's `data-[motion]` slides between items | done — the content (`viewport={false}`, as the demo at this width) fades and zooms in and out over 200 ms from its center on both targets, and slides 208 px between items as Radix's `data-motion` on both (on the web `data-switch` and the overlay module, Aioros NAVIGATION-99, OVERLAY-15) | NAVMENU-06, NAVMENU-07, NAVMENU-08, NAVMENU-09 |

## Menubar

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Bar | `h-9 rounded-md border bg-background p-1 shadow-xs gap-1` | done | MENUBAR-01, MENUBAR-02 |
| Triggers | `px-2 py-1 font-medium`, `data-[state=open]:bg-accent` | done — on both targets (on the web) | MENUBAR-01, MENUBAR-02 |
| Content | `min-w-[12rem] rounded-md border p-1 shadow-md`, `sideOffset` 8, `alignOffset` -4 | done — on both targets (on the web) | MENUBAR-01, MENUBAR-02 |
| Items, shortcuts, separators, checks, radios, submenus | Cosmo's menu items | done — Cosmo's menu items (shortcuts in Linux's notation) | MENUBAR-01, MENUBAR-02 |
| Keyboard | Radix's arrows between menus, first item focused, Escape | done — Avalonia's menu keys | MENUBAR-03 |
| Commands and checks from the model | (React state) | done — commands and checkable items bound two way on both targets (on the web) | MENUBAR-04, MENUBAR-05 |
| Animation | `fade-in-0 zoom-in-95 slide-in-from-top-2`, `animate-out` on close | done — the menus enter on both targets and leave with a fade and zoom on the web and, as pictures of them, on the desktop | MENUBAR-06, MENUBAR-07, MENUBAR-08 |

## Resizable

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Group | `flex h-full w-full aria-[orientation=vertical]:flex-col` | done — a `Grid` of star tracks | RESIZABLE-01, RESIZABLE-02 |
| Handle | `w-px bg-border`, `after:w-1` hit area, `focus-visible:ring-1 ring-ring ring-offset-1` | done — the line on both targets; the hit area and ring on the desktop | RESIZABLE-01, RESIZABLE-02 |
| Grip | `withHandle`: `h-4 w-3 rounded-xs border bg-border`, `GripVerticalIcon size-2.5` | done — on both targets (on the web) | RESIZABLE-02 |
| Sizes | `defaultSize`, `minSize`, `maxSize` in percent | done — percentages of the group on both targets; limits on the desktop | RESIZABLE-01, RESIZABLE-03 |
| Dragging | pointer drag of the separator | done — Avalonia's `GridSplitter` on the desktop, Aioros's split module on the web | RESIZABLE-03 |
| Keyboard | arrows by 5%, Home, End, Enter (collapse), F6 | done — on both targets (on the web `a:Web.ResizeStep` 5, `a:Web.CollapsedSize`, `a:Web.MinSize` and `a:Web.MaxSize`; F6 is the split module's) | RESIZABLE-03 |
| Bound sizes | `onLayoutChange`, `panel.resize()` | done — `Size` two way on both targets: on the web a drag or a key on the handle posts its position and the model follows | RESIZABLE-04, RESIZABLE-05, RESIZABLE-06 |

## Carousel

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Viewport and slides | `overflow-hidden`, `basis-full`, `-ml-4`/`pl-4` | done — on the desktop (no gap between slides while sliding) and as a scroll-snap strip of the template's slides on the web | CAROUSEL-01, CAROUSEL-02 |
| Previous and next | `size-8 rounded-full` outline buttons 48 px outside, disabled at the ends | done — on both targets (on the web) | CAROUSEL-02, CAROUSEL-03 |
| Orientation | `orientation="vertical"`, `-mt-4`, buttons turned 90° | done — `vertical` and `align-start` on both targets (on the web) | CAROUSEL-02, CAROUSEL-03 |
| Keyboard and swipe | ArrowLeft and ArrowRight, Embla's drag | done — keys on both targets; `IsSwipeEnabled` on the desktop; on the web touch scrolls natively and the mouse drags the strip as Embla's (since) | CAROUSEL-03, CAROUSEL-06 |
| Loop | `opts.loop` | done — `WrapSelection` on both targets (on the web) | CAROUSEL-03 |
| Indicators | (carousel-api's `Slide 1 of 5`) | done — Cosmo's `indicators` class on both targets (on the web); the Binding example's `Slide N of M` follows the buttons and the indicators on both targets | CAROUSEL-03, CAROUSEL-04, CAROUSEL-05, CAROUSEL-07 |
| Bound slides and index | `setApi`, `api.on("select")` | done — `ItemsSource`, `ItemTemplate` and `SelectedIndex` two way on both targets: on the web each move is posted once and a slide the model adds is written with its dot | CAROUSEL-04, CAROUSEL-05, CAROUSEL-07 |

## Command

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Input | `h-9 border-b px-3`, `SearchIcon size-4 opacity-50`, `h-10 py-3 text-sm` input | done — the input and its row on both targets | COMMAND-01, COMMAND-02 |
| Groups and headers | `p-1`, heading `px-2 py-1.5 text-xs font-medium text-muted-foreground` | done | COMMAND-01, COMMAND-02 |
| Items | `px-2 py-1.5 rounded-sm text-sm gap-2`, `data-[selected=true]:bg-accent`, `data-[disabled=true]:opacity-50` | done — on both targets (on the web from the items' template) | COMMAND-01, COMMAND-02 |
| Shortcuts | `ml-auto text-xs tracking-widest text-muted-foreground` | done — on both targets (on the web from the items' template) | COMMAND-02 |
| Filtering and ranking | cmdk's `command-score`, groups hidden, separators hidden, `CommandEmpty` | done — on both targets (the desktop's score equal to cmdk's, the web's filter module with cmdk's `command-score`; a `Value` and `Keywords` reach the web as `TextSearch.Text`) | COMMAND-02, COMMAND-03, COMMAND-04 |
| Keyboard and pointer | arrows, Ctrl+N/J/P/K, Home, End, Alt and Meta with the arrows, Enter, `loop`, pointer selection | done — on both targets (on the web the filter module's keys) | COMMAND-04 |
| Dialog | `CommandDialog`: `p-0`, `h-12` input, `py-3` items, Ctrl+J/⌘J in the example | done — desktop (`Gesture`, Meta as Ctrl); on the web the `Gesture` presses the dialog's `Trigger` (its hot key), whose post opens the dialog (need 37), and a dialog without a trigger opens and closes with its `Gesture` in the browser (`a:Web.ModalKey`) | COMMAND-01, COMMAND-06, COMMAND-08, COMMAND-10 |
| Bound search, value and items | `value`/`onValueChange`, `onSelect` | done — `Text`, `SelectedValue`, items from the model and their commands on both targets (a group's `ItemCommand` and `ItemCommandParameter` read from each item): on the web the post carries the text and the chosen item, which becomes the selected value before its command runs | COMMAND-05, COMMAND-09, COMMAND-11, COMMAND-12 |

## Combobox

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Trigger | outline `Button`, `w-[200px] justify-between`, `ChevronsUpDown opacity-50` | done | COMBOBOX-01, COMBOBOX-02 |
| Popover | `PopoverContent w-[200px] p-0` with a `Command` (`h-9` input) | done — on both targets 200 px as its trigger, 4 px below it; on the web the popover holds the command's input and options, filtered in the browser, the options' texts starting at the items' padding | COMBOBOX-01, COMBOBOX-02, COMBOBOX-06 |
| Choosing | toggles the value, closes the popover, the check `opacity-100` on the chosen item | done — on both targets (on the web the chosen item posts and shows on the trigger; chosen in the browser with no request, the trigger, the check and the popover following, the index posted with the next post or at once when bound) | COMBOBOX-02, COMBOBOX-03, COMBOBOX-05 |
| Bound items and selection | (React state) | done — `ItemsSource`, `DisplayMemberBinding`, `SelectedItem`, `SelectedItems`, `IsDropDownOpen` two way on the desktop; on the web the items, filtering and a posted `SelectedItem` | COMBOBOX-04 |

## Data Table

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Toolbar | `Input` `max-w-sm` filter, outline `Columns` dropdown with checkbox items | done — the filter and the menu on both targets; its checks follow the columns on the desktop | DATATABLE-01, DATATABLE-03, DATATABLE-05 |
| Table | shadcn's `Table` in `rounded-md border`, rows `data-state=selected` | done — on both targets (on the web the header look, row borders and selected rows, the header row at its 40 px;) | DATATABLE-01, DATATABLE-03 |
| Header controls | select-all `Checkbox` (indeterminate), ghost sort `Button` with `ArrowUpDown` | done — on both targets (on the web) | DATATABLE-03, DATATABLE-05 |
| Filtering | `includesString` on email | done — as typed on the desktop, posted 300 ms after the last keystroke on the web (`a:Web.Delay`) | DATATABLE-02, DATATABLE-04, DATATABLE-05 |
| Sorting | `alphanumeric`, ascending then descending | done — `SortCommand` on both targets (on the web) | DATATABLE-03, DATATABLE-04, DATATABLE-05 |
| Pagination | `previousPage`, `nextPage`, `getCanPreviousPage`, 10 a page | done — `PreviousPageCommand` and `NextPageCommand` with `CanExecute` on both targets | DATATABLE-02, DATATABLE-04 |
| Row selection | row checkboxes, `N of M row(s) selected.` across pages | done — on both targets (on the web each check posts on change) | DATATABLE-02, DATATABLE-04, DATATABLE-05 |
| Column visibility | `column.toggleVisibility` from the menu | done — `IsVisible` two way on both targets, also through `Source.Columns[n]` (on the web) | DATATABLE-05 |
| Table state across posts | TanStack's `state` (`pagination`, `sorting`, `columnFilters`, `columnVisibility`, `rowSelection`), kept by React between renders | done — `DataTableSource.State` bound to `c:DataTable.State` travels in the page's hidden input on the web (Aioros's `a:Web.State`), so a post binds the rows the visitor saw; sorting goes back to the first page as TanStack's `autoResetPageIndex` | DATATABLE-06, DATATABLE-07, DATATABLE-08 |

## Sidebar

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Layout | `SidebarProvider` flex row, `w-(--sidebar-width)` 16rem, `SidebarInset` | done — on both targets (on the web `a:Web.Layout="Dock"`) | SIDEBAR-01, SIDEBAR-02 |
| Variants | `sidebar`, `floating` (`rounded-lg border shadow-sm`, `p-2`), `inset` (`m-2 rounded-xl shadow-sm`) | done — on both targets (on the web) | SIDEBAR-02 |
| Collapsible | `offcanvas` (slides out), `icon` (3rem, labels `-mt-8 opacity-0`, buttons `size-8`), `none`; 200 ms linear | done — on both targets (on the web switched in the browser with no request) | SIDEBAR-03, SIDEBAR-04 |
| Menu | `SidebarMenu`, `SidebarMenuItem`, `SidebarMenuButton` (`isActive`, `tooltip`, `sm`, `lg`, `outline`), `SidebarMenuSub`, `SidebarMenuAction`, `SidebarMenuBadge` | done — on both targets (on the web the menus and their looks, the icon mode's tooltips) | SIDEBAR-01, SIDEBAR-02, SIDEBAR-03 |
| Collapsible items | `Collapsible` around a `SidebarMenuItem`, the chevron `rotate-90` | done — `IsCollapsible`, `IsExpanded` two way on both targets: on the web toggled in the browser and a bound `IsExpanded` posted to the model (Aioros ENGINE-259, ENGINE-264) | SIDEBAR-02, SIDEBAR-04, SIDEBAR-09, SIDEBAR-10 |
| Trigger and rail | `SidebarTrigger` (`size-7` ghost, `PanelLeft`), `SidebarRail` (2px line on hover) | done — on both targets (on the web `a:Web.Flips`) | SIDEBAR-01, SIDEBAR-04 |
| Keyboard shortcut | ⌘B / Ctrl+B | done — `ToggleGesture` on the desktop, the trigger's hot key on the web (Meta as Ctrl) | SIDEBAR-04 |
| Mobile | a `Sheet` (18rem) under 768 px, `openMobile` | done — on both targets; on the web a modal `<dialog>` under the provider's breakpoint | SIDEBAR-05, SIDEBAR-08 |
| Bound and persisted state | `open`/`onOpenChange`, the `sidebar_state` cookie | done — `IsOpen` two way and `StateStore` on the desktop; on the web the state switched in the browser is posted to a bound `IsOpen` and kept in the `StateKey` cookie, which the app's loader reads (the Workspace example) | SIDEBAR-01, SIDEBAR-06, SIDEBAR-07 |

## Calendar

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Box, header and grid | `p-3`, `--cell-size` 32 px, ghost `button_previous`/`button_next`, `month_caption` `text-sm font-medium`, weekdays `text-[0.8rem] text-muted-foreground`, weeks `mt-2` | done — at shadcn/ui's boxes on the desktop and through Cosmo's theme on the web | CALENDAR-01, CALENDAR-04, FIDELITY-12 |
| Shown weeks | only the weeks of the month (no `fixedWeeks`) | done — on both targets (`c:CalendarDays`; the other weeks hidden on the web) | CALENDAR-01, CALENDAR-04 |
| Selected day | `data-selected-single` `bg-primary text-primary-foreground` | done — on both targets (`aria-selected` with Cosmo's look on the web) | CALENDAR-01, CALENDAR-04 |
| Range | `mode="range"`: `range_start`/`range_end` `bg-primary`, `range_middle` `bg-accent`, the band rounded at each week's ends | done — the looks and two clicks (`AllowTapRangeSelection`) on both targets (on the web); `RangeStart`/`RangeEnd` two way | CALENDAR-02, CALENDAR-03 |
| Multiple | `mode="multiple"` | done — the looks and `Dates` two way on both targets; a plain click or Space toggles a day as react-day-picker does, on the desktop and on the web, where the selection is kept across posts (Aioros ENGINE-277) | CALENDAR-02, CALENDAR-03, CALENDAR-07, CALENDAR-10 |
| Disabled days | `disabled`, `text-muted-foreground opacity-50` | done — `BlackoutDates` on both targets (on the web) | CALENDAR-02 |
| Outside days | `outside` muted, `showOutsideDays={false}` | done — muted, and hidden with `outside-hidden`, on both targets | CALENDAR-01, CALENDAR-02 |
| Navigation and keys | previous and next months, react-day-picker's arrow keys | done — Avalonia's buttons and keys on the desktop; the buttons posted and the WAI-ARIA date grid's keys moving the focus on the web | CALENDAR-03, CALENDAR-04 |
| Caption layout | `captionLayout="dropdown"` (month and year selects) | done — the `dropdown` class: the culture's abbreviated months and the years from a hundred before today to today in two selects (`c:CalendarCaption`), native `<select>`s on the web posting the chosen month | CALENDAR-06, CALENDAR-09 |
| Week numbers | `showWeekNumber` | done — the `week-numbers` class: each week's number in the culture's rule before its days, a row header on the web (Aioros RANGES-34) | CALENDAR-05, CALENDAR-08 |
| Locale | the browser's locale (`June 2025`) | done — the culture's year and month pattern, month names and first day on both targets; on the web the visitor's culture (the Gallery serves `en-US` and `es-ES`) | CALENDAR-03, CALENDAR-04, CALENDAR-11 |
| Bound selection | `selected`/`onSelect` (React state) | done — `SelectedDate`, `RangeStart`/`RangeEnd` and `Dates` two way on both targets; on the web a bound calendar is planned, its month computed for each request (Aioros RANGES-36 to RANGES-39), the Gallery's examples bound to their model | CALENDAR-03, CALENDAR-12 |

## Date Picker

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Trigger | outline `Button` `w-[240px] justify-start font-normal`, `CalendarIcon`, `format(date, "PPP")` or `Pick a date` muted | done — date-fns's `PPP` through Cosmo's `DateText` (`June 12th, 2025`; the culture's long date without its weekday outside English), written through Cosmo's theme on the web | DATEPICKER-01, DATEPICKER-02 |
| Popover | `PopoverContent w-auto p-0 align="start"` with a `Calendar mode="single"` | done — 4 px below the button with Cosmo's calendar on both targets, opened in the browser without a request; a chosen day closes it, the months' buttons keep it open | DATEPICKER-01, DATEPICKER-02 |
| Input variant | an `Input` with a ghost `size-6` calendar button inside, the popover `align="end"` | done — the `input` class on both targets; on the web the typed date is posted on Enter or when the field loses the focus and parsed as Avalonia's picker parses it | DATEPICKER-01, DATEPICKER-02 |
| Range | `mode="range" numberOfMonths={2}` | done — `c:DateRangePicker`: two months side by side and react-day-picker's `addToRange` on both targets; on the web the popover stays open across the posts of its days and the range is kept (Aioros NAVIGATION-98, RANGES-35, SSR-242, ENGINE-277;) | DATEPICKER-03, DATEPICKER-05 |
| Presets | a `Select` of days from today above the calendar | done — `c:DatePresets.Items` on a `CalendarDatePicker`: the select above the framed calendar on both targets, a choice picking that many days from today; on the web the choice posts from inside the popover, which stays open (Aioros RANGES-35, ITEMS-144) | DATEPICKER-04, DATEPICKER-06 |
| Bound date | `selected`/`onSelect` | done — `SelectedDate` two way on the desktop; posted by the date input on the web | DATEPICKER-02 |

## Chart

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Container | `ChartContainer`: `aspect-video`, `text-xs`, Recharts' grid, ticks, cursors, dots and sectors restyled from the tokens | done — Pictor's theme keys in every color theme, light and dark, and `c:CosmoChartsTheme`'s 12 px text in Geist | CHART-01, FIDELITY-13, FIDELITY-14 |
| Chart config | `ChartConfig`: each key's label and color, as `--color-{key}` | done — `c:ChartConfig` of `c:ChartConfigItem`s (`Key`, `Label`, `Color`: a theme key followed through the variant, or a CSS color) given with `c:ChartConfig.Config`: each series of that data key takes the label and the color where it sets none; the 26 fixtures read their examples' `chartConfig`; a pie's slices keep their `NameKey` and `Cell`s, as shadcn/ui's pies read their rows' `fill` | CHART-04, CHART-06, CHART-10, CHART-11 |
| Tooltip | `ChartTooltipContent`: the label, a line per payload entry, `indicator` dot, line or dashed, `hideLabel`, `hideIndicator` | done — `c:ChartTooltipContent` on both targets with a row per line however many (Pictor's `TooltipItems`, the content bringing its rows' template), the dashed indicator drawn dashed (`w-0 border-[1.5px] border-dashed`) | CHART-02, CHART-04, CHART-05, CHART-11 |
| Formatters | `labelFormatter`, `formatter`, `tickFormatter` | done — .NET composite formats (`LabelFormat`, `Format`, `TickFormat`; a date text formatted as its date) for what a format says, and value converters on Pictor's `LabelFormatter`, `Formatter` and `TickFormatter` (a function from code through `FuncValueConverter`), run by the web's writer when the app's generated converters create their class (else on Avalonia's engine path) | CHART-07, CHART-11 |
| Legend | `ChartLegendContent`: a centered row of squares and names, `nameKey`, classes such as `*:basis-1/4` | done — `c:ChartLegendContent` (`Spacing`, `ItemBasis`) with `c:ChartLegendItem` on both targets | CHART-03, CHART-06 |
| Charts | area, bar, line, pie, radar and radial examples | done — the 26 examples on both targets | FIDELITY-13, FIDELITY-14 |
| Interactive examples | a select of time ranges, header toggles of the series | done — a bound select and the `chart-toggles` list box, posted on the web | CHART-08, CHART-09 |
| Recharts 3 | shadcn/ui pins Recharts 3.8.0 | done — Pictor lays radial bar charts out and places default-index tooltips as Recharts 3.8.0 does (its): the radial and tooltip examples 98.1% to 98.8% on the desktop and 99.5% to 99.9% on the web against shadcn/ui's Recharts 3.8.0 render | FIDELITY-13, FIDELITY-14 |

## Blocks

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Login blocks | `login-01` to `login-05` (new-york-v4) | done — composed from Cosmo's components on both targets, as fidelity fixtures and `/blocks/{block}` pages, `w-full max-w-*` so they fit a phone, `login-04`'s card 896 px and one column below 768 px | BLOCKS-01, BLOCKS-02, BLOCKS-03, BLOCKS-05, BLOCKS-06, BLOCKS-07, FIDELITY-15, FIDELITY-16 |
| Signup blocks | `signup-01` to `signup-05` | done — as the login blocks | BLOCKS-01, BLOCKS-02, BLOCKS-03, BLOCKS-05, BLOCKS-06, BLOCKS-07, FIDELITY-15, FIDELITY-16 |
| Links in sentences | `<a>`s in `FieldDescription` (`Sign up`, `Terms of Service`) | done — underlined runs with `a:Web.Href`, `<a href>`s of their sentence on the web and runs that navigate on the desktop (Aioros `0.1.0-preview.489.e81f80b`) | BLOCKS-03 |
| Cover image | `/placeholder.svg`, `absolute inset-0 h-full w-full object-cover`, `dark:brightness-[0.2] dark:grayscale` | done — the Gallery's own placeholder covering its column on both targets (on the web) | BLOCKS-03, BLOCKS-04, FIDELITY-16 |
| Two-column pages | `login-02`, `signup-02`: `lg:grid-cols-2` with the image | n/a — at the reference's 1000 px viewport `lg:` does not apply, so the blocks show their form column; the `/blocks` pages follow the same layout |  |
| `dashboard-01` | inset sidebar, section cards, visitors chart, the document table (tabs, columns, pagination, selection, reviewer select, row drag, viewer drawer) | done — a Gallery page (`/blocks/dashboard-01`) and a fixture at 1280 by 800 on both targets; its boxes and its sidebar's and parts' interactions tested on both targets, compared with shadcn/ui's render on both | BLOCKS-09, BLOCKS-10, BLOCKS-11, BLOCKS-12, BLOCKS-13, BLOCKS-14, BLOCKS-15, BLOCKS-16, BLOCKS-17, BLOCKS-18, BLOCKS-19, BLOCKS-20, BLOCKS-21, BLOCKS-22, BLOCKS-23, FIDELITY-21, FIDELITY-22, DRAWER-12, DATATABLE-07, DATATABLE-08 |
| `sidebar-01` | version switcher, search, grouped links | done — a Gallery page (`/blocks/sidebar-01`) and a fixture at 1280 by 800 on both targets; its boxes and its sidebar's and parts' interactions tested on both targets, compared with shadcn/ui's render on both | BLOCKS-09, BLOCKS-10, BLOCKS-11, BLOCKS-12, BLOCKS-13, BLOCKS-14, BLOCKS-15, BLOCKS-16, BLOCKS-17, BLOCKS-18, BLOCKS-19, BLOCKS-20, BLOCKS-22, FIDELITY-21, FIDELITY-22 |
| `sidebar-02` | collapsible groups | done — a Gallery page (`/blocks/sidebar-02`) and a fixture at 1280 by 800 on both targets; its boxes and its sidebar's and parts' interactions tested on both targets, compared with shadcn/ui's render on both | BLOCKS-09, BLOCKS-10, BLOCKS-11, BLOCKS-12, BLOCKS-13, BLOCKS-14, BLOCKS-15, BLOCKS-16, BLOCKS-17, BLOCKS-18, BLOCKS-19, BLOCKS-20, BLOCKS-22, FIDELITY-21, FIDELITY-22 |
| `sidebar-03` | submenus | done — a Gallery page (`/blocks/sidebar-03`) and a fixture at 1280 by 800 on both targets; its boxes and its sidebar's and parts' interactions tested on both targets, compared with shadcn/ui's render on both | BLOCKS-09, BLOCKS-10, BLOCKS-11, BLOCKS-12, BLOCKS-13, BLOCKS-14, BLOCKS-15, BLOCKS-16, BLOCKS-17, BLOCKS-18, BLOCKS-19, BLOCKS-20, BLOCKS-22, FIDELITY-21, FIDELITY-22 |
| `sidebar-04` | floating, 304 px, flat submenus | done — a Gallery page (`/blocks/sidebar-04`) and a fixture at 1280 by 800 on both targets; its boxes and its sidebar's and parts' interactions tested on both targets, compared with shadcn/ui's render on both | BLOCKS-09, BLOCKS-10, BLOCKS-11, BLOCKS-12, BLOCKS-13, BLOCKS-14, BLOCKS-15, BLOCKS-16, BLOCKS-17, BLOCKS-18, BLOCKS-19, BLOCKS-20, BLOCKS-22, FIDELITY-21, FIDELITY-22 |
| `sidebar-05` | collapsible submenus with plus and minus | done — a Gallery page (`/blocks/sidebar-05`) and a fixture at 1280 by 800 on both targets; its boxes and its sidebar's and parts' interactions tested on both targets, compared with shadcn/ui's render on both | BLOCKS-09, BLOCKS-10, BLOCKS-11, BLOCKS-12, BLOCKS-13, BLOCKS-14, BLOCKS-15, BLOCKS-16, BLOCKS-17, BLOCKS-18, BLOCKS-19, BLOCKS-20, BLOCKS-22, FIDELITY-21, FIDELITY-22 |
| `sidebar-06` | submenus as dropdowns, opt-in card | done — a Gallery page (`/blocks/sidebar-06`) and a fixture at 1280 by 800 on both targets; its boxes and its sidebar's and parts' interactions tested on both targets, compared with shadcn/ui's render on both | BLOCKS-09, BLOCKS-10, BLOCKS-11, BLOCKS-12, BLOCKS-13, BLOCKS-14, BLOCKS-15, BLOCKS-16, BLOCKS-17, BLOCKS-18, BLOCKS-19, BLOCKS-20, BLOCKS-22, FIDELITY-21, FIDELITY-22 |
| `sidebar-07` | collapses to icons, team switcher, user menu | done — a Gallery page (`/blocks/sidebar-07`) and a fixture at 1280 by 800 on both targets; its boxes and its sidebar's and parts' interactions tested on both targets, compared with shadcn/ui's render on both | BLOCKS-09, BLOCKS-10, BLOCKS-11, BLOCKS-12, BLOCKS-13, BLOCKS-14, BLOCKS-15, BLOCKS-16, BLOCKS-17, BLOCKS-18, BLOCKS-19, BLOCKS-20, BLOCKS-22, FIDELITY-21, FIDELITY-22 |
| `sidebar-08` | inset, chevron actions, secondary links | done — a Gallery page (`/blocks/sidebar-08`) and a fixture at 1280 by 800 on both targets; its boxes and its sidebar's and parts' interactions tested on both targets, compared with shadcn/ui's render on both | BLOCKS-09, BLOCKS-10, BLOCKS-11, BLOCKS-12, BLOCKS-13, BLOCKS-14, BLOCKS-15, BLOCKS-16, BLOCKS-17, BLOCKS-18, BLOCKS-19, BLOCKS-20, BLOCKS-22, FIDELITY-21, FIDELITY-22, SIDEBAR-12 |
| `sidebar-09` | nested sidebars: icon rail and mail list, 350 px | done — a Gallery page (`/blocks/sidebar-09`) and a fixture at 1280 by 800 on both targets; its boxes and its sidebar's and parts' interactions tested on both targets, compared with shadcn/ui's render on both | BLOCKS-09, BLOCKS-10, BLOCKS-11, BLOCKS-12, BLOCKS-13, BLOCKS-14, BLOCKS-15, BLOCKS-16, BLOCKS-17, BLOCKS-18, BLOCKS-19, BLOCKS-20, BLOCKS-22, FIDELITY-21, FIDELITY-22 |
| `sidebar-10` | compact team switcher, favorites, workspaces, page actions popover open from the start | done — a Gallery page (`/blocks/sidebar-10`) and a fixture at 1280 by 800 on both targets; its boxes and its sidebar's and parts' interactions tested on both targets, compared with shadcn/ui's render on both | BLOCKS-09, BLOCKS-10, BLOCKS-11, BLOCKS-12, BLOCKS-13, BLOCKS-14, BLOCKS-15, BLOCKS-16, BLOCKS-17, BLOCKS-18, BLOCKS-19, BLOCKS-20, BLOCKS-22, FIDELITY-21, FIDELITY-22 |
| `sidebar-11` | changes and a collapsible file tree | done — a Gallery page (`/blocks/sidebar-11`) and a fixture at 1280 by 800 on both targets; its boxes and its sidebar's and parts' interactions tested on both targets, compared with shadcn/ui's render on both | BLOCKS-09, BLOCKS-10, BLOCKS-11, BLOCKS-12, BLOCKS-13, BLOCKS-14, BLOCKS-15, BLOCKS-16, BLOCKS-17, BLOCKS-18, BLOCKS-19, BLOCKS-20, BLOCKS-22, FIDELITY-21, FIDELITY-22 |
| `sidebar-12` | user, calendar (33 px days), collapsible calendars | done — a Gallery page (`/blocks/sidebar-12`) and a fixture at 1280 by 800 on both targets; its boxes and its sidebar's and parts' interactions tested on both targets, compared with shadcn/ui's render on both | BLOCKS-09, BLOCKS-10, BLOCKS-11, BLOCKS-12, BLOCKS-13, BLOCKS-14, BLOCKS-15, BLOCKS-16, BLOCKS-17, BLOCKS-18, BLOCKS-19, BLOCKS-20, BLOCKS-22, FIDELITY-21, FIDELITY-22 |
| `sidebar-13` | a sidebar in a settings dialog open from the start | done — a Gallery page (`/blocks/sidebar-13`) and a fixture at 1280 by 800 on both targets; its boxes and its sidebar's and parts' interactions tested on both targets, compared with shadcn/ui's render on both | BLOCKS-09, BLOCKS-10, BLOCKS-11, BLOCKS-12, BLOCKS-13, BLOCKS-14, BLOCKS-15, BLOCKS-16, BLOCKS-17, BLOCKS-18, BLOCKS-19, BLOCKS-20, BLOCKS-22, FIDELITY-21, FIDELITY-22 |
| `sidebar-14` | on the right, table of contents | done — a Gallery page (`/blocks/sidebar-14`) and a fixture at 1280 by 800 on both targets; its boxes and its sidebar's and parts' interactions tested on both targets, compared with shadcn/ui's render on both | BLOCKS-09, BLOCKS-10, BLOCKS-11, BLOCKS-12, BLOCKS-13, BLOCKS-14, BLOCKS-15, BLOCKS-16, BLOCKS-17, BLOCKS-18, BLOCKS-19, BLOCKS-20, BLOCKS-22, FIDELITY-21, FIDELITY-22 |
| `sidebar-15` | left (sidebar-10's) and right (sidebar-12's, `Collapsible="None"`) sidebars | done — a Gallery page (`/blocks/sidebar-15`) and a fixture at 1280 by 800 on both targets; its boxes and its sidebar's and parts' interactions tested on both targets, compared with shadcn/ui's render on both | BLOCKS-09, BLOCKS-10, BLOCKS-11, BLOCKS-12, BLOCKS-13, BLOCKS-14, BLOCKS-15, BLOCKS-16, BLOCKS-17, BLOCKS-18, BLOCKS-19, BLOCKS-20, BLOCKS-22, FIDELITY-21, FIDELITY-22, SIDEBAR-14 |
| `sidebar-16` | a sticky full-width site header above the sidebar | done — a Gallery page (`/blocks/sidebar-16`) and a fixture at 1280 by 800 on both targets; its boxes and its sidebar's and parts' interactions tested on both targets, compared with shadcn/ui's render on both | BLOCKS-09, BLOCKS-10, BLOCKS-11, BLOCKS-12, BLOCKS-13, BLOCKS-14, BLOCKS-15, BLOCKS-16, BLOCKS-17, BLOCKS-18, BLOCKS-19, BLOCKS-20, BLOCKS-22, FIDELITY-21, FIDELITY-22, SIDEBAR-14 |
| Other registry blocks | none: the pinned registry's blocks are `dashboard-01`, `login-01` to `login-05`, `signup-01` to `signup-05` and `sidebar-01` to `sidebar-16` (no calendar or other block) | n/a — every block of the pinned registry has its row |  |

## Chat

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Marker | `marker` with `MarkerIcon` and `MarkerContent`; `separator`, `border`; shimmer, status, link and button markers | done | MARKER-01..04 |
| Bubble variants | `default`, `secondary`, `muted`, `tinted` (a relative color of `--primary`), `outline`, `ghost`, `destructive` | done — the tinted and destructive colors generated per theme by `tokens.mjs` | BUBBLE-01..03 |
| Bubble width | `w-fit max-w-[80%]`, ghost `max-w-full` | done — star columns, written as a CSS grid; wrapping text fills the 80% | BUBBLE-01, BUBBLE-02 |
| Reactions | `BubbleReactions` on the bottom edge, `ring-3 ring-card` | done | BUBBLE-01, BUBBLE-02 |
| Message | avatar at the bottom, `align="end"`, header, footer, groups | done | MESSAGE-01, MESSAGE-02 |
| Attachment | media, title, description, actions, sizes, vertical images, a trigger over the card | done | ATTACHMENT-01, ATTACHMENT-03, ATTACHMENT-04, ATTACHMENT-05 |
| Attachment states | `idle` dashed, `uploading` and `processing` shimmering, `error` destructive | done — the shimmer animated on both targets | ATTACHMENT-02, ATTACHMENT-05, THEME-09 |
| Message scroller | opening at the end or the start, following new messages, the scroll-to-end button (`MessageScroller` of `@shadcn/react` 0.3.1), its bottom fade | done — on both targets (Aioros LAYOUT-90, LAYOUT-93) | MESSAGESCROLLER-01..06 |
| Code block | header, actions, copy, line numbers, highlighted lines, diffs, `destructive`, `sm`, wrapping, highlighting (the radix base in radix-rhea); floating actions, show more, tabs, ANSI, right to left, typeset, markdown, heights | done — the streaming and markdown examples played back at shadcn/ui's pace on both targets | CODEBLOCK-01..13 |
| Trace | lines with icons, status, descriptions and trailing icons; collapsible steps; links, dialogs, sheets, sources, commits, turns | done — the running shimmer animated; the streamed examples played back at shadcn/ui's pace on both targets | TRACE-01..09 |
| Questionnaire | one question at a time, single and multiple choices, a freeform answer, letter shortcuts, required and skipped questions, Submit with the answers; progress, answer-gated navigation, reset, conditional, controlled, resumed and validated questionnaires, keys | done — on both targets, every step on the web since | QUESTIONNAIRE-01..10 |
| Markdown | Streamdown's CommonMark in the chat examples: paragraphs, headings, lists, quotes, inline code, links, fenced code blocks highlighted by shiki | done — `c:Markdown` (Markdig, in `Aioros.Cosmo.Markdown`), highlighted at build time | CODEBLOCK-08, CODEBLOCK-09, BUBBLE-04, BUBBLE-05 |
| Chat utilities | the `shadcn` package's `shimmer` and `scroll-fade` utilities | done — on both targets (Aioros TEXT-54, LAYOUT-90, LAYOUT-93) | THEME-09, THEME-10 |

## Theming

| Feature | shadcn/ui | Status | Criteria |
| --- | --- | --- | --- |
| Tokens | 31 CSS variables in oklch, light and dark | done | TOKENS-01, TOKENS-02, TOKENS-05 |
| Component opacities and shadows | `/90`, `/80`, `/50`, `/30`, `shadow-xs`, `shadow-sm`, ring | done | TOKENS-04 |
| Radius | `--radius` and `--radius-sm` … `--radius-4xl` | done | TOKENS-03, THEME-02 |
| Color themes | neutral, zinc, slate, stone, gray, red, rose, orange, green, blue, yellow, violet | done | THEME-02, THEME-05 |
| Light and dark at runtime | `.dark` class | done | THEME-01, THEME-06 |
| Overriding a token | editing `globals.css` | done | THEME-03 |
| A themed region | `.theme-*` class on a container | done | THEME-04, THEME-05 |
| Font | Geist (`--font-sans`), Geist Mono | done | FIDELITY-01, BADGE-01 |

## Fidelity matrix

| Component | Light | Dark | Hover | Focus | Disabled | Desktop | Web | Binding |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Tokens | TOKENS-01, TOKENS-02 | TOKENS-01, TOKENS-02 | TOKENS-04 | TOKENS-04 | n/a — no disabled token | TOKENS-01, TOKENS-05 | TOKENS-02 | n/a — resources, not bound |
| Theming | THEME-01, THEME-06 | THEME-01, THEME-06 | n/a — no hover of its own | n/a — no focus of its own | n/a — no disabled state | THEME-02, THEME-03, THEME-04 | THEME-05, THEME-06 | THEME-05 |
| Button | BUTTON-03, FIDELITY-01 | BUTTON-03, FIDELITY-01 | BUTTON-02, BUTTON-05 | BUTTON-06, BUTTON-10, BUTTON-12, BUTTON-13 | BUTTON-03, BUTTON-07 | BUTTON-02, BUTTON-08, BUTTON-14 | BUTTON-01, BUTTON-04, BUTTON-09, BUTTON-15 | BINDING-01, BINDING-02, BINDING-04 |
| Badge | BADGE-01, FIDELITY-01 | BADGE-01, FIDELITY-01 | BADGE-04, BADGE-05 | BADGE-04, BADGE-05 | n/a — a badge has no disabled state | BADGE-02, BADGE-03, BADGE-04 | BADGE-01, BADGE-05 | BINDING-03, BINDING-05, BINDING-06 |
| Card | CARD-01, FIDELITY-01 | CARD-01, FIDELITY-01 | n/a — a card has no hover look | n/a — a card takes no focus | n/a — a card has no disabled state | CARD-02, CARD-03, CARD-04 | CARD-01, CARD-05 | BINDING-01, BINDING-02 |
| Label | LABEL-01 | LABEL-01 | n/a — a label has no hover look | n/a — a label takes no focus; it gives it to its target (LABEL-02) | LABEL-02 | LABEL-02 | LABEL-01, LABEL-04, LABEL-05 | LABEL-03, LABEL-04 |
| Input | INPUT-02, FIDELITY-02 | INPUT-02, FIDELITY-02 | n/a — shadcn's input has no hover look | INPUT-03, INPUT-04 | INPUT-02, INPUT-04 | INPUT-04, INPUT-05, INPUT-07, INPUT-09 | INPUT-01, INPUT-02, INPUT-06, INPUT-08, INPUT-10 | INPUT-05, INPUT-06 |
| Textarea | TEXTAREA-01, FIDELITY-02 | TEXTAREA-01, FIDELITY-02 | n/a — shadcn's textarea has no hover look | INPUT-04 | TEXTAREA-01 | TEXTAREA-02 | TEXTAREA-01 | INPUT-05 |
| Field | FIELD-01, FIELD-02, FIELD-08, FIELD-11, FIELD-15, FIDELITY-02 | FIELD-01, FIELD-02, FIELD-08, FIELD-11, FIELD-15, FIDELITY-02 | n/a — a field has no hover look | n/a — a field's controls take the focus | n/a — a field has no disabled look of its own; its label and controls do (LABEL-02) | FIELD-01, FIELD-03, FIELD-05, FIELD-08, FIELD-09, FIELD-15 | FIELD-02, FIELD-04, FIELD-10, FIELD-11, FIELD-12, FIELD-16 | FIELD-04, FIELD-05, FIELD-13, FIELD-14 |
| Select | NATIVESELECT-02, SELECT-01, SELECT-07, SELECT-09, FIDELITY-02 | NATIVESELECT-02, SELECT-01, SELECT-07, SELECT-09, FIDELITY-02 | SELECT-01, SELECT-02, SELECT-08, SELECT-10 | SELECT-01 | SELECT-01, NATIVESELECT-02 | SELECT-01, SELECT-02, SELECT-03, SELECT-04, SELECT-07, SELECT-08, SELECT-11, SELECT-13 | NATIVESELECT-01, NATIVESELECT-02, SELECT-09, SELECT-10, SELECT-12 | SELECT-04, NATIVESELECT-01 |
| Checkbox | CHECKBOX-02, CHECKBOX-03, FIDELITY-03 | CHECKBOX-02, CHECKBOX-03, FIDELITY-03 | n/a — shadcn's checkbox has no hover look | CHECKBOX-03 | CHECKBOX-02, CHECKBOX-03 | CHECKBOX-03, CHECKBOX-04 | CHECKBOX-01, CHECKBOX-02, CHECKBOX-05 | CHECKBOX-04, CHECKBOX-05 |
| Radio Group | RADIO-02, RADIO-03 | RADIO-02, RADIO-03 | n/a — shadcn's radio has no hover look | RADIO-03 | RADIO-02, RADIO-03 | RADIO-03, RADIO-04 | RADIO-01, RADIO-02, RADIO-05 | RADIO-04, RADIO-05 |
| Switch | SWITCH-02, SWITCH-03 | SWITCH-02, SWITCH-03 | n/a — shadcn's switch has no hover look | SWITCH-03 | SWITCH-02, SWITCH-03 | SWITCH-03, SWITCH-04 | SWITCH-01, SWITCH-02, SWITCH-05 | SWITCH-04, SWITCH-05 |
| Toggle | TOGGLE-02, TOGGLE-03, FIDELITY-03 | TOGGLE-02, TOGGLE-03, FIDELITY-03 | TOGGLE-02, TOGGLE-03 | TOGGLE-03 | TOGGLE-02, TOGGLE-03 | TOGGLE-03, TOGGLE-04 | TOGGLE-01, TOGGLE-02 | TOGGLE-04 |
| Toggle Group | TOGGLEGROUP-02, TOGGLEGROUP-03, FIDELITY-03 | TOGGLEGROUP-02, TOGGLEGROUP-03, FIDELITY-03 | TOGGLEGROUP-02 | n/a — the items' focus ring is the toggle's (TOGGLE-03) | n/a — shadcn's fixture has no disabled group; items are disabled as toggles | TOGGLEGROUP-03, TOGGLEGROUP-04 | TOGGLEGROUP-01, TOGGLEGROUP-02, TOGGLEGROUP-05 | TOGGLEGROUP-04, TOGGLEGROUP-05 |
| Slider | SLIDER-02, SLIDER-05, SLIDER-07, FIDELITY-04 | SLIDER-02, SLIDER-05, SLIDER-07, FIDELITY-04 | SLIDER-02 | SLIDER-02 | SLIDER-01, SLIDER-02, SLIDER-06 | SLIDER-02, SLIDER-03, SLIDER-05, SLIDER-06 | SLIDER-01, SLIDER-04, SLIDER-07, SLIDER-08 | SLIDER-03, SLIDER-04, SLIDER-06, SLIDER-08 |
| Progress | PROGRESS-01, PROGRESS-02 | PROGRESS-01, PROGRESS-02 | n/a — a progress bar has no hover look | n/a — a progress bar takes no focus | n/a — a progress bar has no disabled state | PROGRESS-02 | PROGRESS-01, PROGRESS-03 | PROGRESS-03 |
| Input Group | INPUTGROUP-01, INPUTGROUP-03 | INPUTGROUP-01, INPUTGROUP-03 | n/a — an input group has no hover look | INPUTGROUP-02 | n/a — shadcn's fixture has no disabled group; its text box is disabled as an input (INPUT-04) | INPUTGROUP-01, INPUTGROUP-02 | INPUTGROUP-03, INPUTGROUP-04 | INPUTGROUP-04 |
| Input OTP | INPUTOTP-01, FIDELITY-04 | INPUTOTP-01, FIDELITY-04 | n/a — shadcn's code boxes have no hover look | INPUTOTP-02 | INPUTOTP-02 | INPUTOTP-02 | INPUTOTP-01, INPUTOTP-03, INPUTOTP-04 | INPUTOTP-02, INPUTOTP-03 |
| Popover | POPOVER-01, FIDELITY-05, FIDELITY-06 | POPOVER-01, FIDELITY-05, FIDELITY-06 | n/a — shadcn's popover content has no hover look | POPOVER-02, POPOVER-04 | n/a — a popover has no disabled state; its trigger is a button (BUTTON-07) | POPOVER-01, POPOVER-02, FIDELITY-06, POPOVER-07 | POPOVER-04, FIDELITY-05 | POPOVER-03 |
| Tooltip | TOOLTIP-01, FIDELITY-05, FIDELITY-06 | TOOLTIP-01, FIDELITY-05, FIDELITY-06 | TOOLTIP-02, TOOLTIP-04 | TOOLTIP-02, TOOLTIP-04 | n/a — a tip has no disabled state (Avalonia's `ShowOnDisabled` shows it on a disabled control) | TOOLTIP-01, TOOLTIP-02, FIDELITY-06, TOOLTIP-06 | TOOLTIP-04, FIDELITY-05, TOOLTIP-07 | TOOLTIP-03 |
| Hover Card | HOVERCARD-01, FIDELITY-06 | HOVERCARD-01, FIDELITY-06 | HOVERCARD-02 | HOVERCARD-02 | n/a — a hover card has no disabled state | HOVERCARD-01, HOVERCARD-02, FIDELITY-06, HOVERCARD-07 | HOVERCARD-04 | HOVERCARD-03 |
| Dropdown Menu | DROPDOWN-01, FIDELITY-05, FIDELITY-06 | DROPDOWN-01, FIDELITY-05, FIDELITY-06 | DROPDOWN-01, DROPDOWN-02 | DROPDOWN-03 | DROPDOWN-01, DROPDOWN-04 | DROPDOWN-01, DROPDOWN-02, DROPDOWN-03, FIDELITY-06, DROPDOWN-08 | DROPDOWN-05, FIDELITY-05 | DROPDOWN-04, DROPDOWN-05 |
| Context Menu | CONTEXTMENU-01, FIDELITY-06 | CONTEXTMENU-01, FIDELITY-06 | DROPDOWN-02 | CONTEXTMENU-02 | DROPDOWN-01 | CONTEXTMENU-01, CONTEXTMENU-02, FIDELITY-06, CONTEXTMENU-07 | CONTEXTMENU-04 | CONTEXTMENU-03 |
| Dialog | DIALOG-01, FIDELITY-05, FIDELITY-06 | DIALOG-01, FIDELITY-05, FIDELITY-06 | DIALOG-01 | DIALOG-02 | n/a — a dialog has no disabled state; its buttons are buttons (BUTTON-07) | DIALOG-01, DIALOG-02, FIDELITY-06, DIALOG-09 | DIALOG-04, FIDELITY-05 | DIALOG-03, DIALOG-04 |
| Alert Dialog | ALERTDIALOG-01, FIDELITY-05, FIDELITY-06 | ALERTDIALOG-01, FIDELITY-05, FIDELITY-06 | n/a — an alert dialog has no hover look of its own | ALERTDIALOG-02 | n/a — an alert dialog has no disabled state | ALERTDIALOG-01, ALERTDIALOG-02, FIDELITY-06, ALERTDIALOG-08 | ALERTDIALOG-04, FIDELITY-05 | ALERTDIALOG-03, ALERTDIALOG-04 |
| Sheet | SHEET-01, FIDELITY-05, FIDELITY-06 | SHEET-01, FIDELITY-05, FIDELITY-06 | DIALOG-01 | SHEET-02 | n/a — a sheet has no disabled state | SHEET-01, SHEET-02, FIDELITY-06, SHEET-08 | SHEET-04, FIDELITY-05 | SHEET-03 |
| Drawer | DRAWER-01, FIDELITY-05, FIDELITY-06 | DRAWER-01, FIDELITY-05, FIDELITY-06 | n/a — a drawer has no hover look of its own | DRAWER-02 | n/a — a drawer has no disabled state | DRAWER-01, DRAWER-02, FIDELITY-06, DRAWER-08, DRAWER-09 | DRAWER-04, FIDELITY-05, DRAWER-10 | DRAWER-03 |
| Sonner | SONNER-01, SONNER-10, FIDELITY-06 | SONNER-01, SONNER-10, FIDELITY-06 | SONNER-02, SONNER-10, SONNER-11 | n/a — a toast takes no focus of its own; its action is a button (BUTTON-06) | n/a — a toast has no disabled state | SONNER-01, SONNER-02, SONNER-03, FIDELITY-06, SONNER-07, SONNER-08, SONNER-10 | SONNER-04, SONNER-09, SONNER-11, SONNER-12, SONNER-13 | SONNER-03, SONNER-04 |
| Separator | SEPARATOR-01, SEPARATOR-02, FIDELITY-07 | SEPARATOR-01, SEPARATOR-02, FIDELITY-07 | n/a — a separator has no hover look | n/a — a separator takes no focus | n/a — a separator has no disabled state | SEPARATOR-02, SEPARATOR-03, FIDELITY-08 | SEPARATOR-01, FIDELITY-07 | SEPARATOR-03 |
| Skeleton | SKELETON-01, SKELETON-02, FIDELITY-07 | SKELETON-01, SKELETON-02, FIDELITY-07 | n/a — a skeleton has no hover look | n/a — a skeleton takes no focus | n/a — a skeleton has no disabled state | SKELETON-02, SKELETON-03, FIDELITY-08 | SKELETON-01, FIDELITY-07 | SKELETON-03 |
| Spinner | SPINNER-01, SPINNER-02, FIDELITY-07 | SPINNER-01, SPINNER-02, FIDELITY-07 | n/a — a spinner has no hover look | n/a — a spinner takes no focus | SPINNER-01, SPINNER-02 | SPINNER-02, SPINNER-03, FIDELITY-08 | SPINNER-01, FIDELITY-07 | SPINNER-03 |
| Kbd | KBD-01, KBD-02, FIDELITY-07 | KBD-01, KBD-02, FIDELITY-07 | n/a — a key takes no pointer (`pointer-events-none`) | n/a — a key takes no focus | n/a — a key has no disabled state | KBD-02, KBD-04, FIDELITY-08, KBD-05 | KBD-01, KBD-03, FIDELITY-07, KBD-06 | KBD-03, KBD-04 |
| Avatar | AVATAR-01, AVATAR-02, FIDELITY-07 | AVATAR-01, AVATAR-02, FIDELITY-07 | n/a — an avatar has no hover look | n/a — an avatar takes no focus | n/a — an avatar has no disabled state | AVATAR-02, AVATAR-03, AVATAR-05, FIDELITY-08 | AVATAR-01, AVATAR-04, AVATAR-06, FIDELITY-07 | AVATAR-03, AVATAR-04 |
| Alert | ALERT-01, ALERT-02, FIDELITY-07 | ALERT-01, ALERT-02, FIDELITY-07 | n/a — an alert has no hover look | n/a — an alert takes no focus | n/a — an alert has no disabled state | ALERT-02, ALERT-03, FIDELITY-08 | ALERT-01, ALERT-04, FIDELITY-07 | ALERT-03, ALERT-04 |
| Aspect Ratio | ASPECTRATIO-01, FIDELITY-07 | ASPECTRATIO-01, FIDELITY-07 | n/a — a layout has no hover look | n/a — a layout takes no focus | n/a — a layout has no disabled state | ASPECTRATIO-02, ASPECTRATIO-03, FIDELITY-08 | ASPECTRATIO-01, FIDELITY-07 | ASPECTRATIO-03 |
| Typography | TYPOGRAPHY-01, TYPOGRAPHY-02, FIDELITY-07 | TYPOGRAPHY-01, TYPOGRAPHY-02, FIDELITY-07 | n/a — text has no hover look | n/a — text takes no focus | n/a — text has no disabled state | TYPOGRAPHY-02, TYPOGRAPHY-03, FIDELITY-08, TYPOGRAPHY-05 | TYPOGRAPHY-01, FIDELITY-07, TYPOGRAPHY-04, TYPOGRAPHY-06 | TYPOGRAPHY-03 |
| Empty | EMPTY-01, EMPTY-02, FIDELITY-07 | EMPTY-01, EMPTY-02, FIDELITY-07 | n/a — an empty state has no hover look of its own; its actions are buttons (BUTTON-02) | n/a — its actions take the focus (BUTTON-06) | n/a — an empty state has no disabled state | EMPTY-02, EMPTY-03, FIDELITY-08 | EMPTY-01, FIDELITY-07 | EMPTY-03 |
| Item | ITEM-01, ITEM-02, FIDELITY-07 | ITEM-01, ITEM-02, FIDELITY-07 | ITEM-02 | n/a — shadcn's focus ring is the link's; an item without a link takes no focus | n/a — an item has no disabled state | ITEM-02, ITEM-03, FIDELITY-08, ITEM-05, ITEM-07 | ITEM-01, ITEM-04, FIDELITY-07, ITEM-06, ITEM-08 | ITEM-03, ITEM-04 |
| Tabs | TABS-01, TABS-02, FIDELITY-07 | TABS-01, TABS-02, FIDELITY-07 | TABS-02, TABS-07, TABS-09 | TABS-03, TABS-07, TABS-09, TABS-10, TABS-11 | TABS-02 | TABS-02, TABS-03, TABS-04, TABS-06, TABS-07, TABS-10, FIDELITY-08 | TABS-01, TABS-05, TABS-08, TABS-09, TABS-11, FIDELITY-07 | TABS-04, TABS-05 |
| Accordion | ACCORDION-01, ACCORDION-02, FIDELITY-07 | ACCORDION-01, ACCORDION-02, FIDELITY-07 | ACCORDION-02 | ACCORDION-03 | n/a — shadcn's demo has no disabled item; a disabled header is 50% (`CosmoAccordionTrigger`) | ACCORDION-02, ACCORDION-03, ACCORDION-04, FIDELITY-08 | ACCORDION-01, FIDELITY-07 | ACCORDION-05 |
| Collapsible | COLLAPSIBLE-01, COLLAPSIBLE-02, FIDELITY-07 | COLLAPSIBLE-01, COLLAPSIBLE-02, FIDELITY-07 | COLLAPSIBLE-02 | COLLAPSIBLE-03 | n/a — a collapsible has no disabled look of its own | COLLAPSIBLE-02, COLLAPSIBLE-03, FIDELITY-08 | COLLAPSIBLE-01, FIDELITY-07 | COLLAPSIBLE-03 |
| Breadcrumb | BREADCRUMB-01, BREADCRUMB-02, FIDELITY-07 | BREADCRUMB-01, BREADCRUMB-02, FIDELITY-07 | BREADCRUMB-01, BREADCRUMB-02 | n/a — the links' focus is the browser's and the button theme's ring | n/a — a breadcrumb has no disabled state | BREADCRUMB-02, BREADCRUMB-03, FIDELITY-08 | BREADCRUMB-01, FIDELITY-07, BREADCRUMB-04 | BREADCRUMB-03 |
| Pagination | PAGINATION-01, PAGINATION-02, FIDELITY-07 | PAGINATION-01, PAGINATION-02, FIDELITY-07 | PAGINATION-02 | n/a — the buttons' focus ring is the button's (BUTTON-06) | n/a — shadcn's demo has no disabled link | PAGINATION-02, PAGINATION-03, FIDELITY-08 | PAGINATION-01, PAGINATION-04, FIDELITY-07, PAGINATION-06 | PAGINATION-03, PAGINATION-04 |
| Button Group | BUTTONGROUP-01, BUTTONGROUP-02, FIDELITY-07 | BUTTONGROUP-01, BUTTONGROUP-02, FIDELITY-07 | n/a — the children's hover looks are their own (BUTTON-02) | BUTTONGROUP-02 | n/a — the children's disabled looks are their own (BUTTON-07) | BUTTONGROUP-02, BUTTONGROUP-03, FIDELITY-08 | BUTTONGROUP-01, BUTTONGROUP-04, FIDELITY-07 | BUTTONGROUP-03, BUTTONGROUP-04 |
| Scroll Area | SCROLLAREA-01, SCROLLAREA-02, FIDELITY-07 | SCROLLAREA-01, SCROLLAREA-02, FIDELITY-07 | SCROLLAREA-02 | n/a — shadcn's viewport ring needs a focusable viewport; Cosmo's area is not focusable | n/a — no disabled scroll area | SCROLLAREA-02, SCROLLAREA-03, FIDELITY-08, SCROLLAREA-04 | SCROLLAREA-01, FIDELITY-07, SCROLLAREA-05 | SCROLLAREA-03 |
| Table | TABLE-01, TABLE-02, FIDELITY-07 | TABLE-01, TABLE-02, FIDELITY-07 | TABLE-02 | n/a — shadcn's rows take no focus | n/a — no disabled row in shadcn's table | TABLE-02, TABLE-03, FIDELITY-08 | TABLE-01, TABLE-04, FIDELITY-07 | TABLE-03, TABLE-04 |
| Navigation Menu | NAVMENU-01, NAVMENU-02, FIDELITY-05 | NAVMENU-01, NAVMENU-02, FIDELITY-05 | NAVMENU-02, NAVMENU-03 | NAVMENU-03 | n/a — shadcn's demo has no disabled trigger | NAVMENU-02, NAVMENU-03, FIDELITY-08, NAVMENU-08 | NAVMENU-01, NAVMENU-05, FIDELITY-05, NAVMENU-09, NAVMENU-10 | NAVMENU-04, NAVMENU-05 |
| Resizable | RESIZABLE-01, RESIZABLE-02, FIDELITY-09 | RESIZABLE-01, RESIZABLE-02, FIDELITY-09 | n/a — shadcn's handle has no hover look | RESIZABLE-02 | n/a — react-resizable-panels' disabled group is not in shadcn's examples | RESIZABLE-02, RESIZABLE-03, FIDELITY-10, RESIZABLE-05 | RESIZABLE-01, FIDELITY-09 | RESIZABLE-04, RESIZABLE-06 |
| Carousel | CAROUSEL-01, CAROUSEL-02, FIDELITY-09 | CAROUSEL-01, CAROUSEL-02, FIDELITY-09 | n/a — the buttons' hover looks are the outline button's (BUTTON-02) | n/a — the buttons' focus ring is the button's (BUTTON-06) | CAROUSEL-02, CAROUSEL-03 | CAROUSEL-02, CAROUSEL-03, FIDELITY-10 | CAROUSEL-01, FIDELITY-09, CAROUSEL-06 | CAROUSEL-04, CAROUSEL-05, CAROUSEL-07 |
| Command | COMMAND-01, COMMAND-02, FIDELITY-09, FIDELITY-10 | COMMAND-01, COMMAND-02, FIDELITY-09, FIDELITY-10 | COMMAND-04 | n/a — the focus stays in the input, whose ring cmdk hides (`outline-hidden`) | COMMAND-01, COMMAND-02 | COMMAND-02, COMMAND-04, COMMAND-06, FIDELITY-10, COMMAND-08 | COMMAND-01, FIDELITY-09, COMMAND-10 | COMMAND-05, COMMAND-09, COMMAND-11, COMMAND-12 |
| Combobox | COMBOBOX-01, COMBOBOX-02, FIDELITY-05, FIDELITY-09 | COMBOBOX-01, COMBOBOX-02, FIDELITY-05, FIDELITY-09 | n/a — the trigger's hover look is the outline button's (BUTTON-02) | n/a — the trigger's focus ring is the outline button's (BUTTON-06) | n/a — shadcn's demo has no disabled combobox; its trigger is a button (BUTTON-07) | COMBOBOX-02, COMBOBOX-03, FIDELITY-10 | COMBOBOX-01, FIDELITY-05, FIDELITY-09, COMBOBOX-06 | COMBOBOX-04 |
| Data Table | DATATABLE-01, DATATABLE-03, FIDELITY-09, FIDELITY-10 | DATATABLE-01, DATATABLE-03, FIDELITY-09, FIDELITY-10 | n/a — the rows' hover look is the table's (TABLE-02) | n/a — the controls' focus rings are the checkbox's and the button's (CHECKBOX-03, BUTTON-06) | DATATABLE-01, DATATABLE-03 | DATATABLE-03, DATATABLE-04, DATATABLE-05, FIDELITY-10 | DATATABLE-01, DATATABLE-02, FIDELITY-09 | DATATABLE-02, DATATABLE-05 |
| Sidebar | SIDEBAR-01, SIDEBAR-02, FIDELITY-09, FIDELITY-10 | SIDEBAR-01, SIDEBAR-02, FIDELITY-09, FIDELITY-10 | SIDEBAR-02 | n/a — the buttons' ring is `ring-sidebar-ring`, drawn on the desktop by the button's template (`PART_Ring`) | n/a — shadcn's examples have no disabled item; a disabled button is at 50% (`CosmoSidebarMenuButton`) | SIDEBAR-02, SIDEBAR-03, SIDEBAR-04, SIDEBAR-05, FIDELITY-10 | SIDEBAR-01, FIDELITY-09 | SIDEBAR-06, SIDEBAR-10 |
| Menubar | MENUBAR-01, MENUBAR-02, FIDELITY-05 | MENUBAR-01, MENUBAR-02, FIDELITY-05 | MENUBAR-02 | MENUBAR-03 | MENUBAR-02 | MENUBAR-02, MENUBAR-03, FIDELITY-08, MENUBAR-08 | MENUBAR-01, MENUBAR-05, FIDELITY-05 | MENUBAR-04, MENUBAR-05 |
| Calendar | CALENDAR-01, CALENDAR-02, FIDELITY-11 | CALENDAR-01, CALENDAR-02, FIDELITY-11 | n/a — the days' hover look is the ghost button's accent (BUTTON-02) | n/a — Avalonia's focused day takes the theme's ring as the buttons do (BUTTON-06) | CALENDAR-02 | CALENDAR-01, CALENDAR-02, FIDELITY-11 | CALENDAR-04 | CALENDAR-03 |
| Date Picker | DATEPICKER-01, FIDELITY-11 | DATEPICKER-01, FIDELITY-11 | n/a — the button's hover look is the outline button's (BUTTON-02) | n/a — the button's focus ring is the button's (BUTTON-06) | n/a — shadcn's picker has no disabled example | DATEPICKER-01, FIDELITY-11 | DATEPICKER-02 | DATEPICKER-02 |
| Chart | CHART-01, FIDELITY-13, FIDELITY-14 | CHART-01, FIDELITY-13, FIDELITY-14 | CHART-04, CHART-05 | n/a — shadcn/ui's examples take no focus of their own (`accessibilityLayer` is Recharts' keyboard layer, not drawn) | n/a — a chart has no disabled state | CHART-02, CHART-03, FIDELITY-13 | CHART-04, CHART-06, FIDELITY-14 | CHART-07, CHART-08, CHART-09 |
| Blocks | BLOCKS-01, BLOCKS-09, BLOCKS-10, FIDELITY-15, FIDELITY-16, FIDELITY-21, FIDELITY-22 | BLOCKS-01, BLOCKS-09, FIDELITY-15, FIDELITY-16, FIDELITY-21, FIDELITY-22 | n/a — a block has no hover look of its own; its buttons and links are the button's (BUTTON-02) | n/a — its controls take the focus (BUTTON-06, INPUT-03) | n/a — shadcn/ui's blocks have no disabled state | BLOCKS-01, BLOCKS-04, FIDELITY-15 | BLOCKS-02, BLOCKS-03, FIDELITY-16 | BLOCKS-05 |
| Marker | MARKER-01, MARKER-03, FIDELITY-17, FIDELITY-18 | MARKER-01, MARKER-03, FIDELITY-17, FIDELITY-18 | MARKER-03 | n/a — a marker takes no focus; a link or button marker takes it as a link or a button | n/a — a marker has no disabled state | MARKER-01, MARKER-03, FIDELITY-17 | MARKER-02, MARKER-04, FIDELITY-18 | n/a — the examples bind nothing; its content binds as a `ContentControl`'s |
| Bubble | BUBBLE-01, BUBBLE-03, BUBBLE-04, FIDELITY-17 | BUBBLE-01, BUBBLE-03, BUBBLE-04, FIDELITY-17 | BUBBLE-04 | n/a — a link or button bubble takes the focus as a link or a button | n/a — a bubble has no disabled state | BUBBLE-01, BUBBLE-04, FIDELITY-17 | BUBBLE-02, BUBBLE-05, FIDELITY-18 | n/a — the examples bind nothing; its content binds as a `ContentControl`'s, a collapsible's `IsOpen` two way |
| Message | MESSAGE-01, FIDELITY-17, FIDELITY-18 | MESSAGE-01, FIDELITY-17, FIDELITY-18 | n/a — a message has no hover look | n/a — a message takes no focus | n/a — a message has no disabled state | MESSAGE-01, FIDELITY-17 | MESSAGE-02, FIDELITY-18 | n/a — the examples bind nothing; its parts bind as controls do |
| Attachment | ATTACHMENT-01, ATTACHMENT-02, ATTACHMENT-04, FIDELITY-17 | ATTACHMENT-01, ATTACHMENT-02, ATTACHMENT-04, FIDELITY-17 | ATTACHMENT-04 | n/a — its actions and trigger take the focus as buttons (BUTTON-06) | n/a — shadcn/ui's states are idle, uploading, processing, error and done | ATTACHMENT-01, ATTACHMENT-02, ATTACHMENT-04, FIDELITY-17 | ATTACHMENT-03, ATTACHMENT-05, FIDELITY-18 | n/a — the examples bind nothing; `State` is a styled property |
| Message Scroller | MESSAGESCROLLER-01, FIDELITY-17 | MESSAGESCROLLER-01, FIDELITY-17 | n/a — only the button has a hover look (`--muted`), which shadcn/ui's capture does not show | n/a — the button's focus ring is the button's (BUTTON-06) | n/a — a scroller has no disabled state | MESSAGESCROLLER-01, MESSAGESCROLLER-02, MESSAGESCROLLER-05, FIDELITY-17 | MESSAGESCROLLER-03, MESSAGESCROLLER-06, FIDELITY-18 | n/a — the example binds nothing; the transcript is the content |
| Code Block | CODEBLOCK-01, FIDELITY-19 | CODEBLOCK-01, FIDELITY-19 | n/a — only the actions have a hover look (`--foreground` at 5%), which shadcn/ui's capture does not show | n/a — the content's focus ring (rhea's `ring-3` at 30% of `--ring`) is not drawn; the actions take the focus as buttons (BUTTON-06) | n/a — a code block has no disabled state | CODEBLOCK-01, CODEBLOCK-03, CODEBLOCK-04, CODEBLOCK-06, CODEBLOCK-08, FIDELITY-19 | CODEBLOCK-02, CODEBLOCK-07, CODEBLOCK-09, FIDELITY-20 | n/a — the examples bind nothing; the code is the content |
| Trace | TRACE-01, FIDELITY-19 | TRACE-01, FIDELITY-19 | TRACE-06 | n/a — a trigger's focus ring (rhea's `ring-3`) is not drawn | n/a — a trace has no disabled state | TRACE-01, TRACE-03, TRACE-04, TRACE-06, FIDELITY-19 | TRACE-02, TRACE-05, TRACE-07, FIDELITY-20 | n/a — the examples bind nothing; `Status` is a styled property |
| Questionnaire | QUESTIONNAIRE-01, FIDELITY-19 | QUESTIONNAIRE-01, FIDELITY-19 | n/a — a choice's hover look (`--input` at 40%) is not compared; shadcn/ui's capture shows none | n/a — the choices' focus ring (rhea's `ring-3` on the card) is not drawn; the actions take the focus as buttons (BUTTON-06) | QUESTIONNAIRE-07, QUESTIONNAIRE-08 | QUESTIONNAIRE-01, QUESTIONNAIRE-03, QUESTIONNAIRE-04, QUESTIONNAIRE-05, QUESTIONNAIRE-07, QUESTIONNAIRE-09, FIDELITY-19 | QUESTIONNAIRE-02, QUESTIONNAIRE-06, QUESTIONNAIRE-08, QUESTIONNAIRE-10, FIDELITY-20 | QUESTIONNAIRE-05, QUESTIONNAIRE-07 |
