using NGame2.NUI.NComponent2;
using NVespa;

namespace NGame2.NUI.NWindow.NEquipTreasure;

public class NoneState : NGame2.NUI.NWindow.NEquipTreasure.ITreasureSlotStateHandler
{
	private HeroEquipTreasure.Listener _listener;

	public NoneState(HeroEquipTreasure.Listener listener)
	{
		_listener = listener;
		if (listener == null)
		{
			VespaDebug.LogError("listener must be not null");
		}
	}

	public void OnChangeState(TreasureSlotComponent component)
	{
		if (!(component == null))
		{
			component.ResetUI();
		}
	}

	public void OnSelect(TreasureSlotComponent component)
	{
		if (!(component == null))
		{
			_listener.OnClickItemIcon(component.ItemIconComponent);
		}
	}

	public bool ShowSelectPanel()
	{
		return false;
	}
}
