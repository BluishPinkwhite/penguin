using Godot;
using Incremental.scripts.director;

namespace Incremental.scripts.entity;

public partial class OrbitEntity : Node2D
{
    protected Vector2 _polar_pos; // X = tile, Y = layer
    protected Vector2 _target;

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        
        ApplyPolarTransform();
    }

    protected void ApplyPolarTransform()
    {
        // apply and display
        Position = Game.I._data.PolarToWorld(_polar_pos.X, _polar_pos.Y);
        Rotation = Mathf.Atan2(Position.Y, Position.X) + Mathf.Pi / 2f;
    }

    public Vector2 PolarPos => _polar_pos;
}