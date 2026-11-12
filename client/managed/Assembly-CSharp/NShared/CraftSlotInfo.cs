namespace NShared;

[JsonSerializable(Javascript = true)]
public class CraftSlotInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => SlotIndex;

	public int SlotIndex { get; set; }

	public int CraftIndex { get; set; }

	public int ItemIndex { get; set; }

	public string CompleteTime { get; set; }

	public int ItemCount { get; set; }
}
