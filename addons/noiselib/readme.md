# ShaderNoiseLib
A library for noise functions in visual shaders.

> Note:  
> The noise functions are not going to produce the exact same results as godot's noise functions.
> I simply wanted gpu noise functions which would produce the same types of noise for my shader graphs, while the outputs can be used the same way, they will not 100% match pixel-by-pixel.  
>
> If you want to recreate "domain warp", you can add `PerlinNoise3D` to the UV, for both the x and y coordinates.

# Types of noise implemented
* \[PerlinNoise3D] Perlin noise, taken from [the godot documentation](https://docs.godotengine.org/en/stable/tutorials/plugins/editor/visual_shader_plugins.html). Since it is not included by default, but does follow the goals of this project. I have included it. I did not write this shader, it has been modified to use the `z`-coordinate of `uv` (now `uvw`) instead of `time`.
  * Inputs
    * `uvw (vec3)` - XYZ texture coordinates
    * `offset (vec3)` - Same as UVW, added
    * `scale (vec3)` - The scale of the noise, higher is smaller
  * Outputs (between 0 and 1)
    * `result (float)` - The resulting noise value at `uvw + offset`
* \[VoronoiNoise3D] Voronoi/Worley noise (aka cellular noise), written from scratch, similar outputs to godot's `FastNoiseLite` `Cellular` mode.
  * Inputs (usage similar to `Cellular` mode)
    * `uvw (vec3)` - XYZ texture coordinates
    * `offset (vec3)` - Same as UVW, added
    * `scale (vec3)` - The scale of the noise, higher is smaller
    * `distance scale (float)` - A float which scales the distance, this has a similar effect as multiplying the output distance and distance2 values.
    * `distance function (int [enum])` - An enum indicating which distance function to use, note: currently no dropdown to select the mode. I'm not sure if this is possible and can't look it up since I don't have internet right now.
      * `0` - Euclidean
      * `1` - Euclidean squared
      * `2` - Manhattan
      * `3` - Hybrid (average of `0` and `2`)
    * `jitter (float)` - The randomness of the noise, recommended between 0 and 1, negative values are supported
    * `3d (bool)` - Whether the noise is 3D, `w` from `uvw` has no effect when `3d` is `false`.
  * Outputs (between 0 and 1, clamped)
    * `cell value (float)` - A random value for the cell the `uvw + offset` is closest to. The values of the points are consistent, just like with godot's noise
    * `distance (float)` - The distance to the closest cell, creates a pattern with black dots
    * `distance2 (float)` - The distance to the second-closest cell, creates a pattern with black "scratches" or "stars"
* \[PixelNoise3D] Random value per "pixel", pixels are scaled
  * Inputs
    * `uvw (vec3)` - XYZ texture coordinates
    * `offset (vec3)` - Same as UVW, added
    * `scale (vec3)` - The scale of the noise, (1., 1., 1.) would mean one value per whole number. (10., 10., 10.) would mean 10 values per whole number on each axis.
  * Outputs (between 0 and 1)
    * `noise (float)` - The resulting noise value at `uvw + offset`

Note:  
VoronoiNoise3D with `jitter` at 0, using the `cell value` output looks like pixel noise, however, it is less efficient to calculate, if you need tv-static like noise, use `PixelNoise3D`.