namespace NShared;

[JsonSerializable(Javascript = true)]
public class EquipItemBoostInfo
{
	public int SlotIndex { get; set; }

	public int BoostPercent { get; set; }
}
