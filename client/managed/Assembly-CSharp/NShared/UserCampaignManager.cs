using System.Collections.Generic;

namespace NShared;

public class UserCampaignManager : AccountAttrManager<ChapterDungeonKey, ChapterDungeonInfo>
{
	public ChapterDungeonInfo Get(int chapterIndex, int dungeonIndex)
	{
		return Get(new ChapterDungeonKey(chapterIndex, dungeonIndex));
	}

	public bool ChapterCompleted(int chapterIndex)
	{
		if (GameRoot.Instance == null)
		{
			return false;
		}
		if (GameRoot.Instance.CampaignChapterDataContainer == null)
		{
			return false;
		}
		if (GameRoot.Instance.CampaignChapterDataContainer.GetData(chapterIndex) == null)
		{
			return false;
		}
		IList<CampaignDungeonData> allDataOfBattleType = GameRoot.Instance.CampaignDungeonDataContainer.GetAllDataOfBattleType(BattleType.Boss);
		for (int i = 0; i < allDataOfBattleType.Count; i++)
		{
			if (allDataOfBattleType[i].ChapterIndex == chapterIndex && Get(allDataOfBattleType[i].ChapterIndex, allDataOfBattleType[i].DungeonIndex) == null)
			{
				return false;
			}
		}
		return true;
	}

	public int GetMaxProgressChapter()
	{
		int i;
		for (i = 1; ChapterCompleted(i); i++)
		{
		}
		return i;
	}
}
