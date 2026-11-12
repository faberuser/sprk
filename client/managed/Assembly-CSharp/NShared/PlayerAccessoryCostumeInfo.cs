namespace NShared;

[JsonSerializable(Javascript = true)]
public class PlayerAccessoryCostumeInfo : IAccountAttrKey<PlayerAccessoryCostumeInfoKey>
{
	PlayerAccessoryCostumeInfoKey IAccountAttrKey<PlayerAccessoryCostumeInfoKey>.Key => new PlayerAccessoryCostumeInfoKey(HeroIndex, AccessoryCostumeIndex);

	public int HeroIndex { get; set; }

	public int AccessoryCostumeIndex { get; set; }

	public string PositionInfo { get; set; }

	public string CreatedTime { get; set; }
}
