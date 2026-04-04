using Godot;
using Incremental.scripts.director;
using Incremental.scripts.planet.data;

namespace Incremental.scripts.entity;

public partial class SurfaceEntity : OrbitEntity
{
    public int _currSize;

    public const float Gravity = 10f;


    public override void _PhysicsProcess(double delta)
    {
        DoLayerChecks();
        ApplyPolarTransform();
    }


    public float CircularDelta(float from, float to, float size)
    {
        float diff = Mathf.PosMod(to - from, size);

        if (diff > size / 2f)
            diff -= size;

        return diff;
    }

    public float ScaleTileBetweenLayers(float tile, int fromLayer, int toLayer)
    {
        int fromSize = Game.I._data.GetLayerSize(fromLayer);
        int toSize = Game.I._data.GetLayerSize(toLayer);

        return tile * toSize / (float)fromSize;
    }

    public bool IsAir(int tile, int layer)
    {
        if (layer < 0)
            return false;

        int size = Game.I._data.GetLayerSize(layer);
        tile = Mathf.PosMod(tile, size);

        PlanetTile t = Game.I._data.GetTileAtPolarCoords(tile, layer);

        return t == null || t.Destroyed || t.Material == TileMaterial.Unknown;
    }

    public bool IsSolid(int tile, int layer)
    {
        PlanetTile t = Game.I._data.GetTileAtPolarCoords(tile, layer);

        return t != null &&
               !t.Destroyed &&
               t.Material != TileMaterial.Unknown;
    }


    public void ApplyGravity(float d)
    {
        float gravityY = PolarPos.Y - d * Gravity;
        
        int prevSize = Game.I._data.GetLayerSize(Mathf.FloorToInt(PolarPos.Y));
        int currSize = Game.I._data.GetLayerSize(Mathf.FloorToInt(gravityY));
        
        float newX = PolarPos.X * currSize / prevSize;
        
        if (!CheckCollision(newX, gravityY))
        {
            PolarPos.Y = gravityY;
        }
    }
    
    protected void DoLayerChecks()
    {
        int prevLayer = Mathf.FloorToInt(PrevPolarPos.Y);
        int layer = Mathf.FloorToInt(PolarPos.Y);
        
        int prevSize = Game.I._data.GetLayerSize(prevLayer);
        _currSize = Game.I._data.GetLayerSize(layer);

        if (layer != prevLayer)
            PolarPos.X = PolarPos.X * _currSize / prevSize; 
        
        PolarPos.X = Mathf.PosMod(PolarPos.X, _currSize);

        if (PolarPos.Y < 0f)
            PolarPos.Y = 0f;
    }

    public PlanetTile GetTileBelow(float gravityY)
    {
        return Game.I._data.GetTileAtPolarCoords(Mathf.FloorToInt(PolarPos.X), Mathf.FloorToInt(gravityY));
    }
    
    protected float GetHalfWidthTiles(int layer)
    {
        int size = Game.I._data.GetLayerSize(layer);
        return 0.2f / size;
    }

    public bool CheckCollision(float x, float y)
    {
        int layerBottom = Mathf.FloorToInt(y);
        float halfW = GetHalfWidthTiles(layerBottom);
        int sizeBottom = Game.I._data.GetLayerSize(layerBottom);

        int leftBottom = Mathf.FloorToInt(Mathf.PosMod(x - halfW, sizeBottom));
        if (IsSolid(leftBottom, layerBottom)) return true;

        int rightBottom = Mathf.FloorToInt(Mathf.PosMod(x + halfW, sizeBottom));
        if (IsSolid(rightBottom, layerBottom)) return true;
        
        return false;
    }
}