using System.Runtime.CompilerServices;
using MessagePack;

namespace NShared;

[MessagePackObject(false)]
[JsonSerializable(Javascript = true)]
public class InventoryExtendData : IDataKeySelector<int>
{
	[CompilerGenerated]
	private int _003CEquipItemExtendCount_003Ek__BackingField;

	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int EquipItemExtendCount
	{
		[CompilerGenerated]
		get
		{
			if (InventoryType == InventoryType.Inventory && _003CEquipItemExtendCount_003Ek__BackingField == 90)
			{
				return 140;
			}
			return _003CEquipItemExtendCount_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			_003CEquipItemExtendCount_003Ek__BackingField = value;
		}
	}

	[Key(2)]
	public int EquipItemExtendPrice { get; set; }

	[Key(3)]
	public int EquipItemExtendSize { get; set; }

	[Key(4)]
	public int[] ProductIndex { get; set; }

	[Key(5)]
	public InventoryType InventoryType { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return "Index=" + Index;
	}
}
