using System;
using NGame2.NUI.NComponent2.NTower;
using NGame2.NUI.NWindow.NTower;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;

namespace NGame2.NUI.NManager.NTower;

public class PitImpl : TowerManagement.IImplement, PitWindow.Listener, PitDungeonInfo.Listener
{
	private readonly TowerManagement _management;

	public PitImpl(TowerManagement management)
	{
		_management = management;
	}

	public void Open(TowerData towerData, TowerInfo towerInfo, int showFloor, bool showClearEffect, bool returnFromBattle, Action onOpen)
	{
		ResultCreatureInfo[] heroInfos = ((_management.HeroCache != null) ? _management.HeroCache.CachedHeroInfos : null);
		_management.AddTask(_management.TaskFactory.Run(delegate(PitWindow window)
		{
			window.Init(towerData, towerInfo, heroInfos, showClearEffect, returnFromBattle, onOpen, this);
		}), _management.TaskFactory.Run((PitWindow window) => window.OpenAsync(immediately: false, delegate
		{
			window.OnOpen(showClearEffect);
			Vespa_System_Action_Extension_Method.fire(onOpen);
		})));
	}

	private void OpenPitDungeonInfo()
	{
		TowerData towerData = _management.TowerData;
		TowerFloorData floorData = _management.FloorData;
		_management.AddTask(_management.TaskFactory.Branch(_management.TaskFactory.Check((PitDungeonInfo window) => window.Init(towerData, floorData, 0, this)), _management.TaskFactory.Sequence(_management.TaskFactory.Run((PitDungeonInfo window) => window.SceneChangeTask()), _management.TaskFactory.Run((PitDungeonInfo window) => window.OpenAsync(immediately: false, delegate
		{
			window.onOpened();
		})))));
	}

	void PitWindow.Listener.OnResetPit(PitWindow caller)
	{
		string title = LocalizationManager.GetString(Pit.PIT_RESET_TITLE);
		string message = LocalizationManager.GetString(Pit.PIT_RESET_MESSAGE);
		string action = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_INIT);
		_management.ProcessResetTower(caller, title, message, action);
	}

	void PitWindow.Listener.OnClickStep(PitWindow caller, PitStepComponent component)
	{
		if (component.FloorData.Floor == component.CompletedFloor + 1)
		{
			_management.InitFloorData(component.FloorData);
			OpenPitDungeonInfo();
		}
	}

	int PitWindow.Listener.GetBossHpPercentage(PitWindow caller, TowerFloorData floorData)
	{
		int chapterIndex = floorData.ChapterIndex;
		int dungeonIndex = floorData.DungeonIndex;
		_ = floorData.TowerIndex;
		CreatureData bossCreatureData = GameRoot.Instance.CampaignDungeonDataContainer.GetBossCreatureData(chapterIndex, dungeonIndex, NShared.DungeonDifficultyType.Easy, scenario: false);
		if (bossCreatureData == null)
		{
			return 0;
		}
		if (_management.HeroCache != null)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_management.HeroCache.CachedHeroInfos); i++)
			{
				ResultCreatureInfo resultCreatureInfo = _management.HeroCache.CachedHeroInfos[i];
				if (resultCreatureInfo.TeamId == 1 && resultCreatureInfo.Index == bossCreatureData.Index)
				{
					return (int)((resultCreatureInfo.MaxHp > 0) ? (resultCreatureInfo.Hp * 100 / resultCreatureInfo.MaxHp) : 0);
				}
			}
		}
		return 100;
	}

	void PitDungeonInfo.Listener.OnClickBattleReady()
	{
		_management.OpenBattlePartySetting();
	}
}
