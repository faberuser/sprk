namespace NShared;

[JsonSerializable(Javascript = true)]
public class EquipItemIdentifiedInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => EquipItemSlotIndex;

	public int EquipItemSlotIndex { get; set; }

	public int Identified { get; set; }

	public int ExtraOptionIndex1 { get; set; }

	public int ExtraOptionStep1 { get; set; }

	public int ExtraOptionRenewCount1 { get; set; }

	public int IsRenewedExtraOption1 { get; set; }

	public int ExtraOptionIndex2 { get; set; }

	public int ExtraOptionStep2 { get; set; }

	public int ExtraOptionRenewCount2 { get; set; }

	public int IsRenewedExtraOption2 { get; set; }
}
