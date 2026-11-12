using System;
using NGame2.NBattleContext;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NScene.NLobby;

public class TowerReturnState : BaseEntryMenuReturnState
{
	private int _towerIndex;

	private int _towerFloor;

	private TowerType _towerType;

	public TowerReturnState(int toweIndex, int towerFloor, TowerType towerType)
	{
		_towerIndex = toweIndex;
		_towerFloor = towerFloor;
		_towerType = towerType;
	}

	public override void Execute(LobbyScene2.ReturnStateInfo info, Action onEnd)
	{
		if (_towerType == TowerType.Maze)
		{
			SceneAutoBehaviour<TowerManagement>.instance.OpenMazeTowerList();
		}
		bool lastBattleWin = SingletonAuto<BattleContext>.instance.LastBattleWin;
		bool flag = false;
		if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenHeroDeck) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			SceneAutoBehaviour<MyHeroManagement>.instance.OpenMyDeck();
			Vespa_System_Action_Extension_Method.fire(onEnd);
		}
		else if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenNextDungeon) != LobbyScene2.ReturnStateOpenFlag.None)
		{
			flag = true;
			SceneAutoBehaviour<TowerManagement>.instance.OpenTower(_towerIndex, _towerFloor + 1, lastBattleWin, returnFromBattle: true, delegate
			{
				SceneAutoBehaviour<TowerManagement>.instance.OpenBattlePartySetting();
				Vespa_System_Action_Extension_Method.fire(onEnd);
			});
		}
		else if (info.OpenFlag == LobbyScene2.ReturnStateOpenFlag.None)
		{
			int showFloor = 0;
			if (_towerType == TowerType.GatewaytoTrial)
			{
				showFloor = _towerFloor + 1;
			}
			SceneAutoBehaviour<TowerManagement>.instance.OpenTower(_towerIndex, showFloor, lastBattleWin, returnFromBattle: true, delegate
			{
				Vespa_System_Action_Extension_Method.fire(onEnd);
			});
		}
		else
		{
			SceneAutoBehaviour<TowerManagement>.instance.OpenTower(_towerIndex, _towerFloor, lastBattleWin, returnFromBattle: true, delegate
			{
				if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenPartySetting) != LobbyScene2.ReturnStateOpenFlag.None)
				{
					SceneAutoBehaviour<TowerManagement>.instance.OpenBattlePartySetting();
				}
				else if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenInventory) != LobbyScene2.ReturnStateOpenFlag.None)
				{
					SceneAutoBehaviour<InventoryManagement>.instance.OpenInventory();
				}
				else if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenNewbieShop) != LobbyScene2.ReturnStateOpenFlag.None)
				{
					SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Beginner.ToString());
				}
				else if ((info.OpenFlag & LobbyScene2.ReturnStateOpenFlag.OpenEventShop) != LobbyScene2.ReturnStateOpenFlag.None)
				{
					SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Event.ToString());
				}
				Vespa_System_Action_Extension_Method.fire(onEnd);
			});
		}
		if (!flag && AutoNextBattle.IsEnabled())
		{
			AutoNextBattle.instance.OnClickStop();
		}
	}
}
