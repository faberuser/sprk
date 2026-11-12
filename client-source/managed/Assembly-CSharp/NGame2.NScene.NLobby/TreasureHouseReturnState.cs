using System;
using NGame2.NBattleContext;
using NGame2.NUI.NManager;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NScene.NLobby;

public class TreasureHouseReturnState : LobbyScene2.IReturnState
{
	public string BgmCode => "";

	void LobbyScene2.IReturnState.Execute(LobbyScene2.ReturnStateInfo info, Action onEnd)
	{
		Action onOpen = null;
		if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenHeroDeck) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			onOpen = delegate
			{
				SceneAutoBehaviour<MyHeroManagement>.instance.OpenMyDeck();
			};
		}
		else if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenPartySetting) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			int chapterIndex = SingletonAuto<BattleContext>.instance.LastBattleChapterIndex;
			int dungeonIndex = SingletonAuto<BattleContext>.instance.LastBattleDungeonIndex;
			onOpen = delegate
			{
				SceneAutoBehaviour<TreasureHouseManagement>.instance.OpenEnterDungeon(chapterIndex, dungeonIndex);
			};
		}
		else if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenInventory) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			onOpen = delegate
			{
				SceneAutoBehaviour<InventoryManagement>.instance.OpenInventory();
			};
		}
		else if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenCashShop) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			onOpen = delegate
			{
				SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Ruby.ToString());
			};
		}
		else if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenNewbieShop) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			onOpen = delegate
			{
				SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Beginner.ToString());
			};
		}
		else if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenEventShop) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			onOpen = delegate
			{
				SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Event.ToString());
			};
		}
		SceneAutoBehaviour<TreasureHouseManagement>.instance.OpenTreasureHouseWindow(onOpen);
		Vespa_System_Action_Extension_Method.fire(onEnd);
	}
}
