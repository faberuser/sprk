using NShared;

public class UserChapterRewardManager : NShared.UserChapterRewardManager
{
	public void Set(ChapterRewardInfo info)
	{
		Remove(info.ChapterIndex);
		Add(info);
	}

	public bool ChapterClearRewarded(int chapterIndex, int step)
	{
		return Get(chapterIndex)?.Rewarded(step) ?? false;
	}
}
