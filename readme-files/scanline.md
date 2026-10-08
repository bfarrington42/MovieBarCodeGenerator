# "Scanline" generator

**Input source usage:** &cross; Audio &check; Video

Instead of averaging the frame, each bar is a single horizontal row of pixels sampled from its frame and stretched into a bar. Sharper than the average-based modes and immune to letterbox bars, but a bit noisier.

## Pipeline overview

<pre>
source video file
  &rarr; extract N frames evenly spaced over the duration (ffmpeg, fps filter)
  &rarr; copy the middle row of each frame, stretch to barWidth x barHeight
  &rarr; concatenate the bars left to right
</pre>

## The sampling

There's not much going on here. The center row ($row = 0.5 \times sourceHeight$) is extracted and scaled into a single bar.

Because only one row in 720 (or 1080, or 2160) survives per frame, there is no averaging to hide noise, compression artifacts, or film grain. Whatever the middle row caught, good or bad, becomes the bar. Nothing outside that row can pollute it, however. Black letterbox bars at the top and bottom never enter the computation at all.

## Reading the output

- **Crisp vertical texture** - costume patterns, horizons crossing the middle of the frame, and title cards read far more literally than in [Normal](normal.md).
- **Speckle and flicker** - grain and compression noise show up as-is, so a noisy source makes a noisy barcode
