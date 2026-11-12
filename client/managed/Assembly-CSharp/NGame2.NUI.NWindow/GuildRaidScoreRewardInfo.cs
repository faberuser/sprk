using System.Collections.Generic;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow;

public class GuildRaidScoreRewardInfo : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/GuildRaid/GuildRaidScoreRewardInfo";

	public UIBaseGrid RewardInfoGrid;

	private List<GuildRaidScoreRewardComponent> _rewardInfoComponentList = new List<GuildRaidScoreRewardComponent>();

	public static void Open(int chapterIndex, int difficulty)
	{
		WindowPrefab<GuildRaidScoreRewardInfo> windowPrefab = new WindowPrefab<GuildRaidScoreRewardInfo>("Resources/Prefab2/Window/GuildRaid/GuildRaidScoreRewardInfo", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.OpenInternal(chapterIndex, difficulty);
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	private void OpenInternal(int chapterIndex, int difficulty)
	{
		base.Open(delegate
		{
			Init(chapterIndex, difficulty);
		});
	}

	private void Init(int chapterIndex, int difficulty)
	{
		ClearRewardInfoComponentList();
		if (!RewardInfoGrid)
		{
			return;
		}
		int num = 0;
		foreach (GuildRaidChapterRewardData allDatum in GameRoot.Instance.GuildRaidChapterRewardDataContainer.GetAllData())
		{
			if (allDatum.ChapterIndex == chapterIndex && allDatum.Difficulty == difficulty)
			{
				GuildRaidScoreRewardComponent guildRaidScoreRewardComponent = RewardInfoGrid.AddGridItem(GuildRaidScoreRewardComponent.PrefabAuto);
				guildRaidScoreRewardComponent.Open(allDatum, num++);
				_rewardInfoComponentList.Add(guildRaidScoreRewardComponent);
			}
		}
		RewardInfoGrid.RepositionGrid();
	}

	private void ClearRewardInfoComponentList()
	{
		FactoryBase.ReleaseAndClearList(_rewardInfoComponentList);
	}

	public override void Reset()
	{
		base.Reset();
		ClearRewardInfoComponentList();
	}
}
