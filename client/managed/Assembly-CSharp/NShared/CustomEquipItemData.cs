using MessagePack;

namespace NShared;

[MessagePackObject(false)]
public class CustomEquipItemData : IDataKeySelector<int>
{
	[Key(0)]
	public int Index { get; set; }

	[Key(1)]
	public int SlotIndex { get; set; }

	[Key(2)]
	public int ItemIndex { get; set; }

	[Key(3)]
	public int Star { get; set; }

	[Key(4)]
	public int Level { get; set; }

	[Key(5)]
	public int Exp { get; set; }

	[Key(6)]
	public int OptionIndex1 { get; set; }

	[Key(7)]
	public int OptionStep1 { get; set; }

	[Key(8)]
	public int OptionRenewCount1 { get; set; }

	[Key(9)]
	public int IsRenewedOption1 { get; set; }

	[Key(10)]
	public int OptionIndex2 { get; set; }

	[Key(11)]
	public int OptionStep2 { get; set; }

	[Key(12)]
	public int OptionRenewCount2 { get; set; }

	[Key(13)]
	public int IsRenewedOption2 { get; set; }

	[Key(14)]
	public int OptionIndex3 { get; set; }

	[Key(15)]
	public int OptionStep3 { get; set; }

	[Key(16)]
	public int OptionRenewCount3 { get; set; }

	[Key(17)]
	public int IsRenewedOption3 { get; set; }

	[Key(18)]
	public int OptionIndex4 { get; set; }

	[Key(19)]
	public int OptionStep4 { get; set; }

	[Key(20)]
	public int OptionRenewCount4 { get; set; }

	[Key(21)]
	public int IsRenewedOption4 { get; set; }

	[Key(22)]
	public int OptionIndex5 { get; set; }

	[Key(23)]
	public int OptionStep5 { get; set; }

	[Key(24)]
	public int OptionRenewCount5 { get; set; }

	[Key(25)]
	public int IsRenewedOption5 { get; set; }

	[Key(26)]
	public int RuneSlotCount { get; set; }

	[Key(27)]
	public int RuneItemIndex1 { get; set; }

	[Key(28)]
	public int RuneItemIndex2 { get; set; }

	[Key(29)]
	public int RuneItemIndex3 { get; set; }

	[Key(30)]
	public int UpgradeStarFailBonus { get; set; }

	[Key(31)]
	public byte Locked { get; set; }

	[Key(32)]
	public int EnchantOptionIndex1 { get; set; }

	[Key(33)]
	public int EnchantOptionStep1 { get; set; }

	[Key(34)]
	public int EnchantOptionIndex2 { get; set; }

	[Key(35)]
	public int EnchantOptionStep2 { get; set; }

	[Key(36)]
	public int EnchantOptionIndex3 { get; set; }

	[Key(37)]
	public int EnchantOptionStep3 { get; set; }

	[Key(38)]
	public byte RenewEnchantOptionSlotIndex { get; set; }

	int IDataKeySelector<int>.Key => Index;

	public override string ToString()
	{
		return "Index=" + Index;
	}
}
