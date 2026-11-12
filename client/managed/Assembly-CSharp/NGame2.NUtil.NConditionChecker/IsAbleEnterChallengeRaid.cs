using NGame2.NUI.NManager.NLobby;
using NVespa.NSingleton;

namespace NGame2.NUtil.NConditionChecker;

public class IsAbleEnterChallengeRaid : ConditionChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		if (__Extension_Method_ICollection__.COUNT(values) > 1)
		{
			return false;
		}
		ChallengeRaidWrapper challengeRaidWarpper = SceneAutoBehaviour<RaidManagement>.instance.GetChallengeRaidWarpper();
		if (challengeRaidWarpper == null)
		{
			return false;
		}
		bool flag = challengeRaidWarpper.IsAbleEnter();
		bool result = challengeRaidWarpper.IsAbleSeason();
		if (!flag)
		{
			return result;
		}
		return true;
	}
}
