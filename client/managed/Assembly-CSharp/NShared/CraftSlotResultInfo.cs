namespace NShared;

[JsonSerializable(Javascript = true)]
public class CraftSlotResultInfo
{
	public int SlotIndex { get; set; }

	public int CraftIndex { get; set; }

	public int ItemIndex { get; set; }

	public int RemainTime { get; set; }

	public int ItemCount { get; set; }
}
