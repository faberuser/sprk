using NGame2.NAccount;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NodeFunctionType.Condition;

public class ClearDungeon : ICondition
{
	NShared.NodeFunctionType ICondition.GetFunctionType(string[] values, NShared.NodeFunctionType originType)
	{
		return NShared.NodeFunctionType.None;
	}

	bool ICondition.EnterConditionCheck(string[] values)
	{
		int valueInt = ArgParser.GetValueInt(values, 1);
		int valueInt2 = ArgParser.GetValueInt(values, 2);
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return false;
		}
		return !GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(valueInt, valueInt2);
	}

	bool ICondition.EnterConditionCheck(string[] values, out int chpaterIndex, out int dungeonIndex)
	{
		int valueInt = ArgParser.GetValueInt(values, 1);
		int valueInt2 = ArgParser.GetValueInt(values, 2);
		chpaterIndex = valueInt;
		dungeonIndex = valueInt2;
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return false;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(valueInt, valueInt2);
	}
}
