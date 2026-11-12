using System;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NScene.NLobby;

public class InventoryReturnStateMultipleSelectHero : LobbyScene2.IReturnState
{
	private ItemData _itemData;

	private CreatureData _heroData1;

	private CreatureData _heroData2;

	private CreatureData _lastSelectedHeroData;

	private int _lastSelectSlot;

	public string BgmCode => string.Empty;

	public InventoryReturnStateMultipleSelectHero(ItemData itemData, CreatureData heroData1 = null, CreatureData heroData2 = null, CreatureData lastSelectedHero = null, int lastSelectedSlot = 0)
	{
		_itemData = itemData;
		_heroData1 = heroData1;
		_heroData2 = heroData2;
		_lastSelectedHeroData = lastSelectedHero;
		_lastSelectSlot = lastSelectedSlot;
	}

	public void Execute(LobbyScene2.ReturnStateInfo info, Action onEnd)
	{
		SceneAutoBehaviour<InventoryManagement>.instance.RestoreMultipleHeroSelect(_itemData, _heroData1, _heroData2, _lastSelectedHeroData, _lastSelectSlot);
		Vespa_System_Action_Extension_Method.fire(onEnd);
	}
}
