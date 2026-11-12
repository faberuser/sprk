namespace NShared;

public interface IValanceCreatureFilterInfo : IValanceFilterInfo
{
	SkillAttrType AttrType { get; }

	CreatureTagType CreatureTagType { get; }
}
