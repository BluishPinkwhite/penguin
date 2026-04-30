using Godot;
using Incremental.scripts.saving;

namespace Incremental.ui;

public partial class GameTime : Label
{
    public override void _Ready()
    {
        double time = SaveFileManager.LoadGameTime();
        
        int hours = (int) (time / 3600);
        int minutes = (int) ((time % 3600) / 60);
        int seconds = (int) (time % 60);
        
        if (hours > 0)
            Text = $"[{hours}h {minutes}m {seconds}s]]";
        else
            Text = $"[{minutes}m {seconds}s]";
    }
}