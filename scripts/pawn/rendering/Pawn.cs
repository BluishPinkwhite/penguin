using System;
using Godot;
using Incremental.scripts.director;
using Incremental.scripts.planet.data;

namespace Incremental.scripts.pawn.rendering;

public partial class Pawn : Node2D
{
    private Vector2 _polar_pos; // X = tile, Y = layer
    private int _prevLayer;

    public override void _Ready()
    {
        _polar_pos = new Vector2(600, 120);
        _prevLayer = (int)_polar_pos.Y;
    }

    public override void _PhysicsProcess(double delta)
    {
        float d = (float)delta;

        int prevSize = Game.I._data.GetLayerSize(_prevLayer);
        int currSize = Game.I._data.GetLayerSize((int)_polar_pos.Y);
        
        _polar_pos.X -= d * currSize / 10f;


        int layer = (int)_polar_pos.Y;
        int tile = (int)_polar_pos.X;

        PlanetTile below = Game.I._data.GetTileAtPolarCoords(Math.Max(0, layer - 1), tile);
        

        // gravity
        if (below == null || below.Destroyed || below.Material == TileMaterial.Unknown)
            _polar_pos.Y -= d * 10; // TODO prevent clipping (via substep or up-shift)

        if (_polar_pos.Y < 0)
            _polar_pos.Y = 0;
        
        if (currSize != prevSize)
            _polar_pos.X *= (float)currSize / prevSize;

        if (_polar_pos.X > currSize)
            _polar_pos.X -= currSize;

        if (_polar_pos.X < 0)
            _polar_pos.X += currSize;

        Position = Game.I._data.PolarToWorld(_polar_pos.X, _polar_pos.Y);
        Rotation = Mathf.Atan2(Position.Y, Position.X) + Mathf.Pi / 2f;
        
        _prevLayer = layer;

        if (_polar_pos.Y == 0)
        {
            _polar_pos.Y = 80;
        }
    }
}