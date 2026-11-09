# Template

`dotnet new cosmo` (the `Aioros.Cosmo.Templates` package): an Aioros app built with Cosmo, out of the box. Its shell is
shadcn/ui's `dashboard-01` split into a layout (the inset sidebar and the header) and pages (the dashboard's cards,
interactive area chart, data table and section drawer, and a settings page), its data from the dashboard's models, its
theme Cosmo's with the color and the radius chosen when it is created.

| AC | Criterion |
| --- | --- |
| TEMPLATE-01 | Given Cosmo's packages and the template package packed to a local feed beside Aioros's, when `dotnet new cosmo --color Blue --radius Large` creates an app from it in a private template hive (`--debug:custom-hive`) and the app is restored into a private package cache, built and run with `--precompile=<file>`, then the build reports 0 warnings and 0 errors, the run exits with 0 and reports every page precompiled (`Precompiled N of N pages`, N > 0), its `App.axaml` sets `c:CosmoTheme Color="Blue" Radius="Large"`, and nothing is written to the user's template hive or NuGet cache. Precedent: Aioros's own template test (Aioros CLI-06), after the ASP.NET Core templates' tests. |
