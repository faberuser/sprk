namespace NShared;

public interface IValanceEquipFilterInfo : IValanceFilterInfo
{
	EquipItemSetGroupType SetType { get; }

	bool IsCreatureTagTypeArray { get; }

	CreatureTagType CreatureTagType { get; }

	CreatureTagType[] CreatureTagTypes { get; }

	EquipPartType EquipPartType { get; }

	EquipItemSubType EquipItemSubType { get; }

	int DisplayEquipOptionIndex { get; }
}
