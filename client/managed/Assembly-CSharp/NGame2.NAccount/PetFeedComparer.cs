using System;
using System.Collections.Generic;
using NGame2.NUI.NHelper.NPet;
using NShared;

namespace NGame2.NAccount;

public class PetFeedComparer : Comparer<PetInfo>
{
	public new static PetFeedComparer Default = new PetFeedComparer();

	public override int Compare(PetInfo l, PetInfo r)
	{
		TimeSpan? petGetUpRemainTime = PetHelper.GetPetGetUpRemainTime(l);
		TimeSpan? petGetUpRemainTime2 = PetHelper.GetPetGetUpRemainTime(r);
		int num = 1;
		if (petGetUpRemainTime.HasValue && petGetUpRemainTime.Value.TotalSeconds > 0.0)
		{
			num = -1;
		}
		int value = 1;
		if (petGetUpRemainTime2.HasValue && petGetUpRemainTime2.Value.TotalSeconds > 0.0)
		{
			value = -1;
		}
		int num2 = -num.CompareTo(value);
		if (num2 != 0)
		{
			return num2;
		}
		num2 = -l.FullPoint.CompareTo(r.FullPoint);
		if (num2 != 0)
		{
			return num2;
		}
		CreatureData creatureData = l.CreatureData;
		CreatureData creatureData2 = r.CreatureData;
		if (creatureData != null && creatureData2 != null)
		{
			num2 = -creatureData.Grade.CompareTo(creatureData2.Grade);
			if (num2 != 0)
			{
				return num2;
			}
		}
		num2 = -l.Star.CompareTo(r.Star);
		if (num2 != 0)
		{
			return num2;
		}
		return l.PetIndex.CompareTo(r.PetIndex);
	}
}
