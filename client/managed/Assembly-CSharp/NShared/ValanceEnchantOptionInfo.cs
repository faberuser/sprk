namespace NShared;

[JsonSerializable(Javascript = true)]
public class ValanceEnchantOptionInfo
{
	public int ConsumeItemIndex { get; set; }

	public int EnchantEquipItemSlotIndex { get; set; }

	public int EnchantOptionIndex1 { get; set; }

	public int EnchantOptionStep1 { get; set; }

	public int EnchantOptionIndex2 { get; set; }

	public int EnchantOptionStep2 { get; set; }

	public int EnchantOptionIndex3 { get; set; }

	public int EnchantOptionStep3 { get; set; }
}
