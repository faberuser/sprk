using NGame2.NGuild;
using NShared;

namespace NGame2.NUI.NWindow.NEntryMenu.NCondition;

public class GuildActivityRestrict : ICondition
{
	bool ICondition.Check(CampaignNodeData nodeData, string[] value)
	{
		return GuildManager.GuildValidGuildPenaltyType() != GuildPenaltyType.None;
	}
}
