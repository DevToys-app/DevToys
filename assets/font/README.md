# Introduction

We can edit the SVG of each icon in Figma, Inkscape, Adobe Illustrator or whatever tool we prefer.

We use FontForge to generate the font. Here is a small tutorial:

# Add a glyph to the font

1. Install [FontForge](https://fontforge.org/en-US/) and open [DevToys-Tools-Icons.sfd](/assets/font/DevToys-Tools-Icons.sfd).
2. In Figma, Inkscape, Adobe Illustrator or else, create a glyph and export it as a SVG.
3. In FontForge, go to `Encoding > Add Encoding Slots`.
4. Select the slot you just created and go to `File > Import` and select the SVG you want to import. Under `Options`, uncheck `Scale to fit (Misc)`.
5. Then, go to `Element > Glyph Info` and set a glyph name and Unicode value like `U+0100`.

# Generate a TrueType font

1. First, we need to validate the change we made. Go to `Element > Validation > Validate...`
   * Typical error we encountered in the past is `Self Intersecting`. You can resolve it automatically by using `Element > Overlap > Remove Overlap`. If it made the glyph ugly, you will have to fix the issue manually by editing the SVG.
   * Another typical error we encountered is `Non-Integral Coordinates`. To fix it automatically, use `Element > Round > To Int`.
   * Another error is `Missing Points at Extrema`. To fix it, use `Element > Add Extrema`.
2. Once there are no validation issues anymore, go to `File > Generate Fonts`. Select `TrueType` and check `Validate before saving`. Then click `Generate`.
3. A TTF file should be generated. You can verify that it works well by trying to open it in the app [Character Map UWP](https://apps.microsoft.com/store/detail/character-map-uwp/9WZDNCRDXF41?hl=en-us&gl=us).
