using NShared;

namespace NGame2.NAccount;

public class UserPetIncubatorSlotManager : NShared.UserPetIncubatorSlotManager
{
	public void Apply(PetIncubatorSlotInfo info)
	{
		Add(info);
	}

	public void Apply(PetIncubatorSlotInfo[] infos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			Apply(infos[i]);
		}
	}
}
