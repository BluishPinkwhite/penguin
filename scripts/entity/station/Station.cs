using Godot;
using Incremental.scripts.director;

namespace Incremental.scripts.entity.station;

public partial class Station : OrbitEntity
{
    [Export] private Node2D TouchPoint;

    public Vector2 PolarTouchPos()
    {
        Game.I._data.LocalPositionToPolarCoordsUnbounded(TouchPoint.GlobalPosition, out int layer, out int tile);
        return new Vector2(tile, layer);
    }
}