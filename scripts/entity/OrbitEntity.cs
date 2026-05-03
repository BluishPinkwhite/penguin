using Godot;
using Incremental.scripts.director;

namespace Incremental.scripts.entity;

public abstract partial class OrbitEntity : Node2D
{
    [Export] public Vector2 PolarPos; // X = tile, Y = layer
    public Vector2 PrevPolarPos;
    public Vector2 Target;

    public override void _Ready()
    {
        if (PolarPos.Y < 0)
        {
            PolarPos.Y = -PolarPos.Y + Game.I._data.Layers.Count; 
        }
        
        ApplyPolarTransform();
    }

    public void ApplyPolarTransform()
    {
        // apply and display
        Position = Game.I._data.PolarToWorld(PolarPos.X, PolarPos.Y);
        Rotation = Mathf.Atan2(Position.Y, Position.X) + Mathf.Pi / 2f;
        
        PrevPolarPos = new Vector2(PolarPos.X, PolarPos.Y);
    }
}