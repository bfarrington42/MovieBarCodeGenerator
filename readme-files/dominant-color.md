# "Dominant color" generator

**Input source usage:** &cross; Audio &check; Video

Each bar is painted one flat color - the most common color found in its frame. Where the [Normal generator](normal.md) smears everything into an average, this mode posterizes.

## Pipeline overview

<pre>
source video file
  &rarr; extract N frames evenly spaced over the duration (ffmpeg, fps filter)
  &rarr; shrink each frame to a 64x64 working thumbnail (GDI resize)
  &rarr; histogram the thumbnail, take the winning bucket's mean color
  &rarr; paint one solid barWidth x barHeight bar
</pre>

## The histogram math

Every pixel votes into a 3D color histogram with 5 bits kept per channel:

$$r = R \gg 3,\quad g = G \gg 3,\quad b = B \gg 3$$

$$bucket = (r \cdot 32 + g) \cdot 32 + b$$

That is $32^3 = 32768$ buckets. The winning bucket is simply the one with the highest count, but the bar is *not* painted the bucket's flat color (which would posterize everything into 32-step bands). Instead the running sums kept alongside the counts yield the true mean of the pixels that voted there:

$$\text{bar} = \left(\frac{\sum R}{n},\; \frac{\sum G}{n},\; \frac{\sum B}{n}\right)$$

so the result stays representative of the actual frame colors.

The shared routine also supports two refinements this mode doesn't use itself:
- restricting the vote to a mask (one byte per pixel, used by [Subject color](subject-color.md))
- ignoring near-black pixels below a brightness gate (to keep letterbox residue and edge-blur out of the vote)

## Reading the output

- **Flat poster bands** - scenes collapse to their single most common color, and gradients and texture vanish
- **Dark bias** - black letterbox bars vote too, so widescreen movies skew darker than their content warrants. Cropping or Subject color counteracts this.
- **Flickery scenes** - when two colors run neck-and-neck in a run of frames, tiny changes flip the winner and adjacent bars can jump back and forth.
