using System.Collections.Generic;
using NShared.NJit;

namespace NShared;

public class ChapterClearRewardDataContainer : JitDataContainerDefault<ChapterClearRewardKey, ChapterClearRewardData>
{
	public const string TABLENAME = "ChapterClearRewardTable";

	public override TablePathDefine PathDefine => new TablePathDefine("ChapterClearRewardTable");

	public int GetRewardReqStar(int chapterIndex, int stepIndex)
	{
		return GetData(new ChapterClearRewardKey(chapterIndex, stepIndex))?.RewardReqStar ?? 0;
	}

	public List<ChapterClearRewardData> GetRewardDataByChapterIndex(int chaterIndex)
	{
		List<ChapterClearRewardData> list = new List<ChapterClearRewardData>();
		list.Clear();
		IList<ChapterClearRewardData> allData = GetAllData();
		ChapterClearRewardData chapterClearRewardData = null;
		for (int i = 0; i < allData.Count; i++)
		{
			chapterClearRewardData = allData[i];
			if (chapterClearRewardData != null && chapterClearRewardData.ChapterIndex == chaterIndex)
			{
				list.Add(chapterClearRewardData);
			}
		}
		return list;
	}
}
