namespace NShared;

[JsonSerializable(Javascript = true)]
public class PetIncubatorSlotInfo : IAccountAttrKey<int>
{
	int IAccountAttrKey<int>.Key => SlotIndex;

	public int SlotIndex { get; set; }

	public int MountingIndex { get; set; }

	public int SetItemIndex { get; set; }

	public string CompletedTime { get; set; }
}
