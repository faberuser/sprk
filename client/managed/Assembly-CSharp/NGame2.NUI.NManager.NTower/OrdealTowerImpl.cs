using System;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NUI.NManager.NCommonImpl;
using NGame2.NUI.NWindow.NTower;
using NShared;
using NVespa.NSingleton;
using NVespa.NTask;
using NVespa.NTask.NRuntime;

namespace NGame2.NUI.NManager.NTower;

public class OrdealTowerImpl : TowerManagement.IImplement
{
	private readonly TowerManagement _management;

	private int ShowFloor;

	public TowerData TowerData { get; private set; }

	public OrdealTowerImpl(TowerManagement management)
	{
		_management = management;
	}

	public void Open(TowerData towerData, TowerInfo towerInfo, int showFloor, bool showClearEffect, bool returnFromBattle, Action onOpen)
	{
		TowerData = towerData;
		ShowFloor = showFloor;
		if (returnFromBattle)
		{
			GlobalAutoBehaviour<AccountManager>.instance.TowerManager.Get(TowerData.TowerIndex);
			if (_management.NpcCache != null)
			{
				_ = _management.NpcCache.CachedNpcInfos;
			}
			_management.AddTask(CreateOpenOrdealTowerWindow(onOpen));
		}
		else
		{
			OpenOrdealTowerWindow(onOpen);
		}
	}

	public void OpenOrdealTowerWindow(Action onOpen)
	{
		_management.AddTask(CreateOpenOrdealTowerWindow(onOpen));
	}

	private ITask CreateOpenOrdealTowerWindow(Action onOpen)
	{
		TowerInfo TowerInfo = GlobalAutoBehaviour<AccountManager>.instance.TowerManager.Get(TowerData.TowerIndex);
		CachedAccountInfo[] npcInfos = ((_management.NpcCache != null) ? _management.NpcCache.CachedNpcInfos : null);
		Action<EnterDungeonData> onReadyBattle = delegate
		{
			_management.OpenBattlePartySetting();
		};
		GroupTask groupTask = new GroupTask();
		groupTask.AddTask(_management.TaskFactory.Run((OrdealTowerWindow window) => window.OpenAsync(immediately: false, delegate
		{
			Vespa_System_Action_Extension_Method.fire(onOpen);
		})));
		groupTask.AddTask(_management.TaskFactory.Run(delegate(OrdealTowerWindow window)
		{
			BattleDungeonInfoListener listener = new BattleDungeonInfoListener
			{
				BattleDungeonInfo = window,
				OnReadyBattle = onReadyBattle
			};
			window.Init(TowerData, TowerInfo, npcInfos, ShowFloor, listener);
			window.SetOnChangeFloorCallback(_management.InitFloorData);
		}));
		return groupTask;
	}
}
