using NShared;

namespace NGame2.NAccount;

public class UserEventCraftManager : NShared.UserEventCraftManager
{
	public void Apply(EventCraftItemInfo info)
	{
		Add(info);
	}

	public void Apply(EventCraftItemInfo[] infos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			Apply(infos[i]);
		}
	}
}
