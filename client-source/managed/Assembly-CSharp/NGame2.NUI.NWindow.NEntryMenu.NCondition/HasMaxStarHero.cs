using NGame2.NAccount;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NEntryMenu.NCondition;

public class HasMaxStarHero : ICondition
{
	bool ICondition.Check(CampaignNodeData nodeData, string[] value)
	{
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return false;
		}
		bool flag = false;
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("HeroMaxStar");
		foreach (CachedHeroInfo item in GlobalAutoBehaviour<AccountManager>.instance.HeroManager)
		{
			if (item.Star >= valueInt)
			{
				flag = true;
				break;
			}
		}
		bool valueBool = ArgParser.GetValueBool(value, 1);
		return flag == valueBool;
	}
}
