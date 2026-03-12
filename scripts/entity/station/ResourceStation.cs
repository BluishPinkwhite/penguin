using Godot;
using Incremental.scripts.director;

namespace Incremental.scripts.entity.station;

public partial class ResourceStation: OrbitEntity
{
    public static ResourceStation I;
    public ResourceStation() { I = this; }


    public Vector2 Below;
    
    
    public override void _Ready()
    {
        base._Ready();

        _polar_pos = new Vector2(10.5f, Game.I._data.Layers.Count + 10.5f);
        Below = new Vector2(_polar_pos.X, _polar_pos.Y - 10.25f);
    }
}