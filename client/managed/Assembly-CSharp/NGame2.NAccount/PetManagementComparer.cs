using System.Collections.Generic;
using NShared;

namespace NGame2.NAccount;

public class PetManagementComparer : Comparer<PetInfo>
{
	public new static PetManagementComparer Default = new PetManagementComparer();

	public override int Compare(PetInfo l, PetInfo r)
	{
		CreatureData creatureData = l.CreatureData;
		CreatureData creatureData2 = r.CreatureData;
		int num = 0;
		if (creatureData != null && creatureData2 != null)
		{
			num = -l.CreatureData.Grade.CompareTo(r.CreatureData.Grade);
			if (num != 0)
			{
				return num;
			}
		}
		num = -l.Star.CompareTo(r.Star);
		if (num != 0)
		{
			if (l.Star == 5)
			{
				return 1;
			}
			if (r.Star == 5)
			{
				return -1;
			}
			return num;
		}
		return l.PetIndex.CompareTo(r.PetIndex);
	}
}
