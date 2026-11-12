using NGame2.NAccount;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NEntryMenu.NCondition;

public class ClearDungeon : ICondition
{
	bool ICondition.Check(CampaignNodeData nodeData, string[] value)
	{
		int valueInt = ArgParser.GetValueInt(value, 1);
		int valueInt2 = ArgParser.GetValueInt(value, 2);
		DungeonDifficultyType valueEnum = ArgParser.GetValueEnum(value, 3, DungeonDifficultyType.Easy);
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return false;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(valueInt, valueInt2, valueEnum);
	}
}
