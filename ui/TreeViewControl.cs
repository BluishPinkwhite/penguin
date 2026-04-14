using Godot;

public partial class TreeViewControl : Control
{
    [Export] public float ZoomSpeed = 0.1f;
    [Export] public float MinZoom = 0.5f;
    [Export] public float MaxZoom = 2.0f;
    
    [Export] private Control _movedObject;
    [Export] private Control _scaledObject;

    private bool _dragging;

    public override void _Ready()
    {
        base._Ready();
        Position = Vector2.Zero;
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Left)
                _dragging = mouseButton.Pressed;

            if (mouseButton.Pressed)
            {
                if (mouseButton.ButtonIndex == MouseButton.WheelUp)
                    ZoomAtPoint(1.0f + ZoomSpeed, mouseButton.Position);

                if (mouseButton.ButtonIndex == MouseButton.WheelDown)
                    ZoomAtPoint(1.0f - ZoomSpeed, mouseButton.Position);
            }
        }

        if (@event is InputEventMouseMotion motion && _dragging)
        {
            _movedObject.Position += motion.Relative;
        }
    }

    private void ZoomAtPoint(float zoomFactor, Vector2 mousePos)
    {
        Vector2 oldScale = _scaledObject.Scale;
        Vector2 newScale = oldScale * zoomFactor;

        newScale.X = Mathf.Clamp(newScale.X, MinZoom, MaxZoom);
        newScale.Y = Mathf.Clamp(newScale.Y, MinZoom, MaxZoom);

        Vector2 scaleRatio = newScale / oldScale;

        _movedObject.Position = (_movedObject.Position - mousePos) * scaleRatio + mousePos;
        _scaledObject.Scale = newScale;
    }
}