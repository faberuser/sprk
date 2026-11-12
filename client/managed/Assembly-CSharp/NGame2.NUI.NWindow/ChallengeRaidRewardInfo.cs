using System.Collections.Generic;
using NGame2.NScene.NPreloader;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager.NLobby;
using NShared;
using NShared.NRaid;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class ChallengeRaidRewardInfo : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/Raid/ChallengeRaidRewardInfo";

	public UIScrollGrid RewardGrid;

	public Transform DailyRewardActiveRoot;

	public Transform SeasonRewardActiveRoot;

	private List<ChallengeRaidRankingRewardComponent> _componentList;

	public new void Open()
	{
		Open(Init, null);
	}

	public void Init()
	{
		ShowRankReward();
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

	public void ShowRankReward()
	{
		UIHelper.EnableGameObject(DailyRewardActiveRoot, enable: true);
		UIHelper.EnableGameObject(SeasonRewardActiveRoot, enable: false);
		ChallengeRaidInfo challengeRaidSeasonInfo = SceneAutoBehaviour<RaidManagement>.instance.GetChallengeRaidSeasonInfo();
		if (challengeRaidSeasonInfo != null)
		{
			InitReward(GameRoot.Instance.ChallengeRaidRewardDataContainer.GetChallengeRaidRewards(challengeRaidSeasonInfo.ChallengeRaidIndex).ToArray());
		}
	}

	private void InitReward(ChallengeRaidRewardData[] datas)
	{
		ClearRewardList();
		if (_componentList == null)
		{
			_componentList = new List<ChallengeRaidRankingRewardComponent>();
		}
		foreach (ChallengeRaidRewardData challengeRaidRewardData in datas)
		{
			if (challengeRaidRewardData.MinRank != 0 || challengeRaidRewardData.MaxRank != 0)
			{
				ChallengeRaidRankingRewardComponent challengeRaidRankingRewardComponent = UIHelper.AddGridItem(RewardGrid, ChallengeRaidRankingRewardComponent.PrefabAuto);
				challengeRaidRankingRewardComponent.SetData(challengeRaidRewardData);
				_componentList.Add(challengeRaidRankingRewardComponent);
			}
		}
		UIHelper.RepositionGrid(RewardGrid);
	}

	private void ClearRewardList()
	{
		FactoryBase.ReleaseAndClearList(_componentList);
	}
}
