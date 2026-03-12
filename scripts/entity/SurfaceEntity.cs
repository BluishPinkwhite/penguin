using Godot;
using Incremental.scripts.director;
using Incremental.scripts.planet.data;

namespace Incremental.scripts.entity;

public partial class SurfaceEntity : OrbitEntity
{
    protected int _prevLayer;
    protected int _currSize;

    public override void _Ready()
    {
        _prevLayer = (int)_polar_pos.Y;
        base._Ready();
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        
        int layer = Mathf.FloorToInt(_polar_pos.Y);
        int prevSize = Game.I._data.GetLayerSize(_prevLayer);
        _currSize = Game.I._data.GetLayerSize(layer);

        if (layer != _prevLayer)
            _polar_pos.X = _polar_pos.X * prevSize / _currSize;
        _polar_pos.X = Mathf.PosMod(_polar_pos.X, _currSize);
        
        
        if (_polar_pos.Y < 0f)
            _polar_pos.Y = 0f;
        
        _prevLayer = layer;
    }
    
    protected float CircularDelta(float from, float to, float size)
    {
        float diff = Mathf.PosMod(to - from, size);

        if (diff > size / 2f)
            diff -= size;

        return diff;
    }

    protected float ScaleTileBetweenLayers(float tile, int fromLayer, int toLayer)
    {
        int fromSize = Game.I._data.GetLayerSize(fromLayer);
        int toSize = Game.I._data.GetLayerSize(toLayer);

        return tile * toSize / (float)fromSize;
    }

    protected bool IsAir(int tile, int layer)
    {
        if (layer < 0)
            return false;

        int size = Game.I._data.GetLayerSize(layer);
        tile = Mathf.PosMod(tile, size);

        PlanetTile t = Game.I._data.GetTileAtPolarCoords(tile, layer);

        return t == null || t.Destroyed || t.Material == TileMaterial.Unknown;
    }

    protected bool IsSolid(int tile, int layer)
    {
        PlanetTile t = Game.I._data.GetTileAtPolarCoords(tile, layer);

        return t != null &&
               !t.Destroyed &&
               t.Material != TileMaterial.Unknown;
    }
    
}