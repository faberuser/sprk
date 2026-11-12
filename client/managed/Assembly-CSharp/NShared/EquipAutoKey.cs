using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct EquipAutoKey
{
	[Key(0)]
	public CreatureTagType CreatureTagType;

	[Key(1)]
	public int EquipSlotIndex;

	public EquipAutoKey(CreatureTagType tagType, int slotIndex)
	{
		CreatureTagType = tagType;
		EquipSlotIndex = slotIndex;
	}

	public bool Equals(EquipAutoKey other)
	{
		if (CreatureTagType == other.CreatureTagType)
		{
			return EquipSlotIndex == other.EquipSlotIndex;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(CreatureTagType, EquipSlotIndex);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({CreatureTagType}, {EquipSlotIndex})";
	}

	public static bool operator ==(EquipAutoKey a, EquipAutoKey b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(EquipAutoKey a, EquipAutoKey b)
	{
		return !a.Equals(b);
	}
}
