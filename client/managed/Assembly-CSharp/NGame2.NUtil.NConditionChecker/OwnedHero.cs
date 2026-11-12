using NGame2.NAccount;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUtil.NConditionChecker;

public class OwnedHero : ConditionChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		if (__Extension_Method_ICollection__.COUNT(values) < 6)
		{
			return false;
		}
		int valueInt = ArgParser.GetValueInt(values, 1, -1);
		bool valueBool = ArgParser.GetValueBool(values, 2);
		if (valueInt < 0)
		{
			return false;
		}
		bool flag = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(valueInt) != null;
		if (valueBool != flag)
		{
			return false;
		}
		int result = 0;
		int result2 = 0;
		bool result3 = false;
		if (!int.TryParse(values[3], out result) || !int.TryParse(values[4], out result2) || !bool.TryParse(values[5], out result3))
		{
			return false;
		}
		int originalDungeonIndex = GameRoot.Instance.CampaignDungeonDataContainer.GetOriginalDungeonIndex(result2);
		if (GameRoot.Instance.CampaignDungeonDataContainer.GetData(result, originalDungeonIndex) == null)
		{
			return false;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.CampaignManager != null)
		{
			bool flag2 = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(result, originalDungeonIndex);
			if (result3 != flag2)
			{
				return false;
			}
		}
		return true;
	}
}
