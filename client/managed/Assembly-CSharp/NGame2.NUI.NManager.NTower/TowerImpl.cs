using System;
using NGame2.NBattleContext;
using NGame2.NUI.NComponent2.NTower;
using NGame2.NUI.NManager.NCommonImpl;
using NGame2.NUI.NWindow.NTower;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;

namespace NGame2.NUI.NManager.NTower;

public class TowerImpl : TowerManagement.IImplement, TowerWindow.Listener
{
	private readonly TowerManagement _management;

	public TowerImpl(TowerManagement management)
	{
		_management = management;
	}

	public void Open(TowerData towerData, TowerInfo towerInfo, int showFloor, bool showClearEffect, bool returnFromBattle, Action onOpen)
	{
		CachedAccountInfo[] npcInfos = ((_management.NpcCache != null) ? _management.NpcCache.CachedNpcInfos : null);
		_management.AddTask(_management.TaskFactory.Run((TowerWindow window) => window.OpenAsync(immediately: false, delegate
		{
			Vespa_System_Action_Extension_Method.fire(onOpen);
		})), _management.TaskFactory.Run(delegate(TowerWindow window)
		{
			window.Init(towerData, towerInfo, npcInfos, showFloor, showClearEffect, returnFromBattle, onOpen, this);
		}));
	}

	private void OpenTowerDungeonInfo(TowerFloorComponent component)
	{
		if (component.FloorData.Floor == component.CompletedFloor + 1)
		{
			_management.InitFloorData(component.FloorData);
			int floorTypeIndex = component.FloorTypeIndex;
			CachedAccountInfo npcInfo = component.NpcInfo;
			int chapterIndex = _management.ChapterIndex;
			int dungeonIndex = _management.DungeonIndex;
			Action<EnterDungeonData> onReadyBattle = delegate
			{
				_management.OpenBattlePartySetting();
			};
			_management.AddTask(_management.TaskFactory.Open(delegate(TowerDungeonInfo window)
			{
				EnterDungeonData enterDungeon = new EnterDungeonData(chapterIndex, dungeonIndex, dungeonIndex, NShared.DungeonDifficultyType.Easy);
				BattleDungeonInfoListener listener = new BattleDungeonInfoListener
				{
					EnterDungeon = enterDungeon,
					BattleDungeonInfo = window,
					OnReadyBattle = onReadyBattle
				};
				window.Init(enterDungeon, npcInfo, floorTypeIndex, listener);
			}), _management.TaskFactory.Run(delegate(TowerWindow window)
			{
				window.PlayEnterFloorEffect(component);
			}));
		}
	}

	void TowerWindow.Listener.OnResetTower(TowerWindow caller)
	{
		string title = LocalizationManager.GetString(Tower.TOWER_RESET_TITLE);
		string message = LocalizationManager.GetString(Tower.TOWER_RESET_MESSAGE);
		string action = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_INIT);
		_management.ProcessResetTower(caller, title, message, action);
	}

	void TowerWindow.Listener.OnClickFloor(TowerWindow caller, TowerFloorComponent component)
	{
		OpenTowerDungeonInfo(component);
	}
}
