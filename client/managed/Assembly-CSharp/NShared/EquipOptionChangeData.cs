using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class EquipOptionChangeData : IDataKeySelector<EquipOptionChangeDataKey>
{
	[Key(0)]
	public int OptionIndex { get; set; }

	[Key(1)]
	public int BeforeTier { get; set; }

	[Key(2)]
	public int AfterTier { get; set; }

	[Key(3)]
	public int ResultOptionIndex { get; set; }

	EquipOptionChangeDataKey IDataKeySelector<EquipOptionChangeDataKey>.Key => new EquipOptionChangeDataKey(OptionIndex, BeforeTier, AfterTier);

	public override string ToString()
	{
		return "OptionIndex=" + OptionIndex + " ResultOptionIndex = " + ResultOptionIndex;
	}
}
