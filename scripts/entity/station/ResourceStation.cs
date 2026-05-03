using Godot;
using Incremental.scripts.director;

namespace Incremental.scripts.entity.station;

public partial class ResourceStation: Station
{
    public static ResourceStation I;
    public ResourceStation() { I = this; }
    
    public Vector2 Surface;
    
    
    public override void _Ready()
    {
        base._Ready();
        
        Surface = new Vector2(PolarPos.X, Game.I._data.Layers.Count - 0.25f);
    }
}