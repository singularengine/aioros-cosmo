# Getting started

How to use Cosmo's components once the package and the theme are in your app. If you have not done that yet, see the [README](../README.md).

## Components are XAML

A component is either an Avalonia control with Cosmo's theme, such as `Button` or `TextBox`, or a small control from the `https://aioros.dev/cosmo` namespace, usually prefixed `c:`. Everything else works as in any XAML app: bindings, commands, styles and templates.

```xml
<Button Content="Save" Command="{Binding SaveCommand}" />
<c:Badge Classes="secondary" Content="New" />
```

## Variants

Variants and sizes are classes, as they are in shadcn/ui:

```xml
<Button Content="Default" />
<Button Classes="secondary" Content="Secondary" />
<Button Classes="outline" Content="Outline" />
<Button Classes="ghost" Content="Ghost" />
<Button Classes="destructive" Content="Delete" />
<Button Classes="outline sm" Content="Small" />
```

A class can be bound, as any Avalonia class can: `Classes.destructive="{Binding IsDangerous}"`.

## Links

A `HyperlinkButton` has the look of a `Button` with the same classes, and on the web it is a real `<a href>`:

```xml
<HyperlinkButton Classes="outline" NavigateUri="/settings" Content="Settings" />
```

## Cards and layout

```xml
<c:Card MaxWidth="400">
  <c:CardHeader Title="Create project" Description="Deploy your new project in one click." />
  <c:CardFooter>
    <Button Content="Deploy" Command="{Binding DeployCommand}" />
  </c:CardFooter>
</c:Card>
```

## Forms

Inputs are Avalonia's, so two-way bindings and `INotifyDataErrorInfo` validation work as usual. On the web, validation messages appear in the field that reported them.

```xml
<TextBox Text="{Binding Name}" PlaceholderText="Your name" />
<CheckBox IsChecked="{Binding Accepted}" Content="I accept the terms" />
<Slider Minimum="0" Maximum="100" Value="{Binding Volume}" />
```

On the web, a text box posts its value with the next button press. See the [Aioros concepts](https://github.com/singularengine/aioros/blob/main/docs/concepts.md#commands-and-fields) for how posts work.

## Themes

`c:CosmoTheme` takes a color and a radius:

```xml
<c:CosmoTheme Color="Violet" Radius="Large" />
```

Every design token is a resource, such as `CosmoPrimaryBrush` or `CosmoRadiusMd`. Override one in your app's resources, per light and dark theme, the way you would edit shadcn's `globals.css`. [Tokens](tokens.md) lists them all.

A link to `/theme/toggle` switches between light and dark.

## Charts

Add `Aioros.Cosmo.Charts` and its theme:

```xml
<c:CosmoChartsTheme />
```

Charts are [Aioros.Pictor](https://github.com/singularengine/aioros-pictor) controls, and under this theme they look like shadcn/ui's.

## Translating fixed texts

The file input's fixed texts can be set with `c:FileInput.ChooseText` and `c:FileInput.EmptyText`.

## See every component

The Gallery has a page per component with its code, and a theme customizer:

```bash
dotnet run --project samples/Gallery/Gallery
```

Then open <http://localhost:3031>.
