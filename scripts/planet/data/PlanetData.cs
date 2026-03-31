using System;
using System.Collections.Generic;
using Godot;
using Incremental.scripts.director;

namespace Incremental.scripts.planet.data;

public class PlanetData
{
    public readonly List<PlanetTile[]> Layers = new();
    public readonly List<PlanetChunk> _chunks = new();

    private Vector2I _lastMiningTarget;


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
        _lastMiningTarget = new Vector2I(0, layerCount - 1);
        

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

    public int GetLayerSize(int layer)
    {
        layer = Math.Clamp(layer, 0, Layers.Count - 1);
        return Layers[layer].Length;
    }

    public bool NextMiningTarget(int pawnID, out Vector2 target)
    {
        int startX = _lastMiningTarget.X;
        int startY = _lastMiningTarget.Y;

        for (int y = startY; y >= 0; y--)
        {
            PlanetTile[] layerData = Game.I._data.Layers[y];

            int xStart = (y == startY) ? startX : 0;

            for (int x = xStart; x < layerData.Length; x++)
            {
                PlanetTile tile = layerData[x];

                if (tile.OwnerID == -1 &&
                    !tile.Destroyed &&
                    tile.Material != TileMaterial.Unknown)
                {
                    tile.OwnerID = pawnID;

                    target = new Vector2(x, y);

                    int nextX = x + 1;
                    int nextY = y;

                    if (nextX >= layerData.Length)
                    {
                        nextX = 0;
                        nextY = y - 1;
                    }

                    _lastMiningTarget = new Vector2I(nextX, Mathf.Max(nextY, 0));
                    return true;
                }
            }
        }

        target = Vector2.Zero;
        return false;
    }
}
