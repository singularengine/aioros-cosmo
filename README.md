<img src="assets/icon/cosmo.svg" alt="Cosmo" width="72" height="72" align="right">

# Aioros.Cosmo

**UI components for the [Aioros](https://github.com/singularengine/aioros) ecosystem, in pure XAML, on the web and natively.**

> **Public preview.** APIs can change between versions and it is not ready for production yet.

Aioros.Cosmo gives you the components, variants, design tokens and light and dark themes of [shadcn/ui](https://ui.shadcn.com) as XAML. Where Avalonia already has a control, a Cosmo component is that control with a theme: a Cosmo button is a `Button` with `Classes="outline sm"`. Where it has none, Cosmo adds a small sealed control, such as `c:Badge` or `c:Card`.

- **On the web**, an Aioros page is written as HTML on each request, with no JavaScript of your own. A Cosmo button is a real `<button>`.
- **On the desktop**, the same XAML runs natively on Linux, Windows and macOS.

## Start an app

The quickest way in is the template. It creates an app with a dashboard, a sidebar, an interactive chart, a data table and a settings page:

```bash
dotnet tool install -g Aioros.Cli --prerelease
dotnet new install Aioros.Cosmo.Templates@0.1.0-preview.1
dotnet new cosmo -n Shop --color Blue --radius Large
cd Shop
aioros dev
```

`--color` takes `Neutral`, `Zinc`, `Slate`, `Stone`, `Gray`, `Red`, `Rose`, `Orange`, `Green`, `Blue`, `Yellow` or `Violet`. `--radius` takes `Default`, `None`, `Small`, `Medium`, `Large` or `ExtraLarge`.

## Add it to an app

In an existing [Aioros](https://github.com/singularengine/aioros) app:

```bash
dotnet add package Aioros.Cosmo --version 0.1.0-preview.1
```

Two more packages are optional: `Aioros.Cosmo.Charts` for charts through [Aioros.Pictor](https://github.com/singularengine/aioros-pictor), and `Aioros.Cosmo.Markdown` for rendering Markdown.

Then add the theme to `App.axaml`:

```xml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:c="https://aioros.dev/cosmo">
  <Application.Styles>
    <FluentTheme />
    <c:CosmoTheme Color="Blue" Radius="Default" />
  </Application.Styles>
</Application>
```

## Example

A card on a page:

```xml
<UserControl xmlns="https://github.com/avaloniaui" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:c="https://aioros.dev/cosmo" xmlns:m="using:Shop.Models" x:DataType="m:DeployModel">
  <c:Card MaxWidth="400">
    <c:CardHeader Title="Create project" Description="Deploy your new project in one click." />
    <c:CardFooter>
      <StackPanel Orientation="Horizontal" Spacing="8">
        <Button Classes="outline" Content="Cancel" />
        <Button Command="{Binding DeployCommand}" Content="Deploy" />
      </StackPanel>
    </c:CardFooter>
  </c:Card>
</UserControl>
```

[Getting started](docs/getting-started.md) covers variants, forms, overlays and theming.

## What is included

Every component in shadcn/ui's registry, including:

- **Forms:** input, textarea, field, select, checkbox, radio group, switch, toggle, slider and OTP, with validation.
- **Overlays:** popover, tooltip, hover card, dropdown and context menus, dialog, sheet, drawer and toasts.
- **Display:** avatar, badge, alert, card, skeleton, typography and empty states.
- **Navigation:** tabs, accordion, breadcrumb, pagination, menubar, navigation menu and sidebar.
- **Data:** table, data table, command, combobox, calendar and date picker.
- **Charts and chat**, plus the blocks built from them, such as dashboards and sign-in pages.

The list, row by row, is in [Coverage](docs/coverage.md). Every public property binds, one way and two way, and MVVM needs no code-behind on either target.

## How close it is to shadcn/ui

Each component is compared with shadcn/ui's own render in Chromium, pixel by pixel and style by style. See [Fidelity](docs/fidelity.md).

## What it depends on

| Package | Depends on |
| --- | --- |
| `Aioros.Cosmo` | Avalonia and `Aioros.Markup` |
| `Aioros.Cosmo.Charts` | `Aioros.Cosmo` and `Aioros.Pictor` |
| `Aioros.Cosmo.Markdown` | `Aioros.Cosmo` and Markdig |

## Documentation

| Document | What it holds |
| --- | --- |
| [Getting started](docs/getting-started.md) | Using components, variants, forms and themes |
| [Tokens](docs/tokens.md) | Every design token and how it is converted |
| [Coverage](docs/coverage.md) | Every component and its status |
| [Fidelity](docs/fidelity.md) | How components are compared with shadcn/ui |
| [Benchmarks](docs/benchmarks.md) | How it performs against shadcn/ui on Next.js |
| [Roadmap](docs/roadmap.md) | What exists and what comes next |

## Building from source

```bash
dotnet build Aioros.Cosmo.slnx
dotnet test --solution Aioros.Cosmo.slnx
dotnet run --project samples/Gallery/Gallery              # the Gallery, at http://localhost:3031
dotnet run --project samples/Gallery/Gallery -- --desktop  # the same pages in a native window
```

The SDK is pinned in `global.json`. See [AGENTS.md](AGENTS.md) for how changes are made.

## License

MIT, as is the whole Aioros ecosystem: see [LICENSE](LICENSE).

Aioros.Cosmo is an independent port and is not affiliated with or endorsed by shadcn. shadcn/ui and Radix UI are MIT-licensed, Lucide is ISC-licensed and Inter is under the SIL Open Font License. Their notices are in [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) and ship with every package.
