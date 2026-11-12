using System;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NScene.NLobby;

public class InventoryReturnState : LobbyScene2.IReturnState
{
	private ItemData _itemData;

	private CreatureData _heroData;

	private HeroSelectItemType _heroSelectItemType;

	public string BgmCode => string.Empty;

	public InventoryReturnState(ItemData itemData, CreatureData heroData = null, HeroSelectItemType heroSelectItemType = HeroSelectItemType.Default)
	{
		_itemData = itemData;
		_heroData = heroData;
		_heroSelectItemType = heroSelectItemType;
	}

	public void Execute(LobbyScene2.ReturnStateInfo info, Action onEnd)
	{
		SceneAutoBehaviour<InventoryManagement>.instance.RestoreHeroSelect(_itemData, _heroData, _heroSelectItemType);
		Vespa_System_Action_Extension_Method.fire(onEnd);
	}
}
