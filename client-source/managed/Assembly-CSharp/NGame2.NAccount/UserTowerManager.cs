using NShared;

namespace NGame2.NAccount;

public class UserTowerManager : NShared.UserTowerManager
{
	public void Apply(TowerInfo info)
	{
		Add(info);
	}

	public void Apply(TowerInfo[] infos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			Apply(infos[i]);
		}
	}
}
