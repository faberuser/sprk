using NGame2.NGuild;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NEntryMenu.NCondition;

public class GuildExist : ICondition
{
	bool ICondition.Check(CampaignNodeData nodeData, string[] value)
	{
		if (GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager == null)
		{
			return false;
		}
		bool valueBool = ArgParser.GetValueBool(value, 1);
		return GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GetGuildId() != 0 == valueBool;
	}
}
