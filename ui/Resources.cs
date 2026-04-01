using System.Collections.Generic;
using System.Linq;
using Godot;
using Incremental.scripts.director;

public partial class Resources : BoxContainer
{
    public static Resources I;
    
    private List<BoxContainer> _containers = new();
    private List<Label> _labels = new();
    
    [Export] private BoxContainer _penguinContainer;
    private Label _penguinLabel;

    public Resources()
    {
        I = this;
    }

    public override void _Ready()
    {
        base._Ready();
        
        foreach (Node child in GetChildren())
        {
            if (child is BoxContainer container)
            {
                _containers.Add(container);
                _labels.Add(container.GetNode<Label>("Count"));
                container.Visible = false;
            }
        }

        _penguinLabel = _penguinContainer.GetNode<Label>("Count");

        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        foreach (KeyValuePair<Item, int> pair in Inventory.Items)
        {
            if ((int)pair.Key < _containers.Count)
            {
                if (pair.Value > 0)
                {
                    _containers[(int)pair.Key].Visible = true;
                    _labels[(int)pair.Key].Text = pair.Value.ToString();
                }
                else
                {
                    _containers[(int)pair.Key].Visible = false;
                }
            }
        }

        _penguinLabel.Text = Inventory.Roles.Values.Sum().ToString();
    }
}
