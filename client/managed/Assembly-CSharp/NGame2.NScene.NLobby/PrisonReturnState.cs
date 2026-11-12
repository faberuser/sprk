using System;
using NGame2.NBattleContext;
using NGame2.NUI.NManager;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NScene.NLobby;

public class PrisonReturnState : BaseEntryMenuReturnState
{
	private int _diff;

	public PrisonReturnState(int diff)
	{
		_diff = diff;
	}

	public override void Execute(LobbyScene2.ReturnStateInfo info, Action onEnd)
	{
		if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenHeroDeck) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			SceneAutoBehaviour<MultipleDungeonManagement>.instance.OpenPrisonList();
			SceneAutoBehaviour<MyHeroManagement>.instance.OpenMyDeck();
		}
		else if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenPartySetting) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			int chapterIndex = SingletonAuto<BattleContext>.instance.LastBattleChapterIndex;
			int dungeonIndex = SingletonAuto<BattleContext>.instance.LastBattleDungeonIndex;
			SceneAutoBehaviour<MultipleDungeonManagement>.instance.OpenPrisonList(delegate
			{
				VespaCoroutine.Start(SceneAutoBehaviour<MultipleDungeonManagement>.instance.ReturnStateOpenBattlePartySetting(chapterIndex, dungeonIndex, _diff));
			});
		}
		else if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenInventory) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			SceneAutoBehaviour<MultipleDungeonManagement>.instance.OpenPrisonList();
			SceneAutoBehaviour<InventoryManagement>.instance.OpenInventory();
		}
		else if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenCashShop) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Ruby.ToString());
		}
		else if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenNewbieShop) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			Action onOpen = delegate
			{
				SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Beginner.ToString());
			};
			SceneAutoBehaviour<MultipleDungeonManagement>.instance.OpenPrisonList(onOpen);
		}
		else if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenEventShop) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			Action onOpen2 = delegate
			{
				SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Event.ToString());
			};
			SceneAutoBehaviour<MultipleDungeonManagement>.instance.OpenPrisonList(onOpen2);
		}
		else if (info.OpenFlag == LobbyScene2.ReturnStateOpenFlag.None && info.DisabledFlag == LobbyScene2.ReturnStateOpenFlag.None)
		{
			SceneAutoBehaviour<MultipleDungeonManagement>.instance.OpenPrisonList();
		}
		Vespa_System_Action_Extension_Method.fire(onEnd);
	}
}
