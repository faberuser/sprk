using System.Collections.Generic;
using NShared;
using NShared.NLocalization;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class GuildRaidScoreRewardComponent : UIScrollItemBehaviour, InitOnCreate
{
	public const string PrefabPath = "Resources/Prefab2/Component/GuildRaid/GuildRaidScoreRewardComponent";

	public static PrefabAuto<GuildRaidScoreRewardComponent> PrefabAuto = new PrefabAuto<GuildRaidScoreRewardComponent>("Resources/Prefab2/Component/GuildRaid/GuildRaidScoreRewardComponent");

	public UILabel ScoreDescLabel;

	public UIBaseGrid RewardGrid;

	public Transform[] BgRoot;

	private GuildRaidChapterRewardData _rewardData;

	private List<Component> _rewardComponentList = new List<Component>();

	public void Open(GuildRaidChapterRewardData rewardData, int bgIndex)
	{
		_rewardData = rewardData;
		InitDescription();
		InitRewardComponentList();
		InitBg(bgIndex);
	}

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		ClearRewardComponentList();
	}

	private void InitDescription()
	{
		string text = LocalizationManager.GetString(GuildRaid.GUILDRAID_SCORE_REWARD_INFO, _rewardData.TotalDamage);
		UIHelper.SetLabelText(ScoreDescLabel, text);
	}

	private void InitBg(int bgIndex)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(BgRoot); i++)
		{
			UIHelper.EnableGameObject(BgRoot[i], bgIndex % __Extension_Method_ICollection__.COUNT(BgRoot) == i);
		}
	}

	private void InitRewardComponentList()
	{
		ClearRewardComponentList();
		int rewardIndex = _rewardData.RewardIndex;
		_rewardComponentList = UIHelper.CreateRewardItemIconComponentList(rewardIndex, RewardGrid);
		UIHelper.RepositionGrid(RewardGrid);
	}

	private void ClearRewardComponentList()
	{
		FactoryBase.ReleaseAndClearList(_rewardComponentList);
	}
}
