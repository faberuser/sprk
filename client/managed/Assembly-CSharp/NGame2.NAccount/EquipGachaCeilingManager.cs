using NShared;

namespace NGame2.NAccount;

public class EquipGachaCeilingManager : NShared.EquipGachaCeilingManager
{
	public void Apply(GachaCeilingCountInfo info)
	{
		Add(info);
	}

	public void Apply(GachaCeilingCountInfo[] infos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			Apply(infos[i]);
		}
	}

	public int GetGachaCount(int gachaIndex)
	{
		GachaCeilingCountInfo gachaCeilingCountInfo = Get(gachaIndex);
		if (gachaCeilingCountInfo == null)
		{
			return 0;
		}
		return gachaCeilingCountInfo.GachaCount % 20;
	}
}
