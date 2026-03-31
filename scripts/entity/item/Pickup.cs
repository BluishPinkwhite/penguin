using Godot;
using Incremental.scripts.director;

namespace Incremental.scripts.entity.item;

public partial class Pickup : SurfaceEntity
{
    [Export] private Sprite2D Sprite;
    
    public Item Item { private set; get; }
    
    public static Pickup Instantiate(Vector2 PolarPos, Item item)
    {
        Pickup pickup = Game.I.PickupScene.Instantiate<Pickup>();
        pickup.PolarPos = PolarPos;
        pickup.Item = item;

        pickup.Sprite.RegionRect = new Rect2(32 * ((int)pickup.Item % 8), 32 * (int)((int)pickup.Item / 4), 32, 32);
        
        Game.I.Pickups.AddChild(pickup);
        
        return pickup;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        ApplyGravity((float)delta);
    }
}