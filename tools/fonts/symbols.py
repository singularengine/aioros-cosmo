"""Builds Cosmo Symbols: the keyboard symbols Geist lacks, from Adwaita Sans (SIL OFL 1.1, after Inter), instanced at its
default optical size and subset, renamed as the OFL asks of a modified version: symbols.py src/Aioros.Cosmo/Assets/Fonts.
Needs fontTools and Adwaita Sans 50.0 (GNOME's adwaita-fonts) installed."""
import sys
from pathlib import Path

from fontTools import subset
from fontTools.ttLib import TTFont
from fontTools.varLib import instancer

SOURCE = "/usr/share/fonts/Adwaita/AdwaitaSans-Regular.ttf"
SYMBOLS = "⌘⌥⌃⌫⌦⎋⇪"
out = Path(sys.argv[1])
out.mkdir(parents=True, exist_ok=True)
for weight, style in ((400, "Regular"), (500, "Medium")):
    font = instancer.instantiateVariableFont(TTFont(SOURCE), {"wght": weight, "opsz": 14})
    options = subset.Options()
    options.layout_features = []
    options.name_IDs = ["*"]
    options.notdef_outline = True
    subsetter = subset.Subsetter(options)
    subsetter.populate(unicodes=[ord(c) for c in SYMBOLS])
    subsetter.subset(font)
    names = font["name"]
    family = "Cosmo Symbols"
    for record in list(names.names):
        if record.nameID in (1, 2, 3, 4, 6, 16, 17, 21, 22, 25):
            names.removeNames(nameID=record.nameID)
    full = f"{family} {style}" if style != "Regular" else family
    for platform, encoding, language in ((3, 1, 0x409), (1, 0, 0)):
        names.setName(family if style == "Regular" else full, 1, platform, encoding, language)
        names.setName("Regular", 2, platform, encoding, language)
        names.setName(f"{family}-{style}; derived from Adwaita Sans 50.0", 3, platform, encoding, language)
        names.setName(full, 4, platform, encoding, language)
        names.setName(f"CosmoSymbols-{style}", 6, platform, encoding, language)
        names.setName(family, 16, platform, encoding, language)
        names.setName(style, 17, platform, encoding, language)
    font["OS/2"].usWeightClass = weight
    font.save(out / f"CosmoSymbols-{style}.ttf")
    print(out / f"CosmoSymbols-{style}.ttf", (out / f"CosmoSymbols-{style}.ttf").stat().st_size)
