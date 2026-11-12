using NShared;

namespace NGame2.NAccount;

public class UserPetGachaCountManager : NShared.UserPetGachaCountManager
{
	public void Apply(PetGachaCountInfo info)
	{
		Add(info);
	}

	public void Apply(PetGachaCountInfo[] infos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			Apply(infos[i]);
		}
	}

	public int GetMileage(int gachaIndex)
	{
		return Get(gachaIndex)?.GachaCount ?? 0;
	}
}
