using Godot;

namespace Incremental.ui;

public partial class TreeViewControl : Control, IUpdatable
{
    [Export] public float ZoomSpeed = 0.1f;
    [Export] public float MinZoom = 0.5f;
    [Export] public float MaxZoom = 2.0f;
    
    [Export] private Control _movedObject;
    [Export] private Control _scaledObject;

    private bool _dragging;
    
    private Rect2 _moveBounds;

    public override void _Ready()
    {
        base._Ready();
        Position = Vector2.Zero;

        bool first = true;
        foreach (Node node in _scaledObject.GetChildren())
        {
            if (node is ResearchNode research)
            {
                if (first)
                {
                    first = false;
                    _moveBounds = new Rect2(research.Position, Vector2.Zero);
                }
                else
                {
                    _moveBounds = _moveBounds.Expand(research.Position);
                    _moveBounds = _moveBounds.Expand(research.Position + research.Size);
                }
            }
        }

        LimitPosition();
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
            LimitPosition();
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
        
        LimitPosition();
    }

    private void LimitPosition()
    {
        Vector2 scale = _scaledObject.Scale;
        Vector2 view = Size; 
    
        float margin = 200.0f; 

        float scaledLeft   = _moveBounds.Position.X * scale.X;
        float scaledRight  = _moveBounds.End.X * scale.X;
        float scaledTop    = _moveBounds.Position.Y * scale.Y;
        float scaledBottom = _moveBounds.End.Y * scale.Y;

        float minX = margin - scaledRight;
        float maxX = view.X - margin - scaledLeft;
        float minY = margin - scaledBottom;
        float maxY = view.Y - margin - scaledTop;

        Vector2 newPos = _movedObject.Position;

        if (minX <= maxX)
            newPos.X = Mathf.Clamp(newPos.X, minX, maxX);
        else
            newPos.X = (view.X / 2.0f) - (_moveBounds.GetCenter().X * scale.X);

        if (minY <= maxY)
            newPos.Y = Mathf.Clamp(newPos.Y, minY, maxY);
        else
            newPos.Y = (view.Y / 2.0f) - (_moveBounds.GetCenter().Y * scale.Y);

        _movedObject.Position = newPos;
    }
    
    public void UpdateVisuals()
    {
        foreach (Node child in GetNode<Control>("Move/Scale").GetChildren())
        {
            if (child is ResearchNode research)
            {
                research.UpdateVisuals();
            }
        }
    }
}