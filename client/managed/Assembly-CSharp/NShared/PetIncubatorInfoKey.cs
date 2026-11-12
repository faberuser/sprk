namespace NShared;

public class PetIncubatorInfoKey : Tuple<int, int>
{
	public PetIncubatorInfoKey()
		: base(0, 0)
	{
	}

	public PetIncubatorInfoKey(int slotIndex, int incubatorIndex)
		: base(slotIndex, incubatorIndex)
	{
	}
}
