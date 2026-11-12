using System;
using MessagePack;

namespace NShared;

[JsonSerializable(Javascript = true)]
public class PetInfo : IAccountAttrKey<int>
{
	[IgnoreMember]
	public CreatureData CreatureData => GameRoot.Instance.CreatureDataContainer.GetData(PetIndex);

	[IgnoreMember]
	[NonDbSchema]
	public bool IsLinePet => CreatureData?.IsLinePet ?? false;

	[IgnoreMember]
	[NonDbSchema]
	public int MaxFood => CreatureData?.MaxFood ?? 0;

	[IgnoreMember]
	public PetTierInteractiveData PetTierInteractiveData
	{
		get
		{
			if (CreatureData == null)
			{
				return null;
			}
			return GameRoot.Instance.PetTierInteractiveDataContainer.GetData(CreatureData.Grade);
		}
	}

	int IAccountAttrKey<int>.Key => PetIndex;

	public int PetIndex { get; set; }

	public sbyte Star { get; set; }

	public string CreatedTime { get; set; }

	public int HappinessPoint { get; set; }

	public string LastPlayTime { get; set; }

	public string LastPatTime { get; set; }

	public string LastFeedTime { get; set; }

	public string MaxRewardedTime { get; set; }

	public int FullPoint { get; set; }

	public PetStatusType Status { get; set; }

	public string EndPenaltyTime { get; set; }

	[NonDbSchema]
	public CreatureGrade Grade
	{
		get
		{
			if (CreatureData == null)
			{
				return CreatureGrade.None;
			}
			if (!Enum.IsDefined(typeof(CreatureGrade), (int)CreatureData.Grade))
			{
				return CreatureGrade.None;
			}
			return (CreatureGrade)Enum.ToObject(typeof(CreatureGrade), CreatureData.Grade);
		}
	}

	public bool CanAwaken()
	{
		CreatureData creatureData = CreatureData;
		if (creatureData == null)
		{
			return false;
		}
		if (Star < 5)
		{
			return creatureData.CreatureGrade == CreatureGrade.Legend;
		}
		return false;
	}
}
