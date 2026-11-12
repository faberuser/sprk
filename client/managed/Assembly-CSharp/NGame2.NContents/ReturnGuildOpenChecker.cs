using NGame2.NAccount;
using NShared;

namespace NGame2.NContents;

public class ReturnGuildOpenChecker : IMenuOpenChecker
{
	bool IMenuOpenChecker.IsOpen()
	{
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		return GameRoot.Instance.GuildRaidChapterDataContainer.IsGuildRaidChpater(chapterIndex);
	}
}
