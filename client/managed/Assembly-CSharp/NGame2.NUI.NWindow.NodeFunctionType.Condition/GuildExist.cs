using NGame2.NAccount;
using NGame2.NGuild;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NodeFunctionType.Condition;

public class GuildExist : ICondition
{
	NShared.NodeFunctionType ICondition.GetFunctionType(string[] values, NShared.NodeFunctionType originType)
	{
		NShared.NodeFunctionType valueEnum = ArgParser.GetValueEnum(values, 1, originType);
		NShared.NodeFunctionType valueEnum2 = ArgParser.GetValueEnum(values, 2, originType);
		if (GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager == null)
		{
			return valueEnum2;
		}
		if (GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GetGuildId() == 0)
		{
			return valueEnum2;
		}
		return valueEnum;
	}

	bool ICondition.EnterConditionCheck(string[] values)
	{
		return GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GetGuildId() == 0;
	}

	bool ICondition.EnterConditionCheck(string[] values, out int chpaterIndex, out int dungeonIndex)
	{
		chpaterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		dungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
		bool num = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GetGuildId() != 0;
		if (!num)
		{
			chpaterIndex = ArgParser.GetValueInt(values, 0);
			dungeonIndex = ArgParser.GetValueInt(values, 1);
		}
		return num;
	}
}
