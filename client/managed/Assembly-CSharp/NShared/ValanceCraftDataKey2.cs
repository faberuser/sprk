using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct ValanceCraftDataKey2
{
	[Key(0)]
	public EquipItemSetGroupType SetGroupType;

	[Key(1)]
	public CreatureTagType TagType;

	[Key(2)]
	public EquipPartType PartType;

	[Key(3)]
	public EquipItemSubType ItemSubType;

	public ValanceCraftDataKey2(EquipItemSetGroupType setGroupType, CreatureTagType tagType, EquipPartType partType, EquipItemSubType itemSubType)
	{
		SetGroupType = setGroupType;
		TagType = tagType;
		PartType = partType;
		ItemSubType = itemSubType;
	}

	public bool Equals(ValanceCraftDataKey2 other)
	{
		if (SetGroupType == other.SetGroupType && TagType == other.TagType && PartType == other.PartType)
		{
			return ItemSubType == other.ItemSubType;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(SetGroupType, TagType, PartType, ItemSubType);
	}

	public override string ToString()
	{
		return string.Format("SetGroupType={0} TagType={1} PartType={2} ItemSubType={2}", SetGroupType, TagType, PartType, ItemSubType);
	}
}
