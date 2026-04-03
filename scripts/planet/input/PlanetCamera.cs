using Godot;
using Incremental.scripts.director;
using Incremental.scripts.planet.rendering;

namespace Incremental.scripts.planet.input;

public partial class PlanetCamera : Camera2D
{
    public float Radius; // 0-1 of planet size
    public float Angle; // radians

    [Export] private rendering.PlanetRenderer world;
    
    [Export] public float MoveSpeed = 400f;
    [Export] public float ZoomLevel = 1.0f;
    [Export] public float ZoomSpeed = 1.5f;
    [Export] public float MinZoom = 0.3f;
    [Export] public float MaxZoom = 3.0f;


    public override void _Ready()
    {
        Angle = 0;
        Radius = 1;
    
        MakeCurrent();
    }
    
    public override void _Process(double delta)
    {
        float dt = (float)delta;

        if (Input.IsActionPressed("ui_left"))
            Angle -= dt * MoveSpeed * 0.002f;

        if (Input.IsActionPressed("ui_right"))
            Angle += dt * MoveSpeed * 0.002f;

        if (Input.IsActionPressed("ui_up"))
            Radius += dt * 0.4f;

        if (Input.IsActionPressed("ui_down"))
            Radius -= dt * 0.4f;

        Radius = Mathf.Clamp(Radius, 0f, 1.2f);
        Angle = Mathf.PosMod(Angle, Mathf.Tau);

        float planetRadius = Game.I._data.TileSize * 100f;
        float r = Radius * planetRadius;

        Vector2 offset = new Vector2(
            Mathf.Cos(Angle),
            Mathf.Sin(Angle)
        ) * r;

        Position = world.Position + offset;
        Rotation = Angle + Mathf.Pi / 2f;
        
        if (Input.IsActionPressed("zoom_in"))
            ZoomLevel -= ZoomSpeed * dt;

        if (Input.IsActionPressed("zoom_out"))
            ZoomLevel += ZoomSpeed * dt;

        ZoomLevel = Mathf.Clamp(ZoomLevel, MinZoom, MaxZoom);
        Zoom = new Vector2(ZoomLevel, ZoomLevel);
    }

}