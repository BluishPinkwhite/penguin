using Godot;
using Incremental.scripts.director;

namespace Incremental.scripts.entity;

public partial class OrbitEntity : Node2D
{
    public Vector2 PolarPos; // X = tile, Y = layer
    public Vector2 Target;

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        
        ApplyPolarTransform();
    }

    protected void ApplyPolarTransform()
    {
        // apply and display
        Position = Game.I._data.PolarToWorld(PolarPos.X, PolarPos.Y);
        Rotation = Mathf.Atan2(Position.Y, Position.X) + Mathf.Pi / 2f;
    }
}