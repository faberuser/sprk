using NGame2.NGuild;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NAccount.NCampaign;

public class GuildRaidDungeonCompleteChecker : IDungeonCompleteChecker
{
	bool IDungeonCompleteChecker.IsCompleted(int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty)
	{
		if (GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex) == null)
		{
			return false;
		}
		GuildRaidDungeonInfo guildRaidDungeonInfo = (GlobalAutoBehaviour<GuildManager>.isValidInstance ? GlobalAutoBehaviour<GuildManager>.instance.GetGuildRaidDungeonInfo(chapterIndex) : null);
		if (guildRaidDungeonInfo == null)
		{
			return false;
		}
		if (dungeonIndex >= guildRaidDungeonInfo.DungeonIndex)
		{
			return false;
		}
		return true;
	}
}
