namespace NShared;

public class EquipSlotInfo
{
	public int EquipSlotIndex { get; private set; }

	public EquipItemInfo EquipItemInfo { get; private set; }

	public EquipSlotInfo(int equipSlotIndex, EquipItemInfo equipItemInfo)
	{
		EquipSlotIndex = equipSlotIndex;
		EquipItemInfo = equipItemInfo;
	}

	public void Reset()
	{
		EquipSlotIndex = 0;
		EquipItemInfo = null;
	}
}
