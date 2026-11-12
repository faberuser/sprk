using NShared;

namespace NGame2.NAccount;

public class UserKingsPassManager : NShared.UserKingsPassManager
{
	public KingsPassSeaonInfo KingsPassSeaonInfo { get; private set; }

	public PlayerKingsPassExp PlayerKingsPassExp { get; private set; }

	public void Apply(KingsPassMission info)
	{
		Add(info);
	}

	public void Apply(KingsPassMission[] infos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			Apply(infos[i]);
		}
	}

	public void Set(KingsPassSeaonInfo info)
	{
		if (info != null && info != KingsPassSeaonInfo)
		{
			KingsPassSeaonInfo = info;
		}
	}

	public void Set(PlayerKingsPassExp info)
	{
		if (info != null && info != PlayerKingsPassExp)
		{
			PlayerKingsPassExp = info;
		}
	}
}
