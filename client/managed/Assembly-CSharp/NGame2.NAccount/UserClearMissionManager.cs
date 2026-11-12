using NShared;

namespace NGame2.NAccount;

public class UserClearMissionManager : NShared.UserClearMissionManager
{
	public void Apply(ClearMissionInfo info)
	{
		Add(info);
	}

	public void Apply(ClearMissionInfo[] infos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			Apply(infos[i]);
		}
	}

	public bool IsReceivedReward(int missionIndex)
	{
		ClearMissionInfo clearMissionInfo = Get(missionIndex);
		if (clearMissionInfo == null || (clearMissionInfo != null && clearMissionInfo.LastStep == 0))
		{
			return false;
		}
		return true;
	}
}
