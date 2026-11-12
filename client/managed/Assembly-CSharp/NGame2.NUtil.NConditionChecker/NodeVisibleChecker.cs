using NGame2.NAccount;
using NGame2.NUI.NManager;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUtil.NConditionChecker;

public class NodeVisibleChecker : ConditionChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		if (!int.TryParse(values[1], out var result) || !int.TryParse(values[2], out var result2) || !int.TryParse(values[3], out var result3) || !bool.TryParse(values[4], out var result4))
		{
			return false;
		}
		if (GameRoot.Instance.CampaignDungeonDataContainer.GetData(result2, result3) == null)
		{
			return false;
		}
		NGame2.NAccount.UserCampaignManager campaignManager = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager;
		if (campaignManager == null)
		{
			return false;
		}
		if (result != 0)
		{
			bool num = campaignManager.IsCompleted(result2, result);
			bool flag = GlobalAutoBehaviour<CampaignNodeOpenEffectManager>.instance.IsNewClearNode(result2, result);
			if (!num && !flag)
			{
				return false;
			}
		}
		bool flag2 = campaignManager.IsCompleted(result2, result3);
		return result4 == flag2;
	}
}
