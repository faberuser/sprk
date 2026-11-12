using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class ValanceEquipFilterData : IDataKeySelector<int>, IValanceEquipFilterInfo, IValanceFilterInfo
{
	int IDataKeySelector<int>.Key => Index;

	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int GroupIndex { get; set; }

	[Key(2)]
	public int SortingOrder { get; set; }

	[Key(3)]
	public string LocalKey { get; set; }

	[Key(4)]
	public EquipItemSetGroupType SetType { get; set; }

	[Key(5)]
	public CreatureTagType CreatureTagType { get; set; }

	[Key(6)]
	public EquipPartType EquipPartType { get; set; }

	[Key(7)]
	public EquipItemSubType EquipItemSubType { get; set; }

	[Key(8)]
	public int DisplayEquipOptionIndex { get; set; }

	[Key(9)]
	public int[] ExtraEquipOptionIndices { get; set; }

	EquipItemSetGroupType IValanceEquipFilterInfo.SetType => SetType;

	bool IValanceEquipFilterInfo.IsCreatureTagTypeArray => false;

	CreatureTagType[] IValanceEquipFilterInfo.CreatureTagTypes => null;

	CreatureTagType IValanceEquipFilterInfo.CreatureTagType => CreatureTagType;

	EquipPartType IValanceEquipFilterInfo.EquipPartType => EquipPartType;

	EquipItemSubType IValanceEquipFilterInfo.EquipItemSubType => EquipItemSubType;

	int IValanceEquipFilterInfo.DisplayEquipOptionIndex => DisplayEquipOptionIndex;
}
