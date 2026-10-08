# "Vertical sweep" generator

**Input source usage:** &cross; Audio &check; Video

Each bar is a single vertical column of its frame stretched into a bar, but a *different* column every frame. The sampling position sweeps left to right across the frame width as the movie progresses, wrapping back to the
left edge when it reaches the right edge. Like watching on super fast forward.

## Pipeline overview

<pre>
source video file
  &rarr; extract N frames evenly spaced over the duration (ffmpeg, fps filter)
  &rarr; copy one column of frame i, stretch to barWidth x barHeight
  &rarr; concatenate the bars left to right
</pre>

## The sweep

This is the one frame-aware generator so far. Instead of `GetBar(frame)`, the pipeline calls `GetBar(frame, frameIndex, frameCount)`, and the sampled column is a function of position in the sequence:

$$\text{column}(i) = i \bmod W (\text{where}\:\text{W} = frame width)$

Negative indices would wrap right to left instead. Frame 0 takes the leftmost column, frame 1 takes the next column, and so on. After $W$ frames, the sweep wraps and starts over. With the
default 1000 bars and ~1280-1920px frames, a single left-to-right pass will not cover the full width of a frame, so you would want to adjust the number of bars to be $\ge$ the width of the video.

The $1 \times H$ strip at that column is drawn into a $\text{barWidth} \times \text{barHeight}$ rectangle with high-quality bicubic interpolation. The full column height is resampled down the bar, so vertical detail survives.

## Reading the output

- **Diagonal motion streaks** - camera pans and moving subjects smear across neighboring bars, since adjacent bars sample adjacent columns of adjacent frames
- **Spatial meets temporal** - the horizontal axis is *both* time and a slow pan across each frame, which is why static scenes still drift
- **Wrap discontinuities** - wherever the sweep restarts at the left edge, expect a hard seam unrelated to any cut
