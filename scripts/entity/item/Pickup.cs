using Godot;
using Incremental.scripts.director;
using Incremental.scripts.director.data;

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

        pickup.Sprite.RegionRect = pickup.GetPickupCoords();

        pickup.Sprite.Rotation = Game.RandomAround(0, Mathf.Pi * 0.3f);
        Game.I.Pickups.AddChild(pickup);

        return pickup;
    }

    public Rect2 GetPickupCoords()
    {
        return new Rect2(32 * (Item.RenderIndex() % 8), 32 * (int)(Item.RenderIndex() / 8), 32, 32);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (ApplyGravity((float)delta))
        {
            DoLayerChecks();
            ApplyPolarTransform();
        }
    }
}