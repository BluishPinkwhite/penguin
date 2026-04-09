using System;
using System.Collections.Generic;
using Incremental.scripts.director;
using Incremental.scripts.planet.rendering;

namespace Incremental.scripts.planet.data;

public static class PlanetDataLight
{
    const float lossPerStep = 1.0f / PlanetTile.LightReach;

    public static void PropagateLight(this PlanetData _data, int startLayer, int startTile, float initialLight)
    {
        Queue<(int layer, int tile)> queue = new();
        List<(int layer, int tile)> neighbors = new();

        PlanetTile root = _data.GetTileAtPolarCoords(startTile, startLayer);
        if (root == null) return;

        root.Light = initialLight;
        queue.Enqueue((startLayer, startTile));

        while (queue.Count > 0)
        {
            PlanetRenderer.isDirty = true;

            var (currL, currT) = queue.Dequeue();
            float currentLight = _data.GetTileAtPolarCoords(currT, currL).Light;
            float nextLight = currentLight - lossPerStep;

            // If the light is too dim to spread further, skip
            if (nextLight <= 0) continue;

            _data.GetNeighbors(currL, currT, neighbors);

            foreach (var (nL, nT) in neighbors)
            {
                if (nL < 0 || nL >= _data.Layers.Count)
                    continue;

                PlanetTile neighborTile = _data.GetTileAtPolarCoords(nT, nL);

                if (neighborTile != null && neighborTile.Light < nextLight)
                {
                    neighborTile.Light = nextLight;
                    queue.Enqueue((nL, nT));
                }
            }
        }
    }

    public static void RecalculateGlobalLight(this PlanetData _data)
    {
        Queue<(int l, int t)> queue = new();
        List<(int l, int t)> neighborsList = new();

        // reset light - solid and empty tiles
        for (int l = 0; l < _data.Layers.Count; l++)
        {
            for (int t = 0; t < _data.Layers[l].Length; t++)
            {
                PlanetTile tile = _data.Layers[l][t];

                if (tile.IsEmpty())
                {
                    tile.Light = PlanetTile.LightMax;
                    queue.Enqueue((l, t));
                }
                else
                {
                    tile.Light = 0;
                }
            }
        }

        // reset light - crust
        int topLayer = _data.Layers.Count - 1;
        for (int t = 0; t < _data.Layers[topLayer].Length; t++)
        {
            PlanetTile tile = _data.Layers[topLayer][t];
            tile.Light = 1f;

            queue.Enqueue((topLayer, t));
        }

        // spread light
        while (queue.Count > 0)
        {
            var (currL, currT) = queue.Dequeue();
            PlanetTile tile = _data.GetTileAtPolarCoords(currT, currL);
            if (tile == null)
                continue;

            float currentLight = tile.Light;
            float nextLight = currentLight - lossPerStep;

            if (nextLight <= 0) continue;
            
            _data.GetNeighbors(currL, currT, neighborsList);

            foreach (var (nL, nT) in neighborsList)
            {
                if (nL < 0 || nL >= _data.Layers.Count) continue;

                PlanetTile neighbor = _data.GetTileAtPolarCoords(nT, nL);

                // is dimmer -> propagate further
                if (neighbor != null && neighbor.Light < nextLight)
                {
                    neighbor.Light = nextLight;
                    queue.Enqueue((nL, nT));
                }
            }
        }

        PlanetRenderer.isDirty = true;
    }

    private static void GetNeighbors(this PlanetData _data, int l, int t, List<(int layer, int tile)> neighbors)
    {
        neighbors.Clear();

        // same layer
        neighbors.Add((l, t + 1));
        neighbors.Add((l, t - 1));

        // layer up
        if (l + 1 < _data.Layers.Count)
        {
            int currentSize = _data.Layers[l].Length;
            int nextSize = _data.Layers[l + 1].Length;

            if (nextSize > currentSize)
            {
                neighbors.Add((l + 1, t * 2));
                neighbors.Add((l + 1, t * 2 + 1));
            }
            else
                neighbors.Add((l + 1, t));
        }

        // layer down
        if (l - 1 >= 0)
        {
            int currentSize = _data.Layers[l].Length;
            int prevSize = _data.Layers[l - 1].Length;

            if (prevSize < currentSize)
                neighbors.Add((l - 1, t / 2));
            else
                neighbors.Add((l - 1, t));
        }
    }
}