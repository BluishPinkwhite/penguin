using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Incremental.ui;

public partial class MessageLog : Label
{
    private static MessageLog I;
    
    private static int MaxLines = 10;
    private static List<(double time, string message)> _messages = new();
    private static bool _dirty = false;

    public MessageLog()
    {
        I = this;
    }
    
    public override void _Ready()
    {
        I = this;
        I.Text = "";
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_messages.Count > 0)
        {
            if (_messages.RemoveAll(m => 
                    m.time < Time.GetUnixTimeFromSystem() - 12) > 0)
                _dirty = true;
        }
        
        if (!_dirty)
            return;
        
        if (_messages.Count > 0)
        {
            if (_messages.Count > MaxLines)
                I.Text = string.Join("\n", _messages.GetRange(0, MaxLines).Select(m => m.message));
            else
                I.Text = string.Join("\n", _messages.Select(m => m.message));
        }
        else 
            I.Text = "";

        _dirty = false;
    }

    public static void QueueMessage(string message)
    {
        _messages.Add((Time.GetUnixTimeFromSystem(), message));
        _dirty = true;
    }
}