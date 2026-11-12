using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class ValanceCraftData : IDataKeySelector<ValanceCraftDataKey2>, IValanceEquipFilterInfo, IValanceFilterInfo
{
	ValanceCraftDataKey2 IDataKeySelector<ValanceCraftDataKey2>.Key => new ValanceCraftDataKey2(SetType, CreatureTagType, EquipPartType, EquipItemSubType);

	[Key(0)]
	public EquipItemSetGroupType SetType { get; set; }

	[Key(1)]
	public CreatureTagType CreatureTagType { get; set; }

	[Key(2)]
	public EquipPartType EquipPartType { get; set; }

	[Key(3)]
	public EquipItemSubType EquipItemSubType { get; set; }

	[Key(4)]
	public int MaterialItemIndex { get; set; }

	[Key(5)]
	public int MaterialItemCount { get; set; }

	[Key(6)]
	public int ReqGold { get; set; }

	[Key(7)]
	public string ResultItemGroupCode { get; set; }

	[Key(8)]
	public bool NoticeCheck { get; set; }

	[Key(9)]
	public int AdditionalMaterialItemCount { get; set; }

	[Key(10)]
	public int SubMaterialItemIndex { get; set; }

	[Key(11)]
	public int SubMaterialItemCount { get; set; }

	[Key(12)]
	public int[] EquipOptionGroupIndex { get; set; }

	EquipItemSetGroupType IValanceEquipFilterInfo.SetType => SetType;

	bool IValanceEquipFilterInfo.IsCreatureTagTypeArray => false;

	CreatureTagType IValanceEquipFilterInfo.CreatureTagType => CreatureTagType;

	CreatureTagType[] IValanceEquipFilterInfo.CreatureTagTypes => null;

	EquipPartType IValanceEquipFilterInfo.EquipPartType => EquipPartType;

	EquipItemSubType IValanceEquipFilterInfo.EquipItemSubType => EquipItemSubType;

	int IValanceEquipFilterInfo.DisplayEquipOptionIndex => 0;
}
