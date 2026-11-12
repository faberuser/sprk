namespace NShared;

public class EquipItemRuneKey : Tuple<int, int>
{
	public EquipItemRuneKey()
		: base(0, 0)
	{
	}

	public EquipItemRuneKey(int equipItemSlotIndex, int runePage)
		: base(equipItemSlotIndex, runePage)
	{
	}
}
