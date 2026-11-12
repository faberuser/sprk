namespace NShared;

[JsonSerializable(Javascript = true)]
public class ValanceRenewOptionInfo
{
	public int RenewOptionEquipItemSlotIndex { get; set; }

	public int RenewOptionResultIndex { get; set; }

	public int RenewOptionResultStep { get; set; }

	public int RenewOptionSlotIndex { get; set; }
}
