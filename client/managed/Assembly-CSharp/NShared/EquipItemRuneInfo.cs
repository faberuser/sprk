namespace NShared;

[JsonSerializable(Javascript = true)]
public class EquipItemRuneInfo : IAccountAttrKey<EquipItemRuneKey>
{
	EquipItemRuneKey IAccountAttrKey<EquipItemRuneKey>.Key => new EquipItemRuneKey(EquipItemSlotIndex, RunePage);

	public int EquipItemSlotIndex { get; set; }

	public int RunePage { get; set; }

	public int RuneItemIndex1 { get; set; }

	public int RuneItemIndex2 { get; set; }

	public int RuneItemIndex3 { get; set; }

	public int GetRuneItemIndex(int slotIndex)
	{
		return slotIndex switch
		{
			1 => RuneItemIndex1, 
			2 => RuneItemIndex2, 
			3 => RuneItemIndex3, 
			_ => 0, 
		};
	}
}
