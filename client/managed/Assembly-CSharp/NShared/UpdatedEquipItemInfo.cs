namespace NShared;

[JsonSerializable(Javascript = true)]
public class UpdatedEquipItemInfo
{
	public int SlotIndex { get; set; }

	public int ItemIndex { get; set; }

	public int Level { get; set; }
}
