using System.Collections.Generic;
using Godot;
using Incremental.scripts.director;
using Incremental.scripts.director.data;
using Incremental.scripts.director.data.recipe;
using Incremental.scripts.entity.item;
using Incremental.scripts.entity.station;
using Incremental.scripts.planet.data;

namespace Incremental.scripts.entity.pawn.roles;

public partial class PawnArcheologist : PawnMiner
{
    public override void _Ready()
    {
        base._Ready();
        visual.FrameChanged += OnFrameChanged;
    }
    
    private void OnFrameChanged()
    {
        if (visual.Animation != "brush" || visual.Frame != 3) return;
        if (_targetTile == null || _targetTile.IsEmpty()) return;
        
        if (_targetTile.Material == TileMaterial.Core)
        {
            _targetTile = null;
            State = PawnState.Idle;
            return;
        }
        
        float damage = 0.1f;
        damage += Inventory.Items[Item.Finer_Brushes].Amount * 0.1f;
        damage /= _targetTile.Material.BreakTime();
        
        _targetTile.Integrity -= damage;
        Game.I.Pawns.RecordDamage(Role, damage);

        SFX.PitchScale = (float)GD.RandRange(1.8f, 2.4f);
        SFX.VolumeDb = (float)GD.RandRange(-8f, -4f);
        SFX.Play();

        if (_targetTile.Integrity <= 0)
        {
            BreakTile(_targetTile);

            List<(Item item, int amount)> products = ItemRecipe.TryGetOutput(RecipeID.Gather_Component);
            foreach ((Item item, int amount) tuple in products)
            {
                int amount = tuple.amount;

                // increase chance with depth
                float chance = 0.25f * (1 - (float)_targetTile.PolarY / Game.I._data.Layers.Count);

                if (Inventory.IsResearchUnlocked(RecipeID.Research_BiggerZoomLens))
                    chance += 0.15f;
                if (Inventory.IsResearchUnlocked(RecipeID.Research_FinerBrushes))
                    chance += Inventory.Items[Item.Finer_Brushes].Amount * 0.1f;

                if (tuple.item == Item.Gem)
                    chance /= 3;

                if (GD.Randf() < chance)
                    amount++;

                if (GD.Randf() < chance / 2f)
                    amount++;

                for (int i = 0; i < amount; i++)
                {
                    Pickup.Instantiate(PolarPos, tuple.item);
                }

                if (!Inventory.IsResearchUnlocked(RecipeID.Research_BiggerZoomLens))
                    break; // disallow gem gathering without upgrade
            }

            Counter++;

            State = PawnState.ReturnH;
            Target = ResearchStation.I.PolarTouchPos();
            SetCooldown(1);
        }
    }

    protected override void UpdateAnimationState()
    {
        if (_isOnCooldown || State == PawnState.Idle)
        {
            visual.Animation = "idle";
        }
        else if (State is PawnState.Move or PawnState.ReturnH)
        {
            visual.Animation = Flying ? "fly" : "walk";
        }
        else if (State == PawnState.ReturnV)
        {
            visual.Animation = "fly";
        }
        else if (State == PawnState.Action)
        {
            visual.Animation = "brush";
        }
    }
}