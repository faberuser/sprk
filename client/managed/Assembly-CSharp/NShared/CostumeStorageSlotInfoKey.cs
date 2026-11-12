namespace NShared;

public class CostumeStorageSlotInfoKey : Tuple<int, int>
{
	public CostumeStorageSlotInfoKey(int heroIndex, int costumeStorageSlotIndex)
		: base(heroIndex, costumeStorageSlotIndex)
	{
	}
}
