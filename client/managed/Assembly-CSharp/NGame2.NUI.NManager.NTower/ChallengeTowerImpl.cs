using System;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NUI.NManager.NCommonImpl;
using NGame2.NUI.NWindow.NTower;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager.NTower;

public class ChallengeTowerImpl : TowerManagement.IImplement
{
	private readonly TowerManagement _management;

	public TowerData TowerData { get; private set; }

	public ChallengeTowerImpl(TowerManagement management)
	{
		_management = management;
	}

	public void Open(TowerData towerData, TowerInfo towerInfo, int showFloor, bool showClearEffect, bool returnFromBattle, Action onOpen)
	{
		TowerData = towerData;
		_ = towerInfo?.CompletedFloor;
		TowerInfo TowerInfo = GlobalAutoBehaviour<AccountManager>.instance.TowerManager.Get(TowerData.TowerIndex);
		Action<EnterDungeonData> onReadyBattle = delegate
		{
			_management.OpenBattlePartySetting();
		};
		_management.AddTask(_management.TaskFactory.Run((ChallengeTowerWindow window) => window.OpenAsync(immediately: false, delegate
		{
			Vespa_System_Action_Extension_Method.fire(onOpen);
		})), _management.TaskFactory.Run(delegate(ChallengeTowerWindow window)
		{
			BattleDungeonInfoListener listener = new BattleDungeonInfoListener
			{
				BattleDungeonInfo = window,
				OnReadyBattle = onReadyBattle
			};
			window.Init(TowerData, TowerInfo, showFloor, listener);
			window.SetOnChangeFloorCallback(_management.InitFloorData);
		}));
	}
}
