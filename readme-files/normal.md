# "Normal" generator

**Input source usage:** &cross; Audio &check; Video

Each bar is a whole video frame scaled down to a single bar width, so the barcode reads as a smeared, time-ordered average of the movie.

### Pipeline

<pre>
source video file
  &rarr; extract N frames evenly spaced over the duration (ffmpeg, fps filter)
  &rarr; rescale each frame to barWidth x barHeight (PhotoSauce MagicScaler)
  &rarr; concatenate the bars left to right
</pre>

### Frame sampling

With $barCount = round(width / barWidth)$ bars to fill, frames are pulled at

$$fps = \frac{\text{barCount}}{\text{duration}}$$

so bar $i$ shows the frame nearest timestamp $i / fps$. Every bar therefore represents an equal slice of runtime.

### Rescaling

Each frame is rescaled to exactly one bar ($barWidth$ pixels wide, $barHeight$ tall) with MagicScaler's `Average` interpolation in `Stretch` mode, with no sharpening, and ignoring orientation metadata. The averaging kernel weights every source pixel's contribution correctly (including gamma handling), which is what keeps flat color fields flat instead of posterizing them.

## Reading the output

- **Broad color washes** - The average palette of each scene over time, with cuts and transitions reading as hard edges between washes.
- **Dark bands at the tail** - End credits most likely, unless excluded or the end credit detection failed to find them.
- **Dark bands at top & bottom** - Letterbox bars, which also drag the average slightly toward black.
- **Color variations within bars** - A whole frame collapses into a few pixels, so this mode shows palette and pacing, not content. However some remnants of the frame remain, resulting in variegated bars.

HDR sources are tone-mapped to SDR (Hable curve) before any bars are built, so HDR and SDR releases of the same film produce comparable barcodes at the cost of some highlight compression.
