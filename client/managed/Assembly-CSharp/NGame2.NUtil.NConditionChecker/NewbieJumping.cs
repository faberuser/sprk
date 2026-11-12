using NGame2.NAccount;
using NGame2.NUI.NManager.Mission;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUtil.NConditionChecker;

public class NewbieJumping : ConditionChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		if (__Extension_Method_ICollection__.COUNT(values) < 3)
		{
			return false;
		}
		int valueInt = ArgParser.GetValueInt(values, 1);
		MissionCategoryData missionCategoryDataHead = GameRoot.Instance.MissionCategoryDataContainer.GetMissionCategoryDataHead(valueInt);
		if (missionCategoryDataHead == null)
		{
			return false;
		}
		bool valueBool = ArgParser.GetValueBool(values, 2);
		bool flag = MissionManagement.IsOpenSubCategory(missionCategoryDataHead);
		if (valueBool != flag)
		{
			return false;
		}
		for (int i = 3; i + 2 < __Extension_Method_ICollection__.COUNT(values); i += 3)
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
				bool flag2 = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(result, originalDungeonIndex);
				if (result3 != flag2)
				{
					return false;
				}
			}
		}
		return true;
	}
}
