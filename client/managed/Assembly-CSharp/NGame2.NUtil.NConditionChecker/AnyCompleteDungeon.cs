using NGame2.NAccount;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUtil.NConditionChecker;

public class AnyCompleteDungeon : ConditionChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		if (__Extension_Method_ICollection__.COUNT(values) < 3)
		{
			return false;
		}
		for (int i = 1; i + 2 < __Extension_Method_ICollection__.COUNT(values); i += 2)
		{
			if (!int.TryParse(values[i], out var result) || !int.TryParse(values[i + 1], out var result2))
			{
				return false;
			}
			int originalDungeonIndex = GameRoot.Instance.CampaignDungeonDataContainer.GetOriginalDungeonIndex(result2);
			if (GameRoot.Instance.CampaignDungeonDataContainer.GetData(result, originalDungeonIndex) == null)
			{
				return false;
			}
			if (GlobalAutoBehaviour<AccountManager>.instance.CampaignManager != null && GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(result, originalDungeonIndex))
			{
				return true;
			}
		}
		return false;
	}
}
