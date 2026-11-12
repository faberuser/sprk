using System;
using NGame2.NAccount;
using NShared;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

[Serializable]
public class EventDugeonEnterCount : BaseEnterCount
{
	public Transform RootTransform;

	public void Init(int chapterIndex, int[] dungeonIndices)
	{
		bool flag = CheckEnable(chapterIndex, dungeonIndices);
		UIHelper.EnableGameObject(RootTransform, flag);
		if (flag)
		{
			SetEnterCount(chapterIndex);
		}
	}

	private bool CheckEnable(int chapterIndex, int[] dungeonIndices)
	{
		if (dungeonIndices == null || __Extension_Method_ICollection__.COUNT(dungeonIndices) <= 0)
		{
			return false;
		}
		return GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndices[0])?.IsExceededEntryChapter ?? false;
	}

	private void SetEnterCount(int chapterIndex)
	{
		CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(chapterIndex);
		if (data != null)
		{
			int dailyEnterMaxCount = data.DailyEnterMaxCount;
			int dailyCompletedCountByChapter = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.GetDailyCompletedCountByChapter(chapterIndex);
			string dayTextKey = ((data.Type == ChapterType.MemoryFrag) ? "MEMORY_DUNGEON_LIMITENTRY" : "DUNGEON_DAILY_LIMIT");
			SetResetCountLabel(dailyEnterMaxCount, dayTextKey);
			SetEnterCountLabel(dailyEnterMaxCount, dailyCompletedCountByChapter);
		}
	}
}
