# Introduction

We can edit the SVG of each icon in Figma, Inkscape, Adobe Illustrator or whatever tool we prefer.

We use FontForge to generate the font. Here is a small tutorial:

# Add a glyph to the font

1. In FontForge, go to `Encoding > Add Encoding Slots`.
2. Select the slot you just created and go to `File > Import` and select the SVG you want to import.
3. Then, go to `Element > Glyph Info` and set a glyph name and Unicode value like `U+0100`.

# Generate a TrueType font

1. First, we need to validate the change we made. Go to `Element > Validation > Validate...`
2. Typical error we encountered in the past is `Self Intersecting`. You can resolve it automatically by using `Element > Overlap > Remove Overlap`. If it made the glyph ugly, you will have to fix the issue manually by editing the SVG.
3. Another typical error we encountered is `Non-Integral Coordinates`. To fix it automatically, use `Element > Round > To Int`.
4. Once there are no validation issues anymore, go to File > Generate Fonts. Select `TrueType` and check `Validate before saving`. Then click `Generate`.
5. A TTF file should be generated. You can verify that it works well by trying to open it in the UWP app `Character Map`.