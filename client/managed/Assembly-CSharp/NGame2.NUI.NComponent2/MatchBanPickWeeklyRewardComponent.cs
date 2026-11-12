using System.Collections.Generic;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class MatchBanPickWeeklyRewardComponent : LinkFactoryComponent, InitOnCreate
{
	public const string PrefabPath = "Resources/Prefab2/Component/Match/MatchBanPickWeeklyRewardComponent";

	public static PrefabAuto<MatchBanPickWeeklyRewardComponent> PrefabAuto = new PrefabAuto<MatchBanPickWeeklyRewardComponent>("Resources/Prefab2/Component/Match/MatchBanPickWeeklyRewardComponent");

	public UILabel RankLabel;

	public UISimpleGrid IconPostion;

	private List<Component> IconComponents;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		FactoryBase.ReleaseAndClearList(IconComponents);
	}

	public void Init(MatchBanPickSeasonRewardData data)
	{
		int minRank = data.MinRank;
		int maxRank = data.MaxRank;
		int percentage = data.Percentage;
		_ = data.RewardIndex;
		if (minRank != 0)
		{
			SetRankData(minRank, maxRank);
		}
		else if (percentage != 0)
		{
			SetPercentageData(percentage);
		}
		if (data.RewardIndex > 0)
		{
			IconComponents = UIHelper.CreateRewardItemIconComponentList(data.RewardIndex, IconPostion, includeCurrency: true);
		}
		RewardData data2 = GameRoot.Instance.RewardDataContainer.GetData(data.RewardIndex);
		if (data2 != null && GameRoot.Instance.ItemDataContainer.GetDataByCode(data2.Item1Code) != null)
		{
			UIHelper.RepositionGrid(IconPostion);
		}
	}

	public void SetRankData(int minRank, int maxRank)
	{
		string text = "";
		UIHelper.SetLabelText(text: (minRank != maxRank) ? LocalizationManager.GetStringDirect("WORLDBOSS_RANK_COUNT", minRank, maxRank) : LocalizationManager.GetStringDirect("GUILD_RANKCOUNT", minRank), label: RankLabel);
	}

	public void SetPercentageData(int percentage)
	{
		string stringDirect = LocalizationManager.GetStringDirect("WORLDBOSS_RANK_COUNT_PERCENT", percentage);
		UIHelper.SetLabelText(RankLabel, stringDirect);
	}
}
