# "Subject color" generator

**Input source usage:** &cross; Audio &check; Video

[Dominant color](dominant-color.md), but isolated to the main subject. Each bar is painted the most common color of the *largest object* in its frame rather than the whole frame. Letterbox bars, skies, and walls are precluded. Actors, cars, spaceships, etc. are what we end up pulling from.

## Pipeline overview

<pre>
source video file
  &rarr; extract N frames evenly spaced over the duration (ffmpeg, fps filter)
  &rarr; crop ~1/8 off top and bottom (letterbox guard)
  &rarr; shrink to a 64x64 working thumbnail (GDI resize)
  &rarr; color-aware Sobel edge detection (threshold 50)
  &rarr; morphological close (dilate, then erode)
  &rarr; flood-fill the background from the borders, invert to foreground
  &rarr; largest 8-connected blob = subject mask (needs >= 2% of pixels)
  &rarr; dominant color of the masked pixels, ignoring near-black (gate 16)
  &rarr; paint one solid barWidth x barHeight bar
</pre>

If no significant blob survives, it falls back to the dominant color of the whole frame, so this mode can never do worse than Dominant color.

## The detection math

**Edges.** A [Sobel gradient](https://en.wikipedia.org/wiki/Sobel_operator) is computed per color channel with the classic kernels:

```
Gx = [-1  0  1]      Gy = [-1 -2 -1]
     [-2  0  2]           [ 0  0  0]
     [-1  0  1]           [ 1  2  1]
```

and each pixel keeps its strongest channel, which is what makes the detector color-aware. Isoluminant boundaries (like red on black) still register, where a grayscale conversion would lose edges:

$$\text{edge} = \max_c \left(|G_x(c)| + |G_y(c)|\right) \ge 50$$

The outermost pixel ring is skipped since our 3x3 won't fit there. Then the map is closed by dilating[^1] and eroding[^1] with a 3x3 structuring element to join broken outlines and drop speckle:

$$\text{dilate: pixel} = 1 \text{ if ANY neighbor is 1}$$

$$\text{erode: pixel} = 1 \text{ if ALL neighbors are 1}$$

**Foreground.** Background is whatever non-edge pixels connect to the image border (4-connected flood fill, so diagonal edge gaps don't leak out). Everything else is considered foreground.

**Subject.** Foreground pixels are grouped by 8-connected component labeling (breadth-first search over all 8 neighbors) and the largest blob wins so long as it covers at least 2% of the thumbnail. This way small objects never count as a subject just for being the only one in a frame. If the subject touches the frame edge (breaking the flood fill), a fallback pass takes the largest blob of the twice-dilated outlines instead.

**Color.** The winning mask feeds the same bucketed-histogram vote as [Dominant color](dominant-color.md), except pixels with all channels at or below 16 are skipped to prevent letterbox residue and soft edge fringe from being used as the subject.

## Reading the output

- **Subject-following bands** - bars track costumes, faces, and vehicles instead of skies, walls, and other background scenery. Dialogue scenes can strobe between skin tones.
- **Fallback frames** - empty or confetti shots with no clear blob silently degrade to whole-frame dominant color

This is by far the slowest mode (edges, morphology, and labeling per frame on top of everything else), and fast motion or busy backgrounds can elect surprising "subjects."

[^1]: [Morphological Operations](https://www.mathworks.com/help/images/morphological-dilation-and-erosion.html)