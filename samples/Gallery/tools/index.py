"""Writes /components, the index of the Gallery's component pages (shadcn/ui's /docs/components): a link to each page,
by title, read from the pages pages.py writes."""
import re
from html import escape
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent / "Gallery" / "Pages" / "components"

PAGE = """<UserControl xmlns="https://github.com/avaloniaui" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:a="https://aioros.dev" xmlns:c="https://aioros.dev/cosmo"
             a:Page.Title="Components" a:Page.Description="{lead}">
  <StackPanel Spacing="32" MaxWidth="880" HorizontalAlignment="Left">
    <StackPanel Spacing="10">
      <TextBlock Classes="h1" Text="Components" />
      <TextBlock Classes="lead" Text="{lead}" />
    </StackPanel>
    <WrapPanel a:Web.Id="components" ItemSpacing="32" LineSpacing="16">
{links}
    </WrapPanel>
  </StackPanel>
</UserControl>
"""

LEAD = "Every component of Aioros.Cosmo, written in XAML for the web and the desktop: {count} of them."


def titles():
    pages = []
    for folder in sorted(path for path in ROOT.iterdir() if (path / "Page.axaml").exists()):
        title = re.search(r'a:Page\.Title="([^"]+)"', (folder / "Page.axaml").read_text()).group(1)
        pages.append((title, folder.name))
    return sorted(pages, key=lambda page: page[0].lower())


def write():
    pages = titles()
    links = "\n".join(
        f'      <HyperlinkButton Classes="link" Width="196" Padding="0" Height="NaN" MinHeight="0" FontSize="16" HorizontalContentAlignment="Left" '
        f'NavigateUri="/components/{route}" Content="{escape(title, quote=True)}" />'
        for title, route in pages)
    (ROOT / "Page.axaml").write_text(PAGE.format(lead=escape(LEAD.format(count=len(pages)), quote=True), links=links))


if __name__ == "__main__":
    write()
