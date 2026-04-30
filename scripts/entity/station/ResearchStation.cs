using Incremental.scripts.director;
using Incremental.scripts.director.data;

namespace Incremental.scripts.entity.station;

public partial class ResearchStation : OrbitEntity
{
	private bool _isUnlocked;
	
	public static ResearchStation I;
	public ResearchStation() { I = this; }
	
	public override void _Process(double delta)
	{
		base._Process(delta);
		
		if (_isUnlocked) return;
		if (Inventory.Items[Item.Research_Station].Amount <= 0) return;
		
		SetVisible(true);
		_isUnlocked = true;
	}
}