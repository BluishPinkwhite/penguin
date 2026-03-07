using System;
using System.Collections.Generic;
using Godot;
using Incremental.scripts.director;

namespace Incremental.scripts.planet.data;

public class PlanetData
{
    public readonly List<PlanetTile[]> Layers = new();
    public readonly List<PlanetChunk> _chunks = new();


    public float _innerGrowth; // [0 ; TileSize]
    private const float GrowthSpeed = 20f;


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
        float r = localPos.Length() + _innerGrowth;
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
        float r = localPos.Length() + _innerGrowth;
        float angle = Mathf.Atan2(localPos.Y, localPos.X);
        angle = Mathf.PosMod(angle, Mathf.Tau);

        layer = Mathf.FloorToInt(r / TileSize);

        int tileCount = layer >= Layers.Count ? Layers[^1].Length : Layers[layer].Length;
        float step = Mathf.Tau / tileCount;

        tile = Mathf.FloorToInt(angle / step);
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
        int l = Mathf.RoundToInt(layer);
        float angleStep = Mathf.Tau / GetLayerSize(l);
        float angle = tile * angleStep;

        float radius = layer * Game.I._data.TileSize;

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
}
