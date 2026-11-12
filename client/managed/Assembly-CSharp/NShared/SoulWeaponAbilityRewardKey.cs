using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct SoulWeaponAbilityRewardKey
{
	[Key(0)]
	public ESoulWeaponAbilityTicketType TicketType;

	[Key(1)]
	public CreatureTagType TagType;

	[Key(2)]
	public int Grade;

	[Key(3)]
	public int ReinforceLevel;

	public SoulWeaponAbilityRewardKey(ESoulWeaponAbilityTicketType ticketType, CreatureTagType tagType, int grade, int reinforceLevel)
	{
		TicketType = ticketType;
		TagType = tagType;
		Grade = grade;
		ReinforceLevel = reinforceLevel;
	}

	public bool Equals(SoulWeaponAbilityRewardKey other)
	{
		if (TicketType == other.TicketType && TagType == other.TagType && Grade == other.Grade)
		{
			return ReinforceLevel == other.ReinforceLevel;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(TicketType, TagType, Grade, ReinforceLevel);
	}

	public override string ToString()
	{
		return $"TicketType={TicketType} TagType={TagType} Grade={Grade} ReinforceLevel={ReinforceLevel}";
	}
}
