namespace NShared;

[JsonSerializable(Javascript = true)]
public enum EAffixType
{
	None,
	Skill,
	Stat,
	BanRule,
	InfinityStat
}
