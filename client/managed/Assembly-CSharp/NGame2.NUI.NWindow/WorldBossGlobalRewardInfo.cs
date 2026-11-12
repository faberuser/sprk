using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NContentsInfo;
using NGame2.NScene.NPreloader;
using NGame2.NUI.NComponent2;
using NShared;
using NShared.NWorldBoss;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class WorldBossGlobalRewardInfo : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/WorldBoss/WorldBossGlobalRewardInfo";

	public UIScrollGrid RewardGrid;

	public UILabel Label_Desc;

	private List<WorldBossRankingRewardComponent> _componentList;

	private WorldBossInfo _worldBossInfo;

	private DateTime _endLocalTime;

	private string _endDayOfWeek;

	public override void Open()
	{
		_worldBossInfo = SingletonUpdater<WorldBoss>.instance.WorldBossInfo;
		Open(Init, null);
	}

	public void Init()
	{
		GetEndLocalTime();
		ShowGlobalReward();
	}

	public override void ReserveCache(UiPreloader preloader)
	{
		base.ReserveCache(preloader);
	}

	public override void Reset()
	{
		base.Reset();
		ClearRewardList();
	}

	public void OnClickClose()
	{
		Close();
	}

	private void InitReward(IList<WorldBossRewardData> datas)
	{
		ClearRewardList();
		if (_componentList == null)
		{
			_componentList = new List<WorldBossRankingRewardComponent>();
		}
		foreach (WorldBossRewardData data in datas)
		{
			WorldBossRankingRewardComponent worldBossRankingRewardComponent = UIHelper.AddGridItem(RewardGrid, WorldBossRankingRewardComponent.GlobalPrefabAuto);
			worldBossRankingRewardComponent.SetData(data);
			_componentList.Add(worldBossRankingRewardComponent);
		}
		UIHelper.RepositionGrid(RewardGrid);
	}

	private void ClearRewardList()
	{
		FactoryBase.ReleaseAndClearList(_componentList);
	}

	private void ShowGlobalReward()
	{
		InitReward(GameRoot.Instance.WorldBossGlobalRewardDataContainer.GetAllData());
		string stringDirect = LocalizationManager.GetStringDirect("GLOBAL_WORLDBOSS_WORLD_RANKING_DESC", _endDayOfWeek, _endLocalTime.ToString("HH"));
		SetRewardDesc(stringDirect);
	}

	private void SetRewardDesc(string msg)
	{
		UIHelper.SetLabelText(Label_Desc, msg);
	}

	private void GetEndLocalTime()
	{
		int num = 0;
		WorldBossData data = GameRoot.Instance.WorldBossDataContainer.GetData(_worldBossInfo.Index);
		if (data != null)
		{
			num = data.DealBattleDuration;
		}
		DateTime time = Convert.ToDateTime(_worldBossInfo.CreatedTime);
		_endLocalTime = GlobalAutoBehaviour<AccountManager>.instance.ServerUTCToLocal(time);
		_endLocalTime = _endLocalTime.AddSeconds(num);
		Enum dayOfWeekKey = NGame2.NUI.Common.GetDayOfWeekKey(_endLocalTime.DayOfWeek);
		_endDayOfWeek = LocalizationManager.GetString(dayOfWeekKey);
	}
}
