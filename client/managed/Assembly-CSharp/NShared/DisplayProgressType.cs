namespace NShared;

[JsonSerializable(Javascript = true)]
public enum DisplayProgressType
{
	None,
	EquipCount,
	ClearNode,
	HeroLevel,
	SkillMaster,
	AllSkillMaster,
	TotalEquipGradeCount
}
