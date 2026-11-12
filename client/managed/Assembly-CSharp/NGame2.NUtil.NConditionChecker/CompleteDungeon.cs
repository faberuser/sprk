using NGame2.NAccount;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUtil.NConditionChecker;

public class CompleteDungeon : ConditionChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		if (__Extension_Method_ICollection__.COUNT(values) < 4)
		{
			return false;
		}
		for (int i = 1; i + 2 < __Extension_Method_ICollection__.COUNT(values); i += 3)
		{
			if (!int.TryParse(values[i], out var result) || !int.TryParse(values[i + 1], out var result2) || !bool.TryParse(values[i + 2], out var result3))
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
				bool flag = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(result, originalDungeonIndex);
				if (result3 != flag)
				{
					return false;
				}
			}
		}
		return true;
	}
}
