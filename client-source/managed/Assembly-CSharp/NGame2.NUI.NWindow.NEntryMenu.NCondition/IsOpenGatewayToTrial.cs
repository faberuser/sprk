using NGame2.NAccount;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUI.NWindow.NEntryMenu.NCondition;

public class IsOpenGatewayToTrial : ICondition
{
	bool ICondition.Check(CampaignNodeData nodeData, string[] value)
	{
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return false;
		}
		if (!HasMaxStarHero())
		{
			return false;
		}
		for (int i = 1; i + 2 < value.Length; i += 3)
		{
			int valueInt = ArgParser.GetValueInt(value, i);
			int valueInt2 = ArgParser.GetValueInt(value, i + 1);
			bool valueBool = ArgParser.GetValueBool(value, i + 2);
			if (GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(valueInt, valueInt2) != valueBool)
			{
				return false;
			}
		}
		return true;
	}

	private bool HasMaxStarHero()
	{
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("HeroMaxStar");
		foreach (CachedHeroInfo item in GlobalAutoBehaviour<AccountManager>.instance.HeroManager)
		{
			if (item.Star >= valueInt)
			{
				return true;
			}
		}
		return false;
	}
}
