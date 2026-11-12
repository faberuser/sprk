using System.Collections.Generic;

namespace NShared;

[JsonSerializable(Javascript = true)]
public class ChapterRewardInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => ChapterIndex;

	public int ChapterIndex { get; set; }

	public byte LastRewardDiff { get; set; }

	public bool AllRewarded()
	{
		List<ChapterClearRewardData> rewardDataByChapterIndex = GameRoot.Instance.ChapterClearRewardDataContainer.GetRewardDataByChapterIndex(ChapterIndex);
		ChapterClearRewardData chapterClearRewardData = null;
		for (int i = 0; i < rewardDataByChapterIndex.Count; i++)
		{
			chapterClearRewardData = rewardDataByChapterIndex[i];
			if (chapterClearRewardData == null)
			{
				return false;
			}
			if (!Rewarded(chapterClearRewardData.StepIndex))
			{
				return false;
			}
		}
		return true;
	}

	public bool Rewarded(int step)
	{
		return (LastRewardDiff & (1 << step)) != 0;
	}
}
