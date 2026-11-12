using NShared;

namespace NGame2.NAccount;

public class UserNewbieMissionManager : NShared.UserNewbieMissionManager
{
	public void Apply(NewbieMissionInfo info)
	{
		Add(info);
	}

	public void Apply(NewbieMissionInfo[] infos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			Apply(infos[i]);
		}
	}

	public bool IsReceivedReward(int missionIndex)
	{
		if (Get(missionIndex) == null)
		{
			return false;
		}
		return true;
	}
}
