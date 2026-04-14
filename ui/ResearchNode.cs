using Godot;
using Godot.Collections;

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
	}

	public void PurchaseUpgrade()
	{
		if (IsPurchased) return;
		
		bool canPurchase = true;
		foreach (ResearchNode requiredNode in requiredNodes)
			canPurchase &= requiredNode.IsPurchased;
		
		if (!canPurchase) return;
		
		foreach (ColorRect connector in connectors)
		{
			connector.Color = Colors.AliceBlue;
		}
		iconMaterial.SetShaderParameter("EffectOn", false);
		
		IsPurchased = true;
	}

	public void OnMouseEnter()
	{
		bool canPurchase = true;
		foreach (ResearchNode requiredNode in requiredNodes)
			canPurchase &= requiredNode.IsPurchased;
		
		if (!canPurchase) return;
		
		hint.Show();
		_isHovering = true;
	}

	public void OnMouseExit()
	{
		bool canPurchase = true;
		foreach (ResearchNode requiredNode in requiredNodes)
			canPurchase &= requiredNode.IsPurchased;
		
		if (!canPurchase) return;
		
		hint.Hide();
		_isHovering = false;
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
