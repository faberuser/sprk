using NGame2.NUI.NComponent2;

namespace NGame2.NUI.NWindow.NEquipTreasure;

internal interface ITreasureSlotStateHandler
{
	bool ShowSelectPanel();

	void OnChangeState(TreasureSlotComponent component);

	void OnSelect(TreasureSlotComponent component);
}
