using System.Collections;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NShared;
using NVespa.NSingleton;

namespace NGame2.NLobby.NActivity;

public class OpenChallengeRaid : IActivity
{
	public static IActivity Create(string[] values)
	{
		return new OpenChallengeRaid();
	}

	public OpenChallengeRaid()
	{
		ChallengeRaidWrapper challengeRaidWarpper = SceneAutoBehaviour<RaidManagement>.instance.GetChallengeRaidWarpper();
		if (challengeRaidWarpper != null)
		{
			bool flag = challengeRaidWarpper.IsAbleEnter();
			bool flag2 = challengeRaidWarpper.IsAbleSeason();
			if (flag || flag2)
			{
				SceneAutoBehaviour<RaidManagement>.instance.OpenChallengeRaid();
			}
			else
			{
				SceneAutoBehaviour<PortalManagement>.instance.OpenPortalRenewalWindow(PortalRenewalCategory.ChallengeRaid);
			}
		}
	}

	IEnumerator IActivity.coStart()
	{
		yield return null;
	}

	void IActivity.OnRequestStop()
	{
	}
}
