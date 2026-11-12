using NGame2.NAccount;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NEntryMenu.NCondition;

public class HasHero : ICondition
{
	bool ICondition.Check(CampaignNodeData nodeData, string[] value)
	{
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return false;
		}
		int valueInt = ArgParser.GetValueInt(value, 1);
		return GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(valueInt) != null;
	}
}
