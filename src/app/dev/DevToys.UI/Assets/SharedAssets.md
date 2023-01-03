See the documentation about assets here: https://platform.uno/docs/articles/features/working-with-assets.html

# Here is a cheat sheet:

1. Add the image file to the `Assets` directory of a shared project.
2. Set the build action to `Content`.
3. (Recommended) Provide an asset for various scales/dpi

## Examples

```
\Assets\Images\logo.scale-100.png
\Assets\Images\logo.scale-200.png
\Assets\Images\logo.scale-400.png

\Assets\Images\scale-100\logo.png
\Assets\Images\scale-200\logo.png
\Assets\Images\scale-400\logo.png
```

## Table of scales

| Scale | UWP         | iOS      | Android |
|-------|:-----------:|:--------:|:-------:|
| `100` | scale-100   | @1x      | mdpi    |
| `125` | scale-125   | N/A      | N/A     |
| `150` | scale-150   | N/A      | hdpi    |
| `200` | scale-200   | @2x      | xhdpi   |
| `300` | scale-300   | @3x      | xxhdpi  |
| `400` | scale-400   | N/A      | xxxhdpi |




