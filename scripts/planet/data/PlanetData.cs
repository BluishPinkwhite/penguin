using System;
using System.Collections.Generic;
using Godot;
using Incremental.scripts.director;
using Incremental.scripts.entity.station;
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
    
    public MiningShape CurrentMiningShape = MiningShape.TallerThanWider;
    
    private List<(int x, int y, float dist)> _miningQueue = new();
    private int _miningQueueIndex = 0;
    

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

    public void FillMiningQueue()
    {
        _miningQueue.Clear();

        Vector2 stationPolar = ResourceStation.I.PolarPos;
        float rs = stationPolar.Y * TileSize;
        float thetaS = stationPolar.X * Mathf.Tau / GetLayerSize(Mathf.FloorToInt(stationPolar.Y));

        float k = CurrentMiningShape switch
        {
            MiningShape.Original => 0.4f,
            MiningShape.Taller => 0.9f,
            MiningShape.TallerThanWider => 1.3f,
            _ => 1.0f
        };
        float kSqInv = 1.0f / (k * k);

        for (int y = 0; y < Layers.Count; y++)
        {
            int tileCount = Layers[y].Length;
            float rt = y * TileSize;
            float angleStep = Mathf.Tau / tileCount;

            for (int x = 0; x < tileCount; x++)
            {
                float thetaT = x * angleStep;
                float dTheta = thetaT - thetaS;

                float dr = rt - rs;
                float distSq = dr * dr * kSqInv + 2.0f * rs * rt * (1.0f - Mathf.Cos(dTheta));

                _miningQueue.Add((x, y, distSq));
            }
        }

        _miningQueue.Sort((a, b) => a.dist.CompareTo(b.dist));
        _miningQueueIndex = 0;
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

        float radius = layer * TileSize;

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

        float radius = polar.Y * TileSize;

        return new Vector2(
            Mathf.Cos(angle),
            Mathf.Sin(angle)
        ) * radius;
    }

    public float PolarDistanceSquared(Vector2 a, Vector2 b)
    {
        return PolarToWorld(a).DistanceSquaredTo(PolarToWorld(b));
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

    public bool NextMiningTarget(Vector2 fromPos, out Vector2 target, out PlanetTile bestTile)
    {
        if (_miningQueue.Count == 0)
            FillMiningQueue();
        
        bestTile = null;
        (int x, int y, float dist) best = (0,0,0);

        // TODO fix for multiple mining shapes
        int i = 0;//_miningQueueIndex;
        int a = 0;
        while (a < 10)
        {
            if (i >= _miningQueue.Count)
                break;

            (int x, int y, float dist) next = _miningQueue[i];
            i++;
            
            PlanetTile tile = Layers[next.y][next.x];
            
            // tile is not available -> skip
            if (tile.OwnerID != -1 || tile.IsEmpty())
                continue;
            
            // tile is reachable from above
            PlanetTile above = Game.I._data.GetTileAtPolarCoords(next.x, next.y + 1);
            if (above != null && !above.IsEmpty())
                continue;
            
            float dist = PolarDistanceSquared(new Vector2(next.x, next.y), fromPos);
            float minDist = PolarDistanceSquared(new Vector2(best.x, best.y), fromPos);

            if (bestTile == null || dist + next.dist < minDist + best.dist)
            {
                best = next;
                bestTile = tile;
            }
            a++;
        }
        
        if (bestTile != null)
        {
            target = new Vector2(best.x, best.y);
            return true;
        }

        target = Vector2.Zero;
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
                if (Mathf.Pow(Game.RandomTo(1f), 2) <= (float)layer / Layers.Count)
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
                    below.OwnerID = -1;
                }
            }
        }

        PlanetRenderer.isLightDirty = true;
        
        _miningQueueIndex = 0;
        while (_miningQueueIndex < _miningQueue.Count)
        {
            (int x, int y, float dist) next = _miningQueue[_miningQueueIndex];
            PlanetTile tile = Layers[next.y][next.x];
            
            // tile is not available -> skip
            if (tile.OwnerID != -1 || tile.IsEmpty())
                _miningQueueIndex++;
            else break;
        }
    }
}