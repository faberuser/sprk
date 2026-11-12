namespace NShared;

public class PlayerAccessoryCostumeInfoKey : Tuple<int, int>
{
	public PlayerAccessoryCostumeInfoKey(int HeroIndex, int AccessoryCostumeIndex)
		: base(HeroIndex, AccessoryCostumeIndex)
	{
	}
}
