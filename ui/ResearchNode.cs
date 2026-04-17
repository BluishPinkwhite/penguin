using Godot;
using Godot.Collections;

namespace Incremental.ui;

public partial class ResearchNode : Control
{
    [Export] private Array<ColorRect> connectors;
    [Export] private Array<ResearchNode> requiredNodes;
    [Export] private TextureRect icon;
    private ShaderMaterial iconMaterial;

    [Export] private Control hint;

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
        hint.PivotOffset = (hint.Position) * (mult-1.4174368f) * mult * mult * mult * mult * 14;

        if (!IsPurchasable())
            icon.Visible = false;
    }

    public void PurchaseUpgrade()
    {
        if (IsPurchased) return;

        if (!IsPurchasable()) return;

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
                && research.IsPurchasable()) 
            {
                research.icon.Visible = true;
            }
        }
    }

    public void OnMouseEnter()
    {
        if (!IsPurchasable()) return;

        hint.Show();
        _isHovering = true;
    }

    public void OnMouseExit()
    {
        if (!IsPurchasable()) return;

        hint.Hide();
        _isHovering = false;
    }

    private bool IsPurchasable()
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