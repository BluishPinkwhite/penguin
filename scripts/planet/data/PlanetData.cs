using System;
using System.Collections.Generic;
using Godot;
using Incremental.scripts.director;
using Incremental.scripts.planet.rendering;

namespace Incremental.scripts.planet.data;

public class PlanetData
{
    public readonly List<PlanetTile[]> Layers = new();
    public readonly List<PlanetChunk> _chunks = new();

    public float _innerGrowth; // [0 ; 1]
    public const float GrowthSpeed = 1;


    public readonly int BaseTileCount;
    public readonly float TileSize;
    public readonly float GrowthFactor;

    public PlanetData(
        int layerCount,
        float tileSize,
        int baseTileCount = 8,
        float growthFactor = 1.35f)
    {
        TileSize = tileSize;
        BaseTileCount = baseTileCount;
        GrowthFactor = growthFactor;

        _innerGrowth = TileSize * 0.3f;


        int subdivision = 1; // power-of-two multiplier

        for (int layer = 0; layer < layerCount; layer++)
        {
            float radius = (layer + 0.5f) * tileSize;
            float circumference = Mathf.Tau * radius;

            int desired = Mathf.RoundToInt(circumference / tileSize);
            int current = BaseTileCount * subdivision;

            if (desired >= current * GrowthFactor)
                subdivision *= 2; // always power-of-two

            int tileCount = BaseTileCount * subdivision;

            PlanetTile[] tiles = PlanetGenerator.GenerateLayerTiles(tileCount, layer, layerCount);
            Layers.Add(tiles);
        }
    }


    public bool LocalPositionToPolarCoords(Vector2 localPos, out int layer, out int tile)
    {
        float r = localPos.Length();
        float angle = Mathf.Atan2(localPos.Y, localPos.X);
        angle = Mathf.PosMod(angle, Mathf.Tau);

        layer = Mathf.FloorToInt(r / TileSize);
        if (layer < 0 || layer >= Layers.Count)
        {
            tile = -1;
            return false;
        }

        int tileCount = Layers[layer].Length;
        float step = Mathf.Tau / tileCount;

        tile = Mathf.FloorToInt(angle / step);
        return tile >= 0 && tile < tileCount;
    }

    public void LocalPositionToPolarCoordsUnbounded(Vector2 localPos, out int layer, out int tile)
    {
        float r = localPos.Length();
        float angle = Mathf.Atan2(localPos.Y, localPos.X);
        angle = Mathf.PosMod(angle, Mathf.Tau);

        layer = Mathf.FloorToInt(r / TileSize);

        int tileCount = layer >= Layers.Count ? Layers[^1].Length : Layers[layer].Length;
        float step = Mathf.Tau / tileCount;

        tile = Mathf.FloorToInt(angle / step);
    }

    public PlanetTile GetTileAtPolarCoords(float tile, float layer)
    {
        return GetTileAtPolarCoords(Mathf.FloorToInt(tile), Mathf.FloorToInt(layer));
    }

    public PlanetTile GetTileAtPolarCoords(int tile, int layer)
    {
        if (layer >= Layers.Count || layer < 0)
            return null;

        tile = Mathf.PosMod(tile, Layers[layer].Length);
        return Layers[layer][tile];
    }

    public Vector2 PolarToWorld(float tile, float layer)
    {
        int l = Mathf.FloorToInt(layer);
        float angleStep = Mathf.Tau / GetLayerSize(l);
        float angle = tile * angleStep;

        float radius = layer * Game.I._data.TileSize;

        return new Vector2(
            Mathf.Cos(angle),
            Mathf.Sin(angle)
        ) * radius;
    }

    public Vector2 PolarToWorld(Vector2 polar)
    {
        int l = Mathf.FloorToInt(polar.Y);
        float angleStep = Mathf.Tau / GetLayerSize(l);
        float angle = polar.X * angleStep;

        float radius = polar.Y * Game.I._data.TileSize;

        return new Vector2(
            Mathf.Cos(angle),
            Mathf.Sin(angle)
        ) * radius;
    }

    public int GetLayerSize(int layer)
    {
        layer = Math.Clamp(layer, 0, Layers.Count - 1);
        return Layers[layer].Length;
    }

    private int WrapX(int x, int size)
    {
        return (x % size + size) % size;
    }

    public bool NextMiningTarget(int pawnID, Vector2 fromPolar, out Vector2 target, out PlanetTile bestTile)
    {
        int centerX = Mathf.FloorToInt(fromPolar.X);
        int centerY = Mathf.FloorToInt(fromPolar.Y);

        int maxRadius = Game.I._data.Layers.Count;

        bestTile = null;
        int bestX = 0;
        int bestY = 0;

        for (int radius = 0; radius < maxRadius; radius++)
        {
            bool foundAny = false;
            int r2 = radius * radius;

            for (int dy = -radius; dy <= radius; dy++)
            {
                int y = centerY + dy;
                if (y < 0 || y >= Game.I._data.Layers.Count)
                    continue;

                PlanetTile[] layer = Game.I._data.Layers[y];
                int layerSize = layer.Length;

                for (int dx = -radius; dx <= radius; dx++)
                {
                    if (dx * dx + dy * dy * 10 > r2)
                        continue;

                    int x = WrapX(centerX + dx, layerSize);

                    PlanetTile tile = layer[x];

                    // tile is not available -> skip
                    if (tile.OwnerID != -1 ||
                        tile.IsEmpty())
                        continue;

                    // tile is reachable from above
                    PlanetTile above = Game.I._data.GetTileAtPolarCoords(x, y + 1);
                    if (above != null && !above.IsEmpty())
                        continue;

                    bestTile = tile;
                    bestX = x;
                    bestY = y;

                    foundAny = true;
                    break;
                }

                if (foundAny)
                    break;
            }

            if (foundAny)
            {
                target = new Vector2(bestX, bestY);
                return true;
            }
        }

        target = Vector2.Zero;
        bestTile = null;
        return false;
    }

    public void RegrowLayers()
    {
        // reset center layer
        for (int tile = 0; tile < Layers[0].Length; tile++)
        {
            PlanetTile tileData = Layers[0][tile];
            
            tileData.Integrity = 1f;
            tileData.Material = TileMaterial.Core;
        }
        
        // renew blocks
        float layerSpan = PlanetGenerator.LayerSpan(Layers.Count);
        for (int layer = Layers.Count - 1; layer > 0; layer--)
        {
            int belowLayer = layer - 1;
            int layerSize = Layers[layer].Length;
            bool splitLayer = Layers[layer].Length == Layers[belowLayer].Length;
            float layerPercent = (float)layer / Layers.Count;
            
            for (int tile = 0; tile < layerSize; tile++)
            {
                if (Game.RandomTo(Layers.Count + 5) < layer)
                    continue;
                
                PlanetTile tileData = Layers[layer][tile];
                PlanetTile below = splitLayer 
                    ? Layers[belowLayer][tile]
                    : Layers[belowLayer][tile / 2];

                if (tileData.IsEmpty() && !below.IsEmpty())
                {
                    tileData.Renew(PlanetGenerator.LayerMaterial(layerPercent, layerSpan));
                    tileData.Integrity = below.Integrity;
                    tileData.Regrowing = true;
                    below.Integrity = 1f;
                }
            }
        }

        PlanetRenderer.isLightDirty = true;
    }
}