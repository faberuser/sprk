namespace NShared;

[JsonSerializable(Javascript = true)]
public class PetIncubatorInfo : IAccountAttrKey<PetIncubatorInfoKey>
{
	PetIncubatorInfoKey IAccountAttrKey<PetIncubatorInfoKey>.Key => new PetIncubatorInfoKey(SlotIndex, IncubatorIndex);

	public int SlotIndex { get; set; }

	public int IncubatorIndex { get; set; }

	public string CreatedTime { get; set; }
}
