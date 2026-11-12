using NGame2.NAccount;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NUtil.NModifyChecker;

public class ClearAchievement : ModifyChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		if (__Extension_Method_ICollection__.COUNT(values) < 2)
		{
			return false;
		}
		if (!int.TryParse(values[1], out var result))
		{
			return false;
		}
		int curAchievementStep = GetCurAchievementStep(result);
		int lastStepOnAchievementData = GetLastStepOnAchievementData(result);
		return curAchievementStep > lastStepOnAchievementData;
	}

	private int GetCurAchievementStep(int achievementIndex)
	{
		AchievementInfo achievementInfo = GlobalAutoBehaviour<AccountManager>.instance.AchievementManager.Get(achievementIndex);
		if (achievementInfo == null)
		{
			return 1;
		}
		return achievementInfo.LastStep + 1;
	}

	private int GetLastStepOnAchievementData(int achievementIndex)
	{
		AchievementData achievementData = null;
		int num = 0;
		do
		{
			achievementData = GameRoot.Instance.AchievementDataContainer.GetData(achievementIndex, num + 1);
			if (achievementData != null)
			{
				num++;
			}
		}
		while (achievementData != null);
		return num;
	}
}
