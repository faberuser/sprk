namespace NShared;

[JsonSerializable(Javascript = true)]
public enum RaidBanType
{
	None,
	CreatureTagType,
	SkillAttrType,
	CodeName,
	ClassEnterPermission,
	AttributePermission
}
