using System;
using System.Collections.Generic;
using Godot;
using Incremental.scripts.planet.rendering;

namespace Incremental.scripts.debug;

public partial class DebugDraw : Node2D
{
    private static Dictionary<int, Color> _colors = new();
    private Dictionary<int, (Vector2 start, Vector2 end, Color color)> _lines = new();

    public void SetLine(int id, Vector2 start, Vector2 end, Color color)
    {
        _lines[id] = (start, end, color);
        QueueRedraw();
    }

    public override void _Draw()
    {
        foreach (var line in _lines.Values)
        {
            DrawLine(line.start, line.end, line.color, 1.0f);
        }
    }
    
    public void RemoveLine(int ID)
    {
        _lines.Remove(ID);
        QueueRedraw();
    }

    private static Random _r = new(2); 
    public static Color GetColor(int id)
    {
        if (!_colors.ContainsKey(id))
        {
            _colors[id] = Color.FromHsv(_r.NextSingle(), 1, 1);
        }
        return _colors[id];
    }
}