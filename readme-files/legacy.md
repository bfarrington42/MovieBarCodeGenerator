# "Legacy" generator

**Input source usage:** &cross; Audio &check; Video

The mode used by older versions of the program, kept for backward-compatibility. It does the same job as [Normal](normal.md) - one whole frame per bar - but scales with GDI+ instead of MagicScaler, which produces visibly poorer averages.

## Pipeline overview

<pre>
source video file
  &rarr; extract N frames evenly spaced over the duration (ffmpeg, fps filter)
  &rarr; rescale each frame to barWidth x barHeight (GDI+ DrawImage)
  &rarr; concatenate the bars left to right
</pre>

Frame sampling is identical to Normal ($fps = \text{barCount} / \text{duration}$). Only the resampler differs here.

## Why it looks worse

GDI+ `DrawImage` scaling quality hinges on a handful of graphics settings, and this generator deliberately uses the old defaults rather than the corrected ones:

- **Interpolation** - `InterpolationMode.Default` instead of a high-quality bicubic kernel, so each destination pixel blends fewer, and poorly weighted source pixels
- **Edge handling** - no `TileFlipXY` wrap mode, so sampling near the image borders pulls in duplicated pixels from the edge and darkens the bars
- **Pixel offset** - default instead of high-quality placement, shifting sample positions by up to half a pixel

The class also supports vertically smoothed variants (averaging passes that collapse each bar to its solid mean color), but those are no longer hard-wired in. The global "Smoothed" option now handles this.

## Reading the output

Same smeared-average look as Normal, but muddier. Flat fields band and posterize where Normal stays clean, and high-frequency detail (grain and fine texture) aliases into shimmer that Normal's averaging kernel absorbs. Comparing the two outputs of the same film is the quickest way to see what resampling quality will get you.
