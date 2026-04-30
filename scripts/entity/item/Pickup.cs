using Godot;
using Incremental.scripts.director;
using Incremental.scripts.director.data;

namespace Incremental.scripts.entity.item;

public partial class Pickup : SurfaceEntity
{
    [Export] private Sprite2D Sprite;

    public Item Item { private set; get; }
    public int Amount = 1;

    public static Pickup Instantiate(Vector2 PolarPos, Item item)
    {
        foreach (Node node in Game.I.Pickups.GetChildren())
        {
            if (node is Pickup other
                && other.Item == item
                && (other.PolarPos - PolarPos).Length() < 0.25f)
            {
                other.Amount++;
                return other;
            }
        }
        
        Pickup pickup = Game.I.PickupScene.Instantiate<Pickup>();
        pickup.PolarPos = PolarPos;
        pickup.Item = item;

        pickup.Sprite.RegionRect = pickup.GetAtlasCoords();

        pickup.Sprite.Rotation = Game.RandomAround(0, Mathf.Pi * 0.3f);
        Game.I.Pickups.AddChild(pickup);

        return pickup;
    }

    public Rect2 GetAtlasCoords()
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