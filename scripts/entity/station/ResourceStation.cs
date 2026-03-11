using Godot;
using Incremental.scripts.director;

namespace Incremental.scripts.entity.station;

public partial class ResourceStation: OrbitEntity
{
    public static ResourceStation I;

    public ResourceStation()
    {
        I = this;
    }
    
    public override void _Ready()
    {
        base._Ready();

        _polar_pos = new Vector2(10, Game.I._data.Layers.Count + 10.5f);
    }
}