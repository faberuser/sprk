using System;
using NGame2.NBattleContext;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NScene.NLobby;

public class ParttimeDungeonReturnState : BaseEntryMenuReturnState
{
	protected EnterDungeonData _enterDungeon;

	protected PurchaseDungeonData _purchaseDungeon;

	public ParttimeDungeonReturnState(EnterDungeonData enterDungeon, PurchaseDungeonData purchaseDungeon)
	{
		_enterDungeon = enterDungeon;
		_purchaseDungeon = purchaseDungeon;
	}

	public override void Execute(LobbyScene2.ReturnStateInfo info, Action onEnd)
	{
		if (!SceneAutoBehaviour<ParttimeDungeonManagement>.instance.IsOpened<ParttimeDungeonList>())
		{
			SceneAutoBehaviour<ParttimeDungeonManagement>.instance.OpenParttimeDungeonList();
		}
		if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenHeroDeck) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			SceneAutoBehaviour<MyHeroManagement>.instance.OpenMyDeck();
		}
		else if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenPartySetting) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			SceneAutoBehaviour<ParttimeDungeonManagement>.instance.OpenBattleDungeonInfo(_enterDungeon, _purchaseDungeon);
			SceneAutoBehaviour<ParttimeDungeonManagement>.instance.OnReadyBattle(_enterDungeon);
		}
		else if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenInventory) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			SceneAutoBehaviour<InventoryManagement>.instance.OpenInventory();
		}
		else if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenCashShop) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Ruby.ToString());
		}
		else if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenNewbieShop) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Beginner.ToString());
		}
		Vespa_System_Action_Extension_Method.fire(onEnd);
	}
}
