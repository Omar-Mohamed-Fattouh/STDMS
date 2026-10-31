"""Subset fa-solid-900 to only the icons used in the Razor views.

Icon name -> codepoint mapping is parsed from the FontAwesome CSS itself.
Run from the project root:  python lighthouse/subset_font.py
"""
import os
import re
import subprocess
import pathlib

css = pathlib.Path("wwwroot/lib/fontawesome/css/all.min.css").read_text(encoding="utf-8")
# Handle grouped selectors too, e.g. ".fa-x:before,.fa-y:before{content:\"\\f015\"}"
mapping = {}
for selectors, codepoint in re.findall(r'([^{}]+)\{content:"\\([0-9a-f]+)"\}', css):
    for name in re.findall(r'\.fa-([a-z0-9-]+)::?before', selectors):
        mapping[name] = codepoint

used = [
    "arrow-right", "arrow-up-right-from-square", "bars", "book", "book-open",
    "building", "calendar-check", "calendar-days", "chalkboard-teacher",
    "chalkboard-user", "chart-pie", "check", "check-circle", "chevron-right",
    "circle-check", "circle-user", "clipboard-list", "door-open", "envelope",
    "exclamation-circle", "graduation-cap", "house", "info-circle", "lock",
    "plus", "right-from-bracket", "search", "user-check", "user-graduate",
    "user-plus", "users-gear", "user-xmark", "xmark",
    "eye", "eye-slash", "triangle-exclamation", "arrow-left",
]

missing = [u for u in used if u not in mapping]
print("missing from FA css:", missing)
unicodes = ",".join("U+" + mapping[u].upper() for u in used if u in mapping)
print("subsetting to:", unicodes)

assert not missing, f"cannot subset, icons not found: {missing}"
out = "wwwroot/lib/fontawesome/webfonts/fa-solid-900.woff2"
subprocess.run([
    "pyftsubset", "wwwroot/lib/fontawesome/webfonts/fa-solid-900.ttf",
    f"--unicodes={unicodes}",
    f"--output-file={out}",
    "--flavor=woff2", "--desubroutinize",
], check=True)
print("new woff2 bytes:", os.path.getsize(out))

# verify every required codepoint survived subsetting
from fontTools.ttLib import TTFont
font = TTFont(out)
cmap = font.getBestCmap()
required = {int(mapping[u], 16) for u in used}
print("glyphs in subset:", len(cmap), "| missing codepoints:", required - set(cmap.keys()))
assert required <= set(cmap.keys())
