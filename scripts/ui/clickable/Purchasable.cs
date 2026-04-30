using Godot;
using Incremental.scripts.director;
using Incremental.scripts.director.data;
using Incremental.scripts.director.data.recipe;
using Incremental.ui;
using Incremental.ui.util;

namespace Incremental.scripts.ui.clickable;

public partial class Purchasable : Clickable, IUpdatable
{
    [Export] private Label _costLabel;
    [Export] private TextureRect _costIcon;
    [Export] private Label _purchasableNameLabel;

    [Export] private RecipeID _recipe;


    public override void _Ready()
    {
        base._Ready();

        Resources.I._purchasables.Add(this);

        UpdateVisuals();
    }

    protected override void OnMouseEnter()
    {
        base.OnMouseEnter();
        _purchasableNameLabel.SelfModulate = UIConsts.highlighted;
    }

    protected override void OnMouseExit()
    {
        base.OnMouseExit();
        _purchasableNameLabel.SelfModulate = UIConsts.active;
    }

    public override void OnClick()
    {
        TryPurchase();
        base.OnClick();
    }

    private void TryPurchase()
    {
        ItemRecipe.TryApplyRecipe(_recipe);
    }

    public void UpdateVisuals()
    {
        if (Inventory.Recipes.TryGetValue(_recipe, out ItemRecipe recipe))
        {
            SetVisible(recipe.Unlocked);

            // TODO extend for multiple items
            if (recipe.Ingredients.Count > 0)
            {
                if (recipe.Ingredients[0].Item.Renderable())
                {
                    ((AtlasTexture)_costIcon.Texture).Region = ((AtlasTexture)_costIcon.Texture).Region with
                    {
                        Position = new Vector2(32 * (recipe.Ingredients[0].Item.RenderIndex() % 8),
                            32 * (int)(recipe.Ingredients[0].Item.RenderIndex() / 8))
                    };
                }

                _costLabel.Text = (recipe.Ingredients[0].RenderCost).ToString();
            }

            if (recipe.Products.Count > 0)
            {
                _purchasableNameLabel.Text = recipe.Products[0].Item.ToString().Replace("_", " ");
            }
        }
        else
        {
            GD.PrintErr($"Purchasable not found for recipe: {_recipe} ({_recipe.ToString()})");
        }
    }
}