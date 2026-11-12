using System;
using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class EquipSlotData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public EquipPartType EquipPartType { get; set; }

	[Key(2)]
	public EquipItemSubType[] EquipItemSubType { get; set; }

	[Key(3)]
	public int GroupIndex { get; set; }

	[Key(4)]
	public bool IgnoreStat { get; set; }

	[Key(5)]
	public bool IgnoreFixedOption { get; set; }

	[Key(6)]
	public bool IgnoreRandomOption { get; set; }

	[Key(7)]
	public bool IgnoreEnchantOption { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public bool NeedCheckSubType()
	{
		if (EquipItemSubType != null)
		{
			return EquipItemSubType.Length != 0;
		}
		return false;
	}

	public bool IsValidSubType(EquipItemSubType subType)
	{
		if (!NeedCheckSubType())
		{
			return true;
		}
		return Array.Exists(EquipItemSubType, (EquipItemSubType element) => element == subType);
	}
}
