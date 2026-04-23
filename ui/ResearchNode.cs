using Godot;
using Godot.Collections;
using Incremental.scripts.director.data;
using Incremental.scripts.director.data.recipe;

namespace Incremental.ui;

public partial class ResearchNode : Control, IUpdatable
{
    [Export] private RecipeID Research;

    [Export] private Array<ColorRect> connectors;
    [Export] private Array<ResearchNode> requiredNodes;
    [Export] private TextureRect icon;

    [Export] private Control hint;


    private ShaderMaterial iconMaterial;


    private bool _isHovering;
    public bool IsPurchased;

    public override void _Ready()
    {
        base._Ready();

        hint.Hide();
        MouseEntered += OnMouseEnter;
        MouseExited += OnMouseExit;

        foreach (ColorRect connector in connectors)
        {
            connector.Color = Colors.DimGray;
        }

        iconMaterial = (ShaderMaterial)icon.GetMaterial();
        iconMaterial.SetShaderParameter("EffectOn", true);

        float mult = hint.Scale.X / Scale.X;
        hint.Scale /= Scale;
        hint.PivotOffset = (hint.Position) * (mult - 1.4174368f) * mult * mult * mult * mult * 14;

        if (ItemRecipe.GetRecipe(Research, out ItemRecipe recipe))
        {
            string ingredients = "\n\n  ";
            foreach (Ingredient ingredient in recipe.Ingredients)
            {
                ingredients += ingredient.RenderText + "\n  ";
            }
            GetNode<RichTextLabel>("Hint/MarginContainer/VBoxContainer/RichTextLabel").Text 
                += ingredients;
        }

        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        if (IsPurchased) return;
        
        iconMaterial.SetShaderParameter("Brightness", IsReachable() ? 0.75f : 0.1f);
    }

    public void PurchaseUpgrade()
    {
        if (IsPurchased) return;

        if (!IsPurchasable()) return;
        
        if(!ItemRecipe.TryApplyRecipe(Research)) return;

        foreach (ColorRect connector in connectors)
        {
            connector.Color = Colors.AliceBlue;
        }

        iconMaterial.SetShaderParameter("EffectOn", false);
        icon.Visible = true;
        IsPurchased = true;

        foreach (Node child in GetParent().GetChildren())
        {
            if (child is ResearchNode research
                && research.requiredNodes.Contains(this)
                && research.IsReachable())
            {
                research.icon.Visible = true;
                research.UpdateVisuals();
            }
        }

        
    }

    public void OnMouseEnter()
    {
        if (!IsReachable()) return;

        hint.Show();
        _isHovering = true;
    }

    public void OnMouseExit()
    {
        if (!IsReachable()) return;

        hint.Hide();
        _isHovering = false;
    }

    private bool IsPurchasable()
    {
        if (!IsReachable())
            return false;

        if (ItemRecipe.GetRecipe(Research, out ItemRecipe recipe))
            return recipe.HasIngredients();

        return false;
    }
    
    private bool IsReachable()
    {
        bool canPurchase = true;
        foreach (ResearchNode requiredNode in requiredNodes)
            canPurchase &= requiredNode.IsPurchased;

        return canPurchase;
    }

    public override void _Input(InputEvent @event)
    {
        if (!_isHovering) return;

        base._Input(@event);
        if (@event.IsActionPressed("press"))
        {
            OnClick();
        }
    }

    private void OnClick()
    {
        PurchaseUpgrade();
    }
}