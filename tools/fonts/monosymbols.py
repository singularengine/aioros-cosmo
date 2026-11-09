"""Builds Cosmo Mono Symbols: the check mark Geist Mono lacks, subset from JetBrains Mono (SIL OFL 1.1) and renamed:
monosymbols.py <out folder>."""
import sys
from pathlib import Path

from fontTools import subset
from fontTools.ttLib import TTFont

SOURCE = "/usr/share/fonts/TTF/JetBrainsMonoNerdFont-Regular.ttf"
EMOJI = "✓"
out = Path(sys.argv[1])
out.mkdir(parents=True, exist_ok=True)
font = TTFont(SOURCE)
options = subset.Options()
options.layout_features = ["*"]
options.name_IDs = ["*"]
options.notdef_outline = True
subsetter = subset.Subsetter(options)
subsetter.populate(unicodes=[ord(c) for c in EMOJI])
subsetter.subset(font)
names = font["name"]
for record in list(names.names):
    if record.nameID in (1, 2, 3, 4, 6, 16, 17, 21, 22, 25):
        names.removeNames(nameID=record.nameID)
for platform, encoding, language in ((3, 1, 0x409), (1, 0, 0)):
    names.setName("Cosmo Mono Symbols", 1, platform, encoding, language)
    names.setName("Regular", 2, platform, encoding, language)
    names.setName("Cosmo Mono Symbols; derived from JetBrains Mono", 3, platform, encoding, language)
    names.setName("Cosmo Mono Symbols", 4, platform, encoding, language)
    names.setName("CosmoMonoSymbols-Regular", 6, platform, encoding, language)
target = out / "CosmoMonoSymbols-Regular.ttf"
font.save(target)
print(target, target.stat().st_size, sorted(font.keys()))
